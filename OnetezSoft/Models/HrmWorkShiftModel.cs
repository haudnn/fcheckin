using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

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
}