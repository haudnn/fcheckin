using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class KpisMetricModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên chỉ số</summary>
    public string name { get; set; }

    /// <summary>Thời gian tạo, chỉnh sửa</summary>
    public long date { get; set; } 

    /// <summary>Tên đơn vị</summary>
    public string units { get; set; }

    /// <summary>Mục tiêu</summary>
    public double target { get; set; }

    /// <summary>Thực đạt</summary>
    public double value { get; set; }

    /// <summary>Mô tả</summary>
    public string description { get; set; }

    /// <summary>Check xoá</summary>
    public bool is_deleted { get; set; }
  }
}
