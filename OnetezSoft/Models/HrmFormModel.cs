using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using OnetezSoft.Data;


namespace OnetezSoft.Models;
public class HrmFormModel
{
  [BsonId]
  public string id { get; set; }

  /// <summary>id user</summary>
  public string user { get; set; }

  /// <summary>Ngày tạo đơn</summary>
  public long created { get; set; }

  /// <summary>Ngày phê duyệt</summary>
  public long confirm_date { get; set; }

  /// <summary>dữ liệu đơn từ áp dụng</summary>
  public HrmRulesModel.Form form { get; set; }

  /// <summary>danh sách khoảng ngày xin phép nghỉ</summary>
  public List<WorkDateShift> work_date_shifts { get; set; } = new();

  /// <summary>Kiểm tra đơn từ đã được phê duyệt hay chưa</summary>
  public bool is_confirm { get; set; }

  /// <summary>lý do</summary>
  public string reason { get; set; }

  /// <summary>Hình ảnh</summary>
  public List<string> images { get; set; } = new();

  /// <summary>Danh sách id user cần duyệt đơn từ</summary>
  public List<string> users { get; set; } = new();

  /// <summary>Danh sách cần duyệt đơn từ</summary>
  public List<FormConfirm> confirm_users { get; set;} = new();

  public class WorkDateShift
  {
    /// <summary>Thời gian bắt đầu nghỉ</summary>
    public long start { get; set; }

    /// <summary>Thời gian kết thúc nghỉ</summary>
    public long end { get; set; }

    /// <summary>Kiểm tra load hết chưa</summary>
    public bool loaded { get; set; }
  }

  public class FormConfirm
  {
    /// <summary>Thứ tự xét đơn từ</summary>
    public int pos { get; set; }

    /// <summary>id user xác nhận đơn</summary>
    public string user { get; set; }

    /// <summary> trạng thái xác nhận 1: Chờ xác nhận, 2: Đã xác nhận, 3: Đã từ chối</summary>
    public int status { get; set; } = 1;
  }
}
