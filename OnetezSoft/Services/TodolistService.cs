using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

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
          var todoItems = DbTodoItem.GetList(companyId, item.id);
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
    /// Thêm công việc vào trong Todolist
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
          error = "Không thể thêm công việc vào Todolist đã check-out!";
      }
      else
        error = "Không thể thêm công việc vào Todolist của quá khứ!";

      return error;
    }
  }
}
