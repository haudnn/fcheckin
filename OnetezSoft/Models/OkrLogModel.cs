using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class OkrLogModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Ngày tạo</summary>
  public long created { get; set; }

  /// <summary>UserID</summary>
  public string user_id { get; set; }

  /// <summary>OKRs ID</summary>
  public string okr_id { get; set; }

  /// <summary>Hành động</summary>
  public string action { get; set; }

  /// <summary>Nội dung cũ</summary>
  public Log old { get; set; } = new();

  /// <summary>Nội dung cũ</summary>
  public Log edit { get; set; } = new();

  public class Log
  {
    /// <summary>Tên OKR</summary>
    public string okr { get; set; }

    /// <summary>Tên KR</summary>
    public List<string> kr { get; set; } = new();

    /// <summary>Mục tiêu</summary>
    public List<string> target { get; set; } = new();
  }
}
