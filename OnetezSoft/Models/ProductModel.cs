using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class ProductModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Hiển thị</summary>
    public bool show { get; set; }

    /// <summary>Tiêu đề chính</summary>
    public string title { get; set; }

    /// <summary>Mô tả ngắn</summary>
    public string desc { get; set; }

    /// <summary>Mô tả dài</summary>
    public string detail { get; set; }

    /// <summary>Icon</summary>
    public string icon { get; set; }

    /// <summary>Màu sắc</summary>
    public string color { get; set; }

    /// <summary>Hình thumb</summary>
    public string cover { get; set; }

    /// <summary>Phiên bản</summary>
    public string version { get; set; }

    /// <summary>Chi tiết cập nhật</summary>
    public string update { get; set; }

    /// <summary>Đơn giá</summary>
    public int price { get; set; }

    /// <summary>Video</summary>
    public Video video { get; set; }

    /// <summary>Hình ảnh</summary>
    public List<string> images { get; set; }

    /// <summary>Bảng giá sản phẩm</summary>
    public List<Content> pricelist { get; set; }

    /// <summary>Tính năng sản phẩm</summary>
    public List<Content> features { get; set; }

    /// <summary>Bạn nhận được gì khi sử dụng</summary>
    public List<Content> modules { get; set; }


    public class Content
    {
      public string id { get; set; }
      public string title { get; set; }
      public string image { get; set; }
      public List<Text> content { get; set; }
    }

    public class Video
    {
      public string link { get; set; }
      public string thumb { get; set; }
    }

    public class Text
    {
      public string id { get; set; }
      public string value { get; set; }
    }
  }
}
