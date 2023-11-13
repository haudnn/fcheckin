using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class BannerModel
  {
    [BsonId]
    public string id { get; set; }
    // Tiêu đề
    public string name { get; set; }
    // Liên kết
    public string link { get; set; }
    // Hình ảnh
    public string image { get; set; }
    // Phòng ban
    public string department { get; set; }
    // Ghim lên đầu
    public bool pin { get; set; }
    // Ngày tạo
    public long date { get; set; }

    public int pos { get; set; }

    // Trang hiển thị
    public Dictionary<string, string> pages { get; set; }
  }
}
