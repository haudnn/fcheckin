using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class CommentModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Mô tả</summary>
    public string detail { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date { get; set; }

    /// <summary>Liên kết model</summary>
    public string model_id { get; set; }

    /// <summary>Người tạo</summary>
    public string user_id { get; set; }

    /// <summary>File đính kèm</summary>
    public List<string> files { get; set; } = new();
  }
}
