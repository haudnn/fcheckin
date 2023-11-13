using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class AchievementModel
  {
    [BsonId]
    public string id { get; set; }
    // Người nhận
    public string user { get; set; }
    // Tiêu đề
    public string name { get; set; }
    // Mô tả
    public string desc { get; set; }
    // Loại: kaizen, cfrs, educate, todolist
    public string type { get; set; }

    public string type_id { get; set; }
    // Số sao
    public int star { get; set; }
    // Ngày tạo
    public long date { get; set; }
    // Xem thành tựu hay chưa → Chưa thì hiện popup
    public bool view { get; set; }

    public class Option
    {
      [BsonId]
      public string id { get; set; }

      public string name { get; set; }

      public string image { get; set; }

      public string des { get; set; }

      public int apply { get; set; }

      public int star { get; set; }

      public bool is_active { get; set; }

      public string parent { get; set; }

      public long update { get; set; }

      public bool is_delete { get; set; }
    }
  }
}
