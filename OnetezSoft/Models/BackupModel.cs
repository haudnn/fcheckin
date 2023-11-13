using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class BackupModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>ID của user</summary>
    public string user_id { get; set; }

    /// <summary>Tiêu đền backup</summary>
    public string name { get; set; }

    /// <summary>Trang sử dụng</summary>
    public string page { get; set; }

    /// <summary>Dữ liệu backup</summary>
    public object data { get; set; }

    public string serializeObject { get; set; }

    /// <summary>Loại dữ liệu</summary>
    public Type type { get; set; }

    /// <summary>Ngày tạo</summary>
    public long create { get; set; }

    /// <summary>Đã restore</summary>
    public bool restored { get; set; }

    /// <summary>Hết hạn</summary>
    public long expired { get; set; }
  }
}
