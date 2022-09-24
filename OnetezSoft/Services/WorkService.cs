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
    #region Dữ liệu cố định

    /// <summary>
    /// Danh sách trạng thái
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
    
    #endregion 
  }
}