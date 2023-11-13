using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class CategoryModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary Tên danh mục </summary>
    public string name { get; set; }

    /// <summary> Màu sắc </summary>
    public string color { get; set; }

    /// <summary> Kiểm tra đã xóa </summary>
    public bool isDeleted { get; set; }
  }
}
