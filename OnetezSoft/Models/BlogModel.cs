using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class BlogModel
  {
    [BsonId]
    public string id { get; set; }
    // Tiêu đề
    public string name { get; set; }
    // Mô tả
    public string desc { get; set; }
    // Liên kết
    public string link { get; set; }
    // Hình ảnh
    public string image { get; set; }
    // Nội dung
    public string content { get; set; }
    // Phòng ban
    public string department { get; set; }
    // Pin
    public bool pin { get; set; }
    // Ngày tạo
    public long date { get; set; }
  }
}
