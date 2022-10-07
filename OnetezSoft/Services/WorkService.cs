using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class WorkService
  {
    /// <summary>
    /// Kiểm tra quyền của nhân viên trong kế hoạch
    /// </summary>
    /// <returns>1: quản lý, 2. thành viên</returns>
    public static int RoleInPlan(WorkPlanModel plan, string userId)
    {
      var member = plan.members.Where(x => x.id == userId).SingleOrDefault();
      if(member != null)
        return member.role;
      return 0; 
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
    /// Xóa thành viên khỏi kế hoạch
    /// </summary>
    public static async Task<bool> RemoveMemberInPlan(string companyId, string planId, string userId)
    {
      var plan = await DbWorkPlan.Get(companyId, planId);
      if(plan != null && !CheckPlanSingleManager(plan, userId))
      {
        plan.members.RemoveAll(x => x.id == userId);
        await DbWorkPlan.Update(companyId, plan);
        return true;
      }
      return false;
    }


    public static async Task<bool> DeleteSection(string companyId, string planId, string sectionId)
    {
      var plan = await DbWorkPlan.Get(companyId, planId);
      if(plan != null)
      {
        // Xóa nhóm công việc
        plan.sections.RemoveAll(x => x.id == sectionId);
        // Cập nhật lại vị trí
        int pos = plan.sections.Count - 1;
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

    /// <summary>
    /// Kiểm tra quyền của nhân viên trong công việc
    /// </summary>
    public static bool RoleEditTask(WorkPlanModel plan, WorkPlanModel.Task task, string userId)
    {
      if(string.IsNullOrEmpty(task.id))
        return true;
      if(RoleInPlan(plan, userId) == 1)
        return true;
      if(task.members.Where(x => x.id == userId).Count() > 0)
        return true;
      return false;
    }

    /// <summary>
    /// /// Xóa tòa bộ dữ liệu kế hoạch
    /// </summary>
    public static async Task DeletePlan(string companyId, string planId)
    {
      // Xóa lịch sử

      // Xóa công việc

      // Xóa kế hoạch
      await DbWorkPlan.Delete(companyId, planId);

      // Gửi thông báo
    }

    #region Dữ liệu cố định

    /// <summary>
    /// Danh sách trạng thái công việc chính
    /// </summary>
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, name = "Todo" });
      list.Add(new() { id = 2, name = "Doing" });
      list.Add(new() { id = 3, name = "Review" });
      list.Add(new() { id = 4, name = "Done" });
      list.Add(new() { id = 5, name = "Cancel" });

      return list;
    }

    /// <summary>
    /// Danh sách trạng thái công việc phụ
    /// </summary>
    public static List<StaticModel> StatusSub()
    {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, name = "Todo" });
      list.Add(new() { id = 2, name = "Doing" });
      list.Add(new() { id = 4, name = "Done" });

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
      return new StaticModel();
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
    
    #endregion 
  }
}