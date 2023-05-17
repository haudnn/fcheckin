using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

public class HrmTimesheetLogModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Timesheet ID</summary>
  public string timesheet { get; set; }

  /// <summary>Ngày chấm công</summary>
  public long day { get; set; }

  /// <summary>Ca chỉnh sửa</summary>
  public bool is_morning { get; set; }

  /// <summary>Thời gian chỉnh sửa</summary>
  public long edit_date { get; set; }

  /// <summary>Nội dung mới</summary>
  public string edit_content { get; set; }

  /// <summary>Nội dung cũ</summary>
  public string old_content { get; set; }

  /// <summary>Người thực hiện</summary>
  public string editor { get; set; }
}