using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class WorkLogModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên hành động</summary>
    public string name { get; set; }

    /// <summary>Chi tiết hành động</summary>
    public string detail { get; set; }

    /// <summary>Thời gian thực hiện</summary>
    public long date { get; set; }

    /// <summary>Người thực hiện</summary>
    public MemberModel user { get; set; }
  }
}