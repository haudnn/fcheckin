using OnetezSoft.Data;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class TodolistService
  {
    // <summary>
    /// Hàm tự động xác nhận Todolist
    /// </summary>
    /// <param name="database">Dữ liệu Todolist</param>
    /// <param name="time_confirm">Thời gian tự động xác nhận</param>
    public static async Task AutoConfirm(List<TodolistModel> database, string time_confirm, string companyId)
    {
      var confirmList = database.Where(x => string.IsNullOrEmpty(x.user_confirm)).ToList();
      var timeConfirm = Convert.ToDateTime(DateTime.Now.ToShortDateString() + $", {time_confirm}:00");
      var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());

      foreach (var item in confirmList)
      {
        bool autoConfirm = false;

        if (item.date < today.Ticks)
          autoConfirm = true;
        else if (item.date == today.Ticks && DateTime.Now > timeConfirm)
          autoConfirm = true;

        if (autoConfirm)
        {
          var todoItems = await DbTodoItem.GetList(companyId, item.id);
          foreach (var todo in todoItems)
          {
            todo.confirm = true;
            await DbTodoItem.Update(companyId, todo);
          }
          item.date_confirm = DateTime.Now.Ticks;
          item.user_confirm = "auto";
          await DbTodolist.Update(companyId, item);
        }
      }
    }


    /// <summary>
    /// Lấy Todolist theo ngày
    /// </summary>
    public static async Task<TodolistModel> GetTodoList(string companyId, string userId, long day)
    {
      // Lấy Todolist
      var todolist = await DbTodolist.GetbyDay(companyId, userId, new DateTime(day));
      if (todolist == null)
      {
        todolist = new TodolistModel();
        todolist.date = day;
        todolist.user_create = userId;
        await DbTodolist.Create(companyId, todolist);
      }

      return todolist;
    }

    /// <summary>
    /// Thêm công việc vào Todolist
    /// </summary>
    public static async Task<TodolistModel> AddTodoItem(string companyId, string userId, TodolistModel.Todo todo)
    {
      // Lấy Todolist
      var todolist = await GetTodoList(companyId, userId, todo.date);

      // Thêm công việc
      todo.id = Mongo.RandomId();
      todo.status = 1;
      todo.confirm = false;
      todo.date = todolist.date;
      todo.user = todolist.user_create;
      todo.todolist = todolist.id;
      await DbTodoItem.Create(companyId, todo);

      // Cập nhật Todolist
      await DbTodolist.Update(companyId, todolist);

      return todolist;
    }

    /// <summary>
    /// Chấp nhận công việc vào trong Todolist
    /// </summary>
    public static async Task<string> AddTodoItem(string companyId, string userId, long day, TodolistModel.Todo todo)
    {
      string error = string.Empty;
      if (day >= DateTime.Today.Ticks)
      {
        // Lấy Todolist theo ngày
        var todolist = await GetTodoList(companyId, userId, day);

        // Thêm công việc
        if (todolist.status < 3)
        {
          todo.status = 1;
          todo.date = todolist.date;
          todo.user = todolist.user_create;
          todo.todolist = todolist.id;
          await DbTodoItem.Update(companyId, todo);
          await DbTodolist.Update(companyId, todolist);
        }
        else
          error = "Bạn không thể xác nhận công việc này vì đã check out Todolist ngày " + string.Format("{0:dd/MM/yyyy}", new DateTime(day));
      }
      else
        error = "Không thể thêm công việc vào Todolist của ngày đã qua!";

      return error;
    }

    /// <summary>
    /// Thêm công việc vào Todolist
    /// </summary>
    public static async Task<TodolistModel> AddTodoItem(string companyId, string userId, DateTime date, TodolistModel.Todo todo)
    {
      // Lấy Todolist
      var todolist = await GetTodoList(companyId, userId, date.Ticks);

      // Thêm công việc
      todo.status = 1;
      todo.confirm = false;
      todo.date = todolist.date;
      todo.user = todolist.user_create;
      todo.todolist = todolist.id;
      await DbTodoItem.Create(companyId, todo);

      // Cập nhật Todolist
      await DbTodolist.Update(companyId, todolist);

      return todolist;
    }

    // Sort theo đầu việc
    public static List<TodolistModel.Todo> SortByStatus(List<TodolistModel.Todo> items)
    {
      var customOrder = new Dictionary<int, int>
      {
        { 3, 0 }, // doing
        { 1, 1 }, // todo
        { 2, 2 }, // pending
        { 4, 3 }, // done
        { 5, 4 }  // cancel
      };
      return items.OrderBy(item =>
      {
        if (customOrder.TryGetValue(item.status, out var order))
        {
          return order;
        }
        return int.MaxValue;
      }).ToList();
    }
    // sort theo thứ tự
    public static List<StaticModel> SortStatusPossition(List<StaticModel> statusList)
    {
      var customOrder = new Dictionary<int, int>
    {
        { 1, 1 }, // todo
        { 3, 2 }, // doing
        { 4, 3 }, // done
        { 2, 4 }, // pending
        { 5, 5 }  // cancel
    };

      return statusList.OrderBy(item =>
      {
        if (customOrder.TryGetValue(item.id, out var order))
        {
          return order;
        }
        return int.MaxValue;
      }).ToList();
    }

    // Kiểm tra quyền xem todolist theo quyền
    public static bool CheckIfAllowToView(int rule, UserModel userToCheck, UserModel user, List<DepartmentModel> all)
    {
      if (rule == 1 || userToCheck.id == user.id)
      {
        return true;
      }
      else
      {
        // Kiểm tra với danh sách phòng ban là quản lý trong đó
        var result = false;

        var managerDepart = all.Where(x => user.departments_id.Contains(x.id) && (x.manager == user.id || x.deputy == user.id));

        if (managerDepart.Count() == 0)
        {
          result = false;
        }
        else
        {
          foreach (var depart in managerDepart)
          {
            var childs = all.Where(x => x.parent == depart.id).ToList();
            if (CheckIsInDepart(userToCheck, depart, childs, all))
            {
              result = true;
            }
          }
          result = false;
        }

        // Kiểm tra với phòng ban không thuộc quyền quản lý
        var directDepart = all.Where(x => user.departments_id.Contains(x.id) && x.manager != user.id && x.deputy != user.id);

        directDepart = directDepart.OrderBy(x => x.pos).GroupBy(x => x.pos).LastOrDefault();

        if (directDepart.Count() == 0)
        {
          result = false;
        }
        else
        {
          var listId = directDepart.Select(x => x.id);
          if (userToCheck.departments_id.Any(listId.Contains))
          {
            result = true;
          }
        }

        return result;
      }
    }

    // Kiểm tra quyền xem todolist theo quyền
    public static bool CheckIfAllowToView(int rule, MemberModel userToCheck, MemberModel user, List<DepartmentModel> all)
    {
      if (rule == 1 || userToCheck.id == user.id)
      {
        return true;
      }
      else
      {
        // Kiểm tra với danh sách phòng ban là quản lý trong đó
        var result = false;

        var managerDepart = all.Where(x => user.departments_id.Contains(x.id) && (x.manager == user.id || x.deputy == user.id));

        if (managerDepart.Count() == 0)
        {
          result = false;
        }
        else
        {
          foreach (var depart in managerDepart)
          {
            var childs = all.Where(x => x.parent == depart.id).ToList();
            if (CheckIsInDepart(userToCheck, depart, childs, all))
            {
              result = true;
            }
          }
          result = false;
        }

        // Kiểm tra với phòng ban không thuộc quyền quản lý
        var directDepart = all.Where(x => user.departments_id.Contains(x.id) && x.manager != user.id && x.deputy != user.id);

        directDepart = directDepart.OrderBy(x => x.pos).GroupBy(x => x.pos).LastOrDefault();

        if (directDepart.Count() == 0)
        {
          result = false;
        }
        else
        {
          var listId = directDepart.Select(x => x.id);
          if (userToCheck.departments_id.Any(listId.Contains))
          {
            result = true;
          }
        }

        return result;
      }
    }

    public static bool CheckIsInDepart(UserModel userToCheck, DepartmentModel parent, List<DepartmentModel> childs, List<DepartmentModel> all)
    {
      if (parent == null || childs.Count == 0)
      {
        return false;
      }
      else
      {
        foreach (var child in childs)
        {
          if (userToCheck.departments_id.Contains(child.id))
          {
            return true;
          }
          else
          {
            var subChilds = all.Where(x => x.parent == child.id).ToList();
            if (CheckIsInDepart(userToCheck, child, subChilds, all))
            {
              return true;
            }
          }
        }
        return false;
      }
    }

    public static bool CheckIsInDepart(MemberModel userToCheck, DepartmentModel parent, List<DepartmentModel> childs, List<DepartmentModel> all)
    {
      if (parent == null || childs.Count == 0)
      {
        return false;
      }
      else
      {
        foreach (var child in childs)
        {
          if (userToCheck.departments_id.Contains(child.id))
          {
            return true;
          }
          else
          {
            var subChilds = all.Where(x => x.parent == child.id).ToList();
            if (CheckIsInDepart(userToCheck, child, subChilds, all))
            {
              return true;
            }
          }
        }
        return false;
      }
    }


    /// <summary>
    /// Handle loop 
    /// </summary>
    /// <param name="parentTodo"></param>
    /// <param name="companyId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>

    public static async Task<bool> HandleCreateTodoLoop(TodolistModel.Todo parentTodo, string companyId, string userId)
    {
      DateTime startDate = new DateTime(parentTodo.date).AddDays(1);
      DateTime endDate = new(parentTodo.dateEnd_cycle);
      int cycle = parentTodo.cycle;

      for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
      {
        var todoList = await DbTodolist.GetbyDay(companyId, userId, date);
        if (todoList == null)
        {
          var modelTodoList = new TodolistModel
          {
            date = date.Ticks,
            user_create = userId
          };

          var resultTodoList = await DbTodolist.Create(companyId, modelTodoList);

          await HandleLogicLoop(parentTodo, companyId, userId, resultTodoList.id, date, cycle);
          await DbTodolist.Update(companyId, resultTodoList);
        }
        // Nếu đã có todoList và chưa out
        else if (todoList.status != 3)
        {
          await HandleLogicLoop(parentTodo, companyId, userId, todoList.id, date, cycle);
          await DbTodolist.Update(companyId, todoList);
        }
      }
      return true;

    }


    /// <summary>
    /// Xử lý để lấy thông tin lặp lại
    /// </summary>
    public static string GetInfoLoop(TodolistModel.Todo todo)
    {
      string result = string.Empty;
      switch (todo.cycle)
      {
        case 1:
          result = "Hằng ngày";
          break;
        case 2:
          result = "Hằng tuần vào";
          result += " " + string.Join(", ", todo.weeks.Select(week => Handled.Shared.ConvertPosToWeekdays(week)));
          break;
        case 3:
          result = "Hằng tháng vào ngày ";
          result += todo.option == 1 ? new DateTime(todo.date).Day : Handled.Shared.ConvertWeekdays(new DateTime(todo.date), true);
          break;
        case 4:
          result = "Hằng năm";
          break;
      }
      return result;
    }


    /// <summary>
    /// Hàm xử lý xoá những todolist thuộc chu kỳ lặp lại chỉ chạy với nút xoá
    /// Xoá ngày hiện tại || Xoá ngày hiện tại và tương lai
    /// </summary>

    public static async Task<bool> HandleRemoveTodoLoop(TodolistModel.Todo parentTodo, string companyId, string userId)
    {
      // Xử lý trong tương lai
      DateTime startDate = new DateTime(parentTodo.date).AddDays(1);
      DateTime endDate = new(parentTodo.dateEnd_cycle);
      // Lấy theo chu kỳ
      for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
      {
        var todoList = await DbTodolist.GetbyDay(companyId, userId, date);
        if (todoList != null)
        {

          // chưa checkout hoặc chưa xác nhận
          if (todoList.status != 3 && todoList.user_confirm != "auto")
          {
            var todoItems = await DbTodoItem.GetList(companyId, todoList.id);
            if (todoItems != null)
            {
              var todoItemsParent = todoItems.Where(x => x.parentId == parentTodo.parentId && !x.confirm).ToList();
              foreach (var item in todoItemsParent)
              {
                await DbTodoItem.Delete(companyId, item.id);
                await DbTodolist.Update(companyId, todoList);
              }
            }
          }
        }
      }
      return true;
    }

    /// <summary>  Hàm xử lý cập nhật những công việc thuộc chu kỳ lặp lại </summary>

    public static async Task<bool> HandleEditTodoLoop(TodolistModel.Todo parentTodo, string companyId, string userId)
    {
      DateTime startDate = new DateTime(parentTodo.date).AddDays(1);
      DateTime endDate = new(parentTodo.dateEnd_cycle);

      for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
      {
        var todoList = await DbTodolist.GetbyDay(companyId, userId, date);
        if (todoList != null)
        {
          var todoItems = await DbTodoItem.GetList(companyId, todoList.id);
          if (todoItems != null)
          {
            var item = todoItems.FirstOrDefault(x => x.parentId == parentTodo.parentId);
            if (item != null)
            {
              await EditTodoItem(parentTodo, item, companyId);
              await DbTodolist.Update(companyId, todoList);
            }
          }
        }
      }
      return true;
    }

    /// <summary> Xử lý create todo </summary>
    public static async Task CreateTodoItem(TodolistModel.Todo parentTodo, string companyId, string userId, string todoListId, long dateTicks)
    {
      var modelTodoItems = new TodolistModel.Todo
      {
        user = userId,
        date = dateTicks,
        todolist = todoListId,
        name = parentTodo.name,
        detail = parentTodo.detail,
        result = parentTodo.result,
        start = parentTodo.start,
        end = parentTodo.end,
        type = parentTodo.type,
        level = parentTodo.level,
        status = 1,
        confirm = false,
        plan_task = parentTodo.plan_task,
        parentId = parentTodo.id,
        is_loop = parentTodo.is_loop,
        cycle = parentTodo.cycle,
        option = parentTodo.option,
        dateEnd_cycle = parentTodo.dateEnd_cycle,
        assign_user = parentTodo.assign_user,
        assign_status = parentTodo.assign_status,
        weeks = parentTodo.weeks,
      };
      await DbTodoItem.Create(companyId, modelTodoItems);
    }


    /// <summary> Xử lý Edit todo </summary>
    public static async Task EditTodoItem(TodolistModel.Todo parentTodo, TodolistModel.Todo currentTodo, string companyId)
    {
      currentTodo.name = parentTodo.name;
      currentTodo.detail = parentTodo.detail;
      currentTodo.result = parentTodo.result;
      currentTodo.start = parentTodo.start;
      currentTodo.end = parentTodo.end;
      currentTodo.level = parentTodo.level;
      currentTodo.dateEnd_cycle = parentTodo.date;
      await DbTodoItem.Update(companyId, currentTodo);
    }

    /// <summary> Hàm xử lý logic loop todolist </summary>
    public static async Task HandleLogicLoop(TodolistModel.Todo parentTodo, string companyId, string userId, string todoListId, DateTime date, int cycle)
    {
      bool shouldCreateTodo;

      // Lặp lại theo ngày
      if (cycle == 1)
        shouldCreateTodo = true;

      // Lặp lại theo tuần
      else if (cycle == 2)
      {
        var loopWeek = parentTodo.weeks;
        int weekday = date.DayOfWeek == 0 ? 8 : (int)date.DayOfWeek + 1;
        shouldCreateTodo = loopWeek.Contains(weekday);
      }

      // Lặp lại theo tháng
      else if (cycle == 3)
        shouldCreateTodo = parentTodo.option == 1 ? new DateTime(parentTodo.date).Day == date.Day : (int)date.DayOfWeek == (int)new DateTime(parentTodo.date).DayOfWeek;

      // Lặp lại theo năm
      else
        shouldCreateTodo = parentTodo.date == date.AddYears(-1).Ticks;

      if (shouldCreateTodo)
        await CreateTodoItem(parentTodo, companyId, userId, todoListId, date.Ticks);
    }
  }
}