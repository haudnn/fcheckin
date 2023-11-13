using System.Collections.Generic;
using System.Linq;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class TransactionService
  {


    #region Dữ liệu cố định


    /// <summary>
    /// Danh sách trạng thái
    /// </summary>
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Chờ duyệt",
        color = "",
        icon = "hourglass_top"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Đã duyệt",
        color = "has-text-success",
        icon = "done"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Từ chối",
        color = "has-text-danger",
        icon = "close"
      });

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
    /// Danh sách loại giao dịch
    /// </summary>
    public static List<StaticModel> Type()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Nạp tiền",
        color = "",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Hệ thống nạp/rút",
        color = "",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Thanh toán",
        color = "",
      });

      return list;
    }

    /// <summary>
    /// Chi tiết loại giao dịch
    /// </summary>
    public static StaticModel Type(int id)
    {
      var result = Type().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      return new StaticModel();
    }

    #endregion
  }
}