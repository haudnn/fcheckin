using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmTimesheetLogModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>id user</summary>
  public string user { get; set; }

  /// <summary>Ngày chấm công</summary>
  public long day { get; set; }

  /// <summary>Tên ca</summary>
  public string shift_name { get; set; }

  /// <summary>Thời gian chỉnh sửa</summary>
  public long edit_date { get; set; }

  /// <summary>Nội dung mới</summary>
  public string edit_content { get; set; }

  /// <summary>Nội dung cũ</summary>
  public string old_content { get; set; }

  /// <summary>Người thực hiện</summary>
  public string editor { get; set; }
}