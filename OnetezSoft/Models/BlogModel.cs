using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class BlogModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tiêu đề</summary>
    public string name { get; set; }

    /// <summary>Mô tả</summary>
    public string desc { get; set; }

    /// <summary>Liên kết</summary>
    public string link { get; set; }

    /// <summary>Hình ảnh</summary>
    public string image { get; set; }

    /// <summary>Nội dung</summary>
    public string content { get; set; }

    /// <summary>Phòng ban</summary>
    public string department { get; set; }

    /// <summary>Danh mục</summary>
    public string category { get; set; }

    /// <summary>Ngày hiển thị</summary>
    public long date { get; set; }

    /// <summary>Ngày tạo</summary>
    public long created { get; set; }

    /// <summary>Thứ tự ghim</summary>
    public int pos { get; set; }

    /// <summary>Ghim bài viết</summary>
    public bool pin { get; set; }

    /// <summary>Hiển thị bài viết</summary>
    public bool is_show { get; set; } = true;

    /// <summary>Tin của tông công ty</summary>
    public bool is_all { get; set; }
  }
}
