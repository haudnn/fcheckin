
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class WorkLogModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên hành động</summary>
    public string name { get; set; }

    /// <summary>Chi tiết hành động</summary>
    public string detail { get; set; }

    /// <summary>Thời gian thực hiện</summary>
    public long date { get; set; }

    /// <summary>Kế hoạch</summary>
    public string plan { get; set; }

    /// <summary>Công việc chính</summary>
    public string task { get; set; }

    /// <summary>Công việc phụ</summary>
    public string sub_task { get; set; }

    /// <summary>Người thực hiện</summary>
    public MemberModel user { get; set; }
  }
}