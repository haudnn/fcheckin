using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class OkrStarModel
  {
    [BsonId]
    public string id { get; set; }

    // ID người tạo
    public string user { get; set; }

    // Số sao cấp
    public int star { get; set; }

    /// <summary>Loại ví: 1. Ghi nhận, 2. Cá nhân</summary>
    public int wallet { get; set; }

    // Ngày tạo
    public long create_date { get; set; }
    
    // Người tạo
    public string create_id { get; set; }
  }
}
