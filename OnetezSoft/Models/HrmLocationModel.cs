using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

public class HrmLocationModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Tiêu đề địa điểm</summary>
  public string name { get; set; }

  /// <summary>Vĩ độ</summary>
  public string latitude { get; set; }

  /// <summary>Kinh độ</summary>
  public string longitude { get; set; }

  /// <summary>Bán kính châm công (mét)</summary>
  public int radius { get; set; }

  /// <summary>Công ty áp dụng</summary>
  public List<string> companys { get; set; } = new();

  /// <summary>Ngày tạo</summary>
  public long created { get; set; }
}