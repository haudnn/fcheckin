using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class HrmTimesheetUserModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>id user</summary>
    public string user { get; set; }

    /// <summary>id bảng công áp dụng</summary>
    public string timesheet_id { get; set; }

    /// <summary>Ngày bắt đầu</summary>
    public long from { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public long to { get; set; }

    /// <summary>Thời gian update</summary>
    public long updated { get; set; }

    /// <summary>danh sách dữ liệu bảng công theo ngày</summary>
    public List<TimeSheetDate> timesheet_dates { get; set; } = new();

    /// <summary>Công chuẩn theo theo bảng công</summary>
    public double time_total_sheet { get; set; }

    /// <summary>Công chuẩn theo user</summary>
    public double time_total_user { get; set; }

    /// <summary>Công chuẩn có được chỉnh sửa chưa, true: lấy công chuẩn theo user, false: lấy công chuẩn theo tháng</summary>
    public bool is_edit_time { get; set; }

    /// <summary>Check có hiện trên bảng công không</summary>
    public bool is_delete { get; set; }

    /// <summary>Tổng công chuẩn: nếu có set công ch uẩn theo user thì lấy theo user, mặc định lấy theo công chuẩn tháng</summary>
    public double time_total
    {
      get
      {
        if (is_edit_time)
          return time_total_user;
        else
          return time_total_sheet;
      }
    }

    /// <summary>Tổng công overtime</summary>
    public double time_ot
    {
      get
      {
        var ot_shift = timesheet_dates.SelectMany(i => i.shifts_edit.Values).ToList();
        return ot_shift.Where(i => i.is_ot).Sum(i => i.time_edit);
      }
    }

    public class TimeSheetDate
    {
      /// <summary>Ngày bảng công</summary>
      public long date { get; set; }

      /// <summary>Kiểm tra khoá ngày => không được chỉnh sửa</summary>
      public bool locked { get; set; }

      /// <summary>Dữ liệu thay đổi khi chỉnh sửa hoặc áp dụng đơn từ: Key = id ca làm, Value = dữ liệu sau khi chỉnh sửa</summary>
      public Dictionary<string, TimeSheetEdit> shifts_edit { get; set; } = new();

      /// <summary>Dữ liệu thay đổi khi chỉnh sửa hoặc áp dụng đơn từ: Key = id ca làm, Value = dữ liệu sau khi áp dụng đơn từ</summary>
      public Dictionary<string, TimeSheetForm> shifts_form { get; set; } = new();
    }

    public class TimeSheetEdit
    {
      /// <summary>Thời gian chỉnh sửa</summary>
      public long updated { get; set; }

      /// <summary>id đơn từ áp dụng TH: công không có lương: 0, công có lương: 1, và các id đơn từ sử dụng</summary>
      public string form_id { get; set; }

      /// <summary>Tên đơn từ áp dụng</summary>
      public string form_name { get; set; }

      /// <summary>Tên ca làm khi tạo ca làm mới</summary>
      public string work_name { get; set; }

      /// <summary>Ký hiệu đơn từ áp dụng</summary>
      public string form_sign { get; set; }

      /// <summary>Công sau khi chỉnh sửa</summary>
      public double time_edit { get; set; }

      /// <summary>Kiểm tra phải chỉnh sửa ca overtime không</summary>
      public bool is_ot { get; set; }

      /// <summary>Lý do chỉnh sửa</summary>
      public string reason { get; set; }
    }

    public class TimeSheetForm
    {
      /// <summary>Thời gian sử dụng đơn từ</summary>
      public long updated { get; set; }

      /// <summary>id đơn từ áp dụng</summary>
      public string form_id { get; set; }
    }
  }
}
