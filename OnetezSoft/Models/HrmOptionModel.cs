using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmOptionModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Loại hạng mục</summary>
  public string type { get; set; }

  /// <summary>Tên hạng mục</summary>
  public string name { get; set; }

  /// <summary>Thứ tự</summary>
  public int pos { get; set; }

  /// <summary>ID Công ty liên kết</summary>
  public string company { get; set; }

  /// <summary>Quyền truy cập</summary>
  public List<string> managers { get; set; } = new();
}