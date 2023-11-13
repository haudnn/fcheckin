using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmTimekeepingModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>ID nhân sự</summary>
  public string user { get; set; }

  /// <summary>Ngày chấm công</summary>
  public long date { get; set; }

  /// <summary>Danh sách ca làm được phân cho nhân viên trong ngày</summary>
  public List<HrmWorkShiftModel> shifts { get; set; } = new();

  /// <summary>Tracking thời gian chấm công của 1 ngày</summary>
  public List<TimeData> time_tracking { get; set; } = new();


  /// <summary>Dữ liệu thời gian checkin/checkout</summary>
  public class TimeData
  {

    /// <summary>ID ca làm</summary>
    public string time_id { get; set; }

    /// <summary>Tên ca làm</summary>
    public string time_name { get; set; }

    /// <summary>ID checkin</summary>
    public string checkin_id { get; set; }

    /// <summary>Loại chấm công: checkin/checkout</summary>
    public string time_type { get; set; }

    /// <summary>Số công của ca</summary>
    public double time_work { get; set; }

    /// <summary>Thời gian phân ca</summary>
    public string time_shift { get; set; }

    /// <summary>Thời gian chấm công</summary>
    public string time_active { get; set; }

    /// <summary>Thời gian chấm công</summary>
    public long time_active_tick { get; set; }

    /// <summary>Thời gian chấm công so với phân ca: lớn 0. Đi trễ/Về sớm (phút)</summary>
    public long time_difference { get; set; }

    /// <summary>Vị trí chấm công: true. Trong công ty, false. Ngoài công ty</summary>
    public bool in_company { get; set; }

    /// <summary>chế độ hybird</summary>
    public bool is_hybrid { get; set; }

    public Location location { get; set; }

    /// <summary>Hợp lệ hay không: true = đúng giờ hoặc sớm/trễ trong thời gian cho phép</summary>
    public bool is_valid { get; set; }

    /// <summary>Ghi chú</summary>
    public string note { get; set; }

    /// <summary>Loại lý do</summary>
    public string reason { get; set; }

    /// <summary>Hình ảnh</summary>
    public List<string> images { get; set; } = new();

    /// <summary>Ca OT: true = ca OT, false = Không phải ca OT</summary>
    public bool is_ot { get; set; }

    /// <summary>Kiểm tra ca qua ngày</summary>
    public bool is_overday { get; set; }

    /// <summary>Thiết bị hợp lệ</summary>
    public bool is_valid_device { get; set; }

    /// <summarCoordinatesy>Toạ độ chấm công của user</summary>
    public Coordinates coordinates { get; set; }

    /// <summary>Thời gian checkout trước chỉnh sửa => BỎ</summary>
    public string time_shift_checkout { get; set; }

    /// <summary>Thời gian làm thực tế</summary>
    public double time_work_real { get; set; }

  }

  public class Location
  {
    public string id { get; set; }
    public string name { get; set; }
  }

  public class Coordinates
  {
    public double latitude { get; set; }
    public double longitude { get; set; }
  }

  public class Distance
  {
    public bool inside { get; set; }
    public double distance { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }

  }
}