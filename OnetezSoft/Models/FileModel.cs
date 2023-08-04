using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

public class FileModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Link file</summary>
  public string link { get; set; }

  /// <summary>Tên file</summary>
  public string name { get; set; }

  /// <summary>Định dạng</summary>
  public string format { get; set; }

  /// <summary>Kích thước</summary>
  public long size { get; set; }

  /// <summary>Ngày tải lên</summary>
  public long date { get; set; }

  /// <summary>Người xử lý: tên</summary>
  public string author_name { get; set; }

  /// <summary>Người xử lý: avatar</summary>
  public string author_avatar { get; set; }
}
