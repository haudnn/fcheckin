﻿using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class NotifyModel
  {
    [BsonId]
    public string id { get; set; }
    // Nội dung
    public string name { get; set; }
    // Link
    public string link { get; set; }
    // Người nhận
    public string user { get; set; }
    // Ngày tạo
    public long date { get; set; }
    // Xem chưa
    public bool read { get; set; }
    // Loại thông báo
    public int type { get; set; }
    // ID của nội dung
    public string key { get; set; }
  }
}
