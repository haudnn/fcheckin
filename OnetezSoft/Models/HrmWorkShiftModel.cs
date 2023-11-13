using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmWorkShiftModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Ngày tạo</summary>
  public long created { get; set; }

  /// <summary>Tên ca</summary>
  public string name { get; set; }

  /// <summary>Giờ bắt đầu</summary>
  public string checkin { get; set; }

  /// <summary>Giờ kết thúc</summary>
  public string checkout { get; set; }

  /// <summary>Số công</summary>
  public double value { get; set; }

  /// <summary>Kiểm tra ca làm kéo qua ngày</summary>
  public bool is_overday { get; set; }

  /// <summary>Kiểm tra đã xoá </summary>
  public bool is_deleted { get; set; }

  /// <summary>Thời gian xoá ca</summary>
  public long time_delete { get; set; }

  /// <summary>Màu của ca làm</summary>
  public string color { get; set; }

  /// <summary>Ca làm theo giờ?</summary>
  public bool is_byhour { get; set; }

  /// <summary>Số phút tính công</summary>
  public long minutes { get; set; }

  public List<BreakTime> break_times { get; set; } = new();

  public class BreakTime
  {
    public string start { get; set; }
    public string end { get; set; }
  }
}