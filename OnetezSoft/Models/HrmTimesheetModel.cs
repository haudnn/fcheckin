using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Bson.Serialization.Attributes;
using OnetezSoft.Services;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmTimesheetModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>Tên bảng công</summary>
  public string name { get; set; }

  /// <summary>Ngày bắt đầu</summary>
  public long from { get; set; }

  /// <summary>Ngày kết thúc</summary>
  public long to { get; set; }

  /// <summary>Lần cập nhật cuối cùng</summary>
  public long updated { get; set; }

  /// <summary>Hiển thị trên bảng công</summary>
  public bool is_show { get; set; }

  /// <summary>Xóa bảng công</summary>
  public bool is_delete { get; set; }

  /// <summary>Công chuẩn theo bảng công</summary>
  public double time_total { get; set; }

  /// <summary>Kiểm tra khoá bảng công</summary>
  public bool locked { get; set; }
}