using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class PromotionModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tiêu đề</summary>
    public string title { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date { get; set; }

    /// <summary>Loại: 1. thời gian, 2. người dùng</summary>
    public int type { get; set; }

    /// <summary>Chiết khấu (%)</summary>
    public int discount { get; set; }

    /// <summary>Điền kiện áp dụng</summary>
    public int condition { get; set; }
  }
}
