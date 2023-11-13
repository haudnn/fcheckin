using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class KpisCycleModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên chu kì</summary>
    public string name { get; set; }

    /// <summary>Ngày bắt đầu</summary>
    public long start_date { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public long end_date { get; set; }

    /// <summary>Danh sách quản lý</summary>
    public List<string> managers { get; set; } =  new();

    /// <summary>Danh sách người xem</summary>
    public List<string> viewers { get; set; } = new();
  }
}
