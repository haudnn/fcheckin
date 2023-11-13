using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;


namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class NotificationModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tiêu đề</summary>
    public string title { get; set; }

    /// <summary>Tiêu để phụ</summary>
    public string sub_title { get; set; }

    /// <summary>Hình ảnh</summary>
    public string image { get; set; }

    /// <summary>Nội dung</summary>
    public string content { get; set; }

    /// <summary>Danh mục thông báo</summary>
    public string category { get; set; }

    /// <summary>Ngày tạo</summary>
    public long created { get; set; }

    /// <summary>Ngày published</summary>
    public long published { get; set; }

    /// <summary>Kiểm tra lưu nháp</summary>
    public bool isDraft { get; set; }

    /// <summary>Danh sách công ty được hiển thị</summary>
    public List<string> companys { get; set; } = new();

    /// <summary> Thông tin người tạo </summary>
    public MemberModel creator { get; set; }

    /// <summary> Pin lên màn hình </summary>
    public bool pin { get; set; }
  }
}
