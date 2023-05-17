using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

public class HrmTimekeepingModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>ID nhân sự</summary>
  public string user { get; set; }

  /// <summary>Ngày chấm công</summary>
  public long date { get; set; }

  /// <summary>Checkin ca sáng</summary>
  public TimeData morning_checkin { get; set; }

  /// <summary>Checkout ca sáng</summary>
  public TimeData morning_checkout { get; set; }

  /// <summary>Checkin ca chiều</summary>
  public TimeData afternoon_checkin { get; set; }

  /// <summary>Checkout ca chiều</summary>
  public TimeData afternoon_checkout { get; set; }


  /// <summary>Dữ liệu thời gian checkin/checkout</summary>
  public class TimeData
  {
    /// <summary>Loại chấm công: checkin/checkout</summary>
    public string time_type { get; set; }

    /// <summary>Số công của ca</summary>
    public double time_work { get; set; }

    /// <summary>Thời gian phân ca</summary>
    public string time_shift { get; set; }

    /// <summary>Thời gian chấm công</summary>
    public string time_active { get; set; }

    /// <summary>Thời gian chấm công so với phân ca: lớn 0. Đi trễ/Về sớm (phút)</summary>
    public long time_difference { get; set; }

    /// <summary>Vị trí chấm công: true. Trong công ty, false. Ngoài công ty</summary>
    public bool in_company { get; set; }

    /// <summary>Hợp lệ hay không: true = đúng giờ hoặc sớm/trễ trong thời gian cho phép</summary>
    public bool is_valid { get; set; }

    /// <summary>Ghi chú</summary>
    public string note { get; set; }

    /// <summary>Loại lý do</summary>
    public string reason { get; set; }

    /// <summary>Hình ảnh</summary>
    public List<string> images { get; set; } = new();
  }
}