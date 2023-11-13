using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using OnetezSoft.Data;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class WorkService
  {
    private static object id;

    /// <summary>
    /// Kiểm tra quyền của nhân viên trong kế hoạch
    /// </summary>
    /// <returns>1: quản lý, 2. thành viên</returns>
    public static int RoleInPlan(WorkPlanModel plan, string userId)
    {
      var member = plan.members.Where(x => x.id == userId).SingleOrDefault();
      if (member != null)
        return member.role;
      return 0;
    }

    /// <summary>
    /// Kiểm tra quyền xem kế hoạch
    /// </summary>
    public static bool ViewInPlan(WorkPlanModel plan, string userId)
    {
      if (!plan.is_private)
        return true;
      if (plan.members.Where(x => x.id == userId).Count() > 0)
        return true;
      return false;
    }

    /// <summary>
    /// Kiểm tra userId có phải là quản lý duy nhất
    /// </summary>
    /// <returns>true: duy nhất, false: không phải duy nhất</returns>
    public static bool CheckPlanSingleManager(WorkPlanModel plan, string userId)
    {
      var check = plan.members.Where(x => x.role == 1 && x.id != userId).Count();
      return check == 0;
    }

    /// <summary>
    /// Thành viên tự rời khỏi kế hoạch
    /// </summary>
    public static async Task<bool> RemoveMemberInPlan(string companyId, string planId, string userId)
    {
      var plan = await DbWorkPlan.Get(companyId, planId);
      if (plan != null && !CheckPlanSingleManager(plan, userId))
      {
        plan.members.RemoveAll(x => x.id == userId);
        await DbWorkPlan.Update(companyId, plan);

        // Gửi thông báo cho QLKH
        foreach (var member in plan.members.Where(x => x.role == 1))
          await DbNotify.ForPlan(companyId, 718, plan.id, null, member.id, userId);

        return true;
      }
      return false;
    }

    /// <summary>
    /// Xóa nhóm công việc
    /// </summary>
    public static async Task<bool> DeleteSection(string companyId, string planId, string sectionId)
    {
      var plan = await DbWorkPlan.Get(companyId, planId);
      if (plan != null)
      {
        // Xóa nhóm công việc
        plan.sections.RemoveAll(x => x.id == sectionId);
        // Cập nhật lại vị trí
        int pos = plan.sections.Count;
        foreach (var item in plan.sections)
        {
          item.pos = pos;
          pos--;
        }
        await DbWorkPlan.Update(companyId, plan);

        // Xóa công viêc trong nhóm này
        var tasks = await DbWorkTask.GetListInPlan(companyId, planId, sectionId);
        foreach (var item in tasks)
          await DbWorkTask.Delete(companyId, item.id);

        return true;
      }
      return false;
    }


    public static async Task<bool> DeleteSheet(string companyId, string planId, string sheetId)
    {
      var plan = await DbWorkPlan.Get(companyId, planId);
      if (plan != null)
      {
        // Xoá sheet
        plan.sheets.RemoveAll(x => x.id == sheetId);
        int newPosition = 1;
        foreach (var sheet in plan.sheets.OrderBy(x => x.possition).ToList())
        {
          sheet.possition = newPosition;
          newPosition++;
        }
        await DbWorkPlan.Update(companyId, plan);
        // Lấy tất cả nhóm công việc thuộc sheet này
        var sections = plan.sections.Where(x => x.sheet == sheetId).ToList();
        foreach (var section in sections)
        {
          // Xoá nhóm công việc
          plan.sections.RemoveAll(x => x.id == section.id);
          await DbWorkPlan.Update(companyId, plan);
          // Xóa công viêc trong nhóm này
          var tasks = await DbWorkTask.GetListInPlan(companyId, planId, section.id);
          foreach (var item in tasks)
            await DbWorkTask.Delete(companyId, item.id);
        }
        return true;
      }

      return false;
    }



    /// <summary>
    /// Xóa tòa bộ dữ liệu kế hoạch
    /// </summary>
    public static async Task DeletePlan(string companyId, string planId)
    {
      // Xóa lịch sử

      // Xóa công việc

      // Xóa kế hoạch
      await DbWorkPlan.Delete(companyId, planId);

      // Gửi thông báo
    }

    /// <summary>
    /// Chuyển trạng thái công việc
    /// </summary>
    public static async Task<string> ChangeTaskStatus(string companyId, UserModel user, WorkPlanModel.Task task, int statusId)
    {
      var status = WorkService.Status(statusId);
      var reviews = task.members.Where(x => x.role == 1).Select(x => x.id).ToList();
      if (statusId != 4)
      {
        // Cập nhật database
        task.status = statusId;
        await DbWorkTask.Update(companyId, task);
        // Lưu lịch sử
        await WorkService.CreateLog(companyId, "Cập nhật trạng thái", status.name, task.plan_id, task.id, user);
        // Thông báo cho người nhận xét
        if (statusId == 3)
        {
          foreach (var memberId in reviews)
            await DbNotify.ForPlan(companyId, 711, task.plan_id, task.id, memberId, user.id);
        }
        return "Đã chuyển trạng thái công việc.";
      }
      else if (statusId == 4)
      {
        // Người nhận xét chuyển trạng thái hoặc không có người nhận xét
        if (reviews.Count == 0 || reviews.Contains(user.id))
        {
          // Cập nhật database
          if (task.status != 4)
            task.date_done = DateTime.Now.Ticks;
          task.status = 4;
          await DbWorkTask.Update(companyId, task);
          // Lưu lịch sử
          await WorkService.CreateLog(companyId, "Cập nhật trạng thái", status.name, task.plan_id, task.id, user);
          // Thông báo cho thành viên
          foreach (var member in task.members)
            await DbNotify.ForPlan(companyId, 710, task.plan_id, task.id, member.id, user.id);
          // Chuyển trạng thái done cho công việc phụ
          var subTasks = await DbWorkTask.GetListInTask(companyId, task.plan_id, task.id);
          foreach (var sub in subTasks)
          {
            if (sub.status != 4)
            {
              sub.status = 4;
              sub.date_done = DateTime.Now.Ticks;
              await DbWorkTask.Update(companyId, sub);
            }
          }

          return "Đã chuyển trạng thái công việc.";
        }
        // Người thực hiện chuyển trạng thái
        else
        {
          // Cập nhật database
          task.status = 3;
          await DbWorkTask.Update(companyId, task);
          // Lưu lịch sử
          await WorkService.CreateLog(companyId, "Cập nhật trạng thái", status.name, task.plan_id, task.id, user);
          // Thông báo cho người nhận xét
          foreach (var memberId in reviews)
            await DbNotify.ForPlan(companyId, 711, task.plan_id, task.id, memberId, user.id);

          return "Chờ người nhận xét review công việc.";
        }
      }
      return string.Empty;
    }

    /// <summary>
    /// Kiểm tra quyền của nhân viên trong công việc
    /// </summary>
    public static bool RoleEditTask(WorkPlanModel plan, WorkPlanModel.Task task, string userId)
    {
      if (string.IsNullOrEmpty(task.id))
        return true;
      if (RoleInPlan(plan, userId) == 1)
        return true;
      if (task.members.Where(x => x.id == userId).Count() > 0)
        return true;
      return false;
    }

    /// <summary>
    /// Cập nhật số lượng công việc phụ
    /// </summary>
    public static async Task UpdateSubTaskCount(string companyId, string taskId, int Count)
    {
      var task = await DbWorkTask.Get(companyId, taskId);
      if (task != null)
      {
        task.sub_task = Count;
        await DbWorkTask.Update(companyId, task);
      }
    }

    /// <summary>
    /// Kiểm tra thời hạn của công việc
    /// </summary>
    public static int CheckDeadline(WorkPlanModel.Task task)
    {
      var now = DateTime.Now;
      if (task.status == 5 || (task.date_start == 0 && task.date_end == 0))
        return 0;
      if (task.status < 4 && now.Ticks <= task.date_end && task.date_end <= now.AddDays(1).Ticks)
        return 1;
      else if (task.date_end < task.date_done || task.date_end < now.Ticks && task.status < 4)
        return 2;
      else
        return 0;
    }

    /// <summary>
    /// Hiển thị label sắp hết hạn, trễ hạn
    /// </summary>
    public static StaticModel TaskDeadline(WorkPlanModel.Task task)
    {
      var check = CheckDeadline(task);
      if (check == 1)
        return new StaticModel() { id = 1, name = "Sắp hết hạn", color = "#BCB51F" };
      else if (check == 2)
        return new StaticModel() { id = 2, name = "Trễ hạn", color = "#FF5449" };
      else
        return null;
    }

    /// <summary>
    /// Tính thống kê từ danh sách công việc
    /// </summary>
    public static WorkPlanModel.Report ReportTasks(List<WorkPlanModel.Task> tasks)
    {
      var result = new WorkPlanModel.Report();
      result.total = tasks.Where(x => x.status <= 4).Count();
      result.done = tasks.Where(x => x.status == 4).Count();
      result.ontime = tasks.Where(x => x.status == 4 && x.date_end >= x.date_done).Count();
      result.late = tasks.Where(x => x.date_end < x.date_done ||
        (x.date_end < DateTime.Now.Ticks && x.status < 4)).Count();

      return result;
    }

    /// <summary>
    /// Tạo lịch sử cập nhật
    /// </summary>
    public static async Task CreateLog(string companyId, string name, string detail, string plan, string task, UserModel user)
    {
      var model = new WorkLogModel()
      {
        name = name,
        detail = detail,
        plan = plan,
        task = task,
        user = UserService.ConvertToMember(user)
      };
      await DbWorkLog.Create(companyId, model);
    }

    /// <summary>
    /// Tạo lịch sử chỉnh sửa khi cập nhật người tham gia
    /// </summary>
    public static async Task LogTaskMembers(string companyId, WorkPlanModel.Task old, WorkPlanModel.Task task, UserModel userEdit, List<UserModel> userList)
    {
      if (old != null)
      {
        // Các thành viên mới thêm vào
        var addList = new List<string>();
        foreach (var item in task.members)
        {
          if (old.members.Where(x => x.id == item.id).Count() == 0)
          {
            var user = UserService.GetUser(userList, item.id);
            if (user != null)
            {
              addList.Add(user.FullName);
              if (string.IsNullOrEmpty(task.parent_id))
                await DbNotify.ForPlan(companyId, 713, task.plan_id, task.id, user.id, userEdit.id);
              else
                await DbNotify.ForPlan(companyId, 715, task.plan_id, task.id, user.id, userEdit.id);
            }
          }
        }
        if (addList.Count > 0
          && string.IsNullOrEmpty(task.parent_id))
          await CreateLog(companyId, "Thêm người tham gia",
            String.Join(", ", addList), task.plan_id, task.id, userEdit);

        // Các thành viên đã xóa
        var removeList = new List<string>();
        foreach (var item in old.members)
        {
          if (task.members.Where(x => x.id == item.id).Count() == 0)
          {
            var user = UserService.GetUser(userList, item.id);
            if (user != null)
            {
              removeList.Add(user.FullName);
              if (string.IsNullOrEmpty(task.parent_id))
                await DbNotify.ForPlan(companyId, 714, task.plan_id, task.id, user.id, userEdit.id);
              else
                await DbNotify.ForPlan(companyId, 716, task.plan_id, task.id, user.id, userEdit.id);
            }
          }
        }
        if (removeList.Count > 0 && string.IsNullOrEmpty(task.parent_id))
          await WorkService.CreateLog(companyId, "Xóa người tham gia",
            String.Join(", ", removeList), task.plan_id, task.id, userEdit);
      }
      else
      {
        // Các thành viên mới thêm vào
        var addList = new List<string>();
        foreach (var item in task.members.Where(x => x.id != userEdit.id))
        {
          var user = UserService.GetUser(userList, item.id);
          if (user != null)
          {
            addList.Add(user.FullName);
            if (string.IsNullOrEmpty(task.parent_id))
              await DbNotify.ForPlan(companyId, 713, task.plan_id, task.id, user.id, userEdit.id);
            else
              await DbNotify.ForPlan(companyId, 715, task.plan_id, task.id, user.id, userEdit.id);
          }
        }
        if (addList.Count > 0 && string.IsNullOrEmpty(task.parent_id))
          await WorkService.CreateLog(companyId, "Thêm người tham gia",
            String.Join(", ", addList), task.plan_id, task.id, userEdit);
      }
    }

    public static async Task LogTaskMembers(string companyId, WorkPlanModel.Task old, WorkPlanModel.Task task, UserModel userEdit, List<MemberModel> userList)
    {
      if (old != null)
      {
        // Các thành viên mới thêm vào
        var addList = new List<string>();
        foreach (var item in task.members)
        {
          if (old.members.Where(x => x.id == item.id).Count() == 0)
          {
            var user = UserService.GetMember(userList, item.id);
            if (user != null)
            {
              addList.Add(user.name);
              if (string.IsNullOrEmpty(task.parent_id))
                await DbNotify.ForPlan(companyId, 713, task.plan_id, task.id, user.id, userEdit.id);
              else
                await DbNotify.ForPlan(companyId, 715, task.plan_id, task.id, user.id, userEdit.id);
            }
          }
        }
        if (addList.Count > 0
          && string.IsNullOrEmpty(task.parent_id))
          await CreateLog(companyId, "Thêm người tham gia",
            String.Join(", ", addList), task.plan_id, task.id, userEdit);

        // Các thành viên đã xóa
        var removeList = new List<string>();
        foreach (var item in old.members)
        {
          if (task.members.Where(x => x.id == item.id).Count() == 0)
          {
            var user = UserService.GetMember(userList, item.id);
            if (user != null)
            {
              removeList.Add(user.name);
              if (string.IsNullOrEmpty(task.parent_id))
                await DbNotify.ForPlan(companyId, 714, task.plan_id, task.id, user.id, userEdit.id);
              else
                await DbNotify.ForPlan(companyId, 716, task.plan_id, task.id, user.id, userEdit.id);
            }
          }
        }
        if (removeList.Count > 0 && string.IsNullOrEmpty(task.parent_id))
          await WorkService.CreateLog(companyId, "Xóa người tham gia",
            String.Join(", ", removeList), task.plan_id, task.id, userEdit);
      }
      else
      {
        // Các thành viên mới thêm vào
        var addList = new List<string>();
        foreach (var item in task.members.Where(x => x.id != userEdit.id))
        {
          var user = UserService.GetMember(userList, item.id);
          if (user != null)
          {
            addList.Add(user.name);
            if (string.IsNullOrEmpty(task.parent_id))
              await DbNotify.ForPlan(companyId, 713, task.plan_id, task.id, user.id, userEdit.id);
            else
              await DbNotify.ForPlan(companyId, 715, task.plan_id, task.id, user.id, userEdit.id);
          }
        }
        if (addList.Count > 0 && string.IsNullOrEmpty(task.parent_id))
          await WorkService.CreateLog(companyId, "Thêm người tham gia",
            String.Join(", ", addList), task.plan_id, task.id, userEdit);
      }
    }
    public static async Task HandleSyncPlan(List<string> companys)
    {
      foreach (string id in companys)
      {
        // Lấy tất cả kế hoạch của công ty này
        List<WorkPlanModel> plans = await DbWorkPlan.GetAll(id);
        // Cập nhật lại theo sheet
        foreach (WorkPlanModel plan in plans)
        {
          if (plan.sheets == null || plan.sheets.Count == 0)
          {
            string idSheet = Mongo.RandomId();
            // Tạo sheet
            plan.sheets = new() { new() { id = idSheet, name = "Sheet 1", possition = 1 } };
            // Nếu đã có nhóm công việc
            if (plan.sections.Any())
            {
              foreach (WorkPlanModel.Section section in plan.sections)
              {
                section.sheet = idSheet;
              }
            }
            // Cập nhật lại kế hoạch
            await DbWorkPlan.Update(id, plan);
            // Cập nhật lại state
            await DbMainCompany.ChangeStateSync(id);
          }
          else
          {
            if (plan.sections.Any(x => x.sheet.IsEmpty()))
            {
              var emptySections = plan.sections.Where(x => x.sheet.IsEmpty()).ToList();
              emptySections.ForEach(x => x.sheet = plan.sheets.FirstOrDefault().id);
              // Cập nhật lại kế hoạch
              await DbWorkPlan.Update(id, plan);
              // Cập nhật lại state
              await DbMainCompany.ChangeStateSync(id);
            }
          }
        }
      }
    }

    // Hàm restore nhóm công việc bị mất
    public static async Task RestoreSectionPlan(string companyId, string planId)
    {
      WorkPlanModel plan = await DbWorkPlan.Get(companyId, planId);

      if (plan != null)
      {
        string idSheet = Mongo.RandomId();
        int countSheet = plan.sheets.Count;

        plan.sheets.Add(new WorkPlanModel.Sheet { id = idSheet, isDefault = false, name = "Sheet không tiêu đề", possition = countSheet++ });
        // Lấy tất cả công việc + nhóm công việc + công việc phụ thuộc kế hoạch này
        List<WorkPlanModel.Task> tasks = await DbWorkTask.GetListTaskByPlan(companyId, planId);
        // Nếu có tasks
        if (tasks.Any())
        {
          // Lấy công việc
          var parents = tasks.Where(x => x.parent_id == null);
          // Lấy nhóm công việc
          foreach (var item in parents)
          {
            // Nếu nhóm công việc khác null
            if (!string.IsNullOrEmpty(item.section_id))
            {
              // Kiểm tra plan hiện tại có nhóm công việc này hay không?
              var isSection = plan.sections.FirstOrDefault(x => x.id == item.section_id);
              int countSections = plan.sections.Count;
              // Nếu không có
              if (isSection == null)
                plan.sections.Add(new WorkPlanModel.Section { id = item.section_id, name = "Khôi phục nhóm công việc", pos = countSheet++, sheet = idSheet });
              else
                continue;
            }
          }

        }

        await DbWorkPlan.Update(companyId, plan);
      }

    }


    #region Các hàm convert
    public static MemberModel ConvertToMember(List<MemberModel> list, WorkPlanModel.Member member)
    {
      return list.FirstOrDefault(x => x.id == member.id);
    }

    public static UserModel ConverToUser(List<UserModel> list, WorkPlanModel.Member member)
    {
      return list.FirstOrDefault(x => x.id == member.id);
    }
    #endregion

    #region Dữ liệu cố định

    /// <summary>
    /// Danh sách trạng thái kế hoạch
    /// </summary>
    public static List<StaticModel> StatusPlan()
    {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, name = "Đang diễn ra", icon = "show_chart" });
      list.Add(new() { id = 2, name = "Đã hoàn thành", icon = "done_all" });

      return list;
    }

    /// <summary>
    /// Chi tiết trạng thái
    /// </summary>
    public static StaticModel StatusPlan(int id)
    {
      var result = StatusPlan().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel();
    }

    /// <summary>
    /// Danh sách trạng thái công việc chính
    /// </summary>
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, name = "Todo", color = "is-grey" });
      list.Add(new() { id = 2, name = "Doing", color = "is-link" });
      list.Add(new() { id = 3, name = "Review", color = "is-info" });
      list.Add(new() { id = 4, name = "Done", color = "is-success" });
      list.Add(new() { id = 5, name = "Cancel", color = "is-dark" });

      return list;
    }

    /// <summary>
    /// Danh sách trạng thái công việc phụ
    /// </summary>
    public static List<StaticModel> StatusSub()
    {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, name = "Todo", color = "is-light" });
      list.Add(new() { id = 2, name = "Doing", color = "is-link" });
      list.Add(new() { id = 4, name = "Done", color = "is-success" });

      return list;
    }

    /// <summary>
    /// Chi tiết trạng thái
    /// </summary>
    public static StaticModel Status(int id)
    {
      var result = Status().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel();
    }


    /// <summary>
    /// Danh sách độ ưu tiên
    /// </summary>
    public static List<StaticModel> Priority()
    {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, name = "Thấp", color = "#8990A5" });
      list.Add(new() { id = 2, name = "Trung bình", color = "#6B8FE0" });
      list.Add(new() { id = 3, name = "Quan trọng", color = "#BCB51F" });
      list.Add(new() { id = 4, name = "Khẩn cấp", color = "#FF5449" });

      return list;
    }

    /// <summary>
    /// Chi tiết độ ưu tiên
    /// </summary>
    public static StaticModel Priority(int id)
    {
      var result = Priority().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel() { id = id, color = "#dddddd" };
    }


    /// <summary>
    /// Danh sách quyền trong kế hoạch
    /// </summary>
    public static List<StaticModel> RolePlan()
    {
      var list = new List<StaticModel>();
      list.Add(new() { id = 1, name = "Quản lý" });
      list.Add(new() { id = 2, name = "Thành viên" });
      return list;
    }

    /// <summary>
    /// Chi tiết quyền trong kế hoạch
    /// </summary>
    public static StaticModel RolePlan(int id)
    {
      var result = RolePlan().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel();
    }


    /// <summary>
    /// Danh sách quyền trong công việc
    /// </summary>
    public static List<StaticModel> RoleTask()
    {
      var list = new List<StaticModel>();
      list.Add(new() { id = 1, name = "Nhận xét" });
      list.Add(new() { id = 2, name = "Thực hiện" });
      return list;
    }

    /// <summary>
    /// Chi tiết quyền trong công việc
    /// </summary>
    public static StaticModel RoleTask(int id)
    {
      var result = RoleTask().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel();
    }

    public static List<StaticModel> Duration()
    {
      var list = new List<StaticModel>();
      list.Add(new() { id = 0, name = "Thời hạn" });
      list.Add(new() { id = 1, name = "Sắp hết hạn" });
      list.Add(new() { id = 2, name = "Trễ hạn" });
      return list;
    }

    public static StaticModel Duration(int id)
    {
      var result = Duration().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel();
    }



    #endregion
  }
}