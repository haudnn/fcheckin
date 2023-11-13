using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class EducateCourseModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên khóa học</summary>
    public string name { get; set; }

    /// <summary>Hình ảnh</summary>
    public string image { get; set; }

    /// <summary>Mô tả</summary>
    public string content { get; set; }

    /// <summary>Danh mục</summary>
    public List<string> category { get; set; } = new();

    /// <summary>Thời lượng</summary>
    public string time { get; set; }

    /// <summary>Số bài học</summary>
    public int lesson { get; set; }

    /// <summary>Số người học</summary>
    public int learned { get; set; }

    /// <summary>Số người tốt nghiệp</summary>
    public int graduated { get; set; }

    /// <summary>Hiển thị</summary>
    public bool show { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date { get; set; }

    /// <summary>Người tạo</summary>
    public string creator { get; set; }

    /// <summary>Danh sách người chấm thi</summary>
    public List<string> examiner { get; set; } = new();

    /// <summary>Giảng viên</summary>
    public string teacher { get; set; }

    /// <summary>Đánh giá</summary>
    public List<Review> reviews { get; set; }

    /// <summary>Người đã lưu khóa học</summary>
    public List<string> bookmarks { get; set; } = new();

    /// <summary>File đính kèm</summary>
    public List<string> files { get; set; } = new();


    public class Review
    {
      public string id { get; set; }
      public string user { get; set; }
      public int point { get; set; }
    }
  }
}
