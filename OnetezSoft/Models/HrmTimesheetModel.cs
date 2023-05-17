using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using OnetezSoft.Services;

namespace OnetezSoft.Models;

public class HrmTimesheetModel
{
  /// <summary>ID = yymm + UserID</summary>
  [BsonId]
  public string id { get; set; }

  /// <summary>ID nhân sự</summary>
  public string user { get; set; }

  /// <summary>Tên nhân viên</summary>
  public string fullname { get; set; }

  /// <summary>Trực thuộc công ty</summary>
  public string company_id { get; set; }

  /// <summary>Trực thuộc công ty</summary>
  public string company_name { get; set; }

  /// <summary>Trực thuộc công ty</summary>
  public string department_id { get; set; }

  /// <summary>Trực thuộc công ty</summary>
  public string department_name { get; set; }

  /// <summary>Tháng của bảng công</summary>
  public long month { get; set; }

  /// <summary>Lần cập nhật cuối cùng</summary>
  public long updated { get; set; }

  /// <summary>Đã khóa bảng công</summary>
  public bool locked { get; set; }

  /// <summary>Công chuẩn tháng</summary>
  public double time_total { get; set; }

  /// <summary>Công thực tế: Công tính lương + Công tác + Làm việc từ xa + Làm việc ngoài giờ *3</summary>
  public double time_real
  {
    get
    {
      double result = 0;
      foreach (var item in days)
      {
        if (item.morning != null)
        {
          if (item.morning.type == null || item.morning.type == "D" || item.morning.type == "O")
            result += item.morning.time;
          else if (item.morning.type == "OT")
            result += item.morning.time * 3;
        }
        if (item.afternoon != null)
        {
          if (item.afternoon.type == null || item.afternoon.type == "D" || item.afternoon.type == "O")
            result += item.afternoon.time;
          else if (item.afternoon.type == "OT")
            result += item.afternoon.time * 3;
        }
      }
      return result;
    }
  }

  /// <summary>Công tính lương: Tổng công thực tế + Lễ/Tết + Phép Năm dùng + Cưới Tang</summary>
  public double time_record
  {
    get
    {
      double result = time_real;
      foreach (var item in days)
      {
        if (item.morning != null &&
          (item.morning.type == "L" || item.morning.type == "P" || item.morning.type == "C"))
          result += item.morning.time;
        if (item.afternoon != null &&
          (item.afternoon.type == "L" || item.afternoon.type == "P" || item.afternoon.type == "C"))
          result += item.afternoon.time;
      }
      return result;
    }
  }

  /// <summary>Dữ liệu bảng công trong tháng</summary>
  public List<DayData> days { get; set; } = new();


  /// <summary>Dữ liệu bảng công trong ngày</summary>
  public class DayData
  {
    /// <summary>Ngày chấm công</summary>
    public long day { get; set; }

    /// <summary>Ca sáng</summary>
    public TimeData morning { get; set; }

    /// <summary>Ca chiều</summary>
    public TimeData afternoon { get; set; }
  }

  /// <summary>Dữ liệu chấm công trong ca</summary>
  public class TimeData
  {
    /// <summary>Số công theo cấu hình phân ca</summary>
    public double time { get; set; }

    /// <summary>Kiểu công</summary>
    public string type { get; set; }

    /// <summary>Tên kiểu công</summary>
    public StaticModel type_model { get => HrmService.TimeType(type); }

    /// <summary>Ghi chú khi chỉnh sửa</summary>
    public string note { get; set; }

    /// <summary>Đã chỉnh sửa hay chưa?</summary>
    public bool edited { get; set; }

    /// <summary>Kiểu công trước khi chỉnh sửa</summary>
    public string type_old { get; set; }

    /// <summary>Cảnh báo dữ liệu bất thường: đi trễ, về sớm, sai vị trí</summary>
    public bool warning { get; set; }
  }
}