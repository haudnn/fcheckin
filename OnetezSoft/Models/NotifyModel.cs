using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class NotifyModel
  {
    [BsonId]
    public string id { get; set; }
    // Nội dung
    public string name { get; set; }
    // Link
    public string link { get; set; }
    // id người gửi
    public string user_send { get; set; }
    // Người nhận
    public string user { get; set; }
    // Avatar người gửi
    public string avatar { get; set; }
    // Ngày tạo
    public long date { get; set; }
    // Xem chưa
    public bool read { get; set; }
    // Loại thông báo
    public int type { get; set; }
    // ID của nội dung
    public string key { get; set; }

    /// <summary>
    /// Nội dung phụ thông báo CMS
    /// </summary>
    public string sub_title { get; set; }

    /// <summary> Có phải thông báo cms? </summary>
    public bool isCMS { get; set; }
  }
}
