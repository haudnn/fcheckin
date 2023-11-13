using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class DayOffModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên ngày nghỉ</summary>
    public string name { get; set; }

    /// <summary>Ngày bắt đầu</summary>
    public long start { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public long end { get; set; }

    /// <summary>Ngày tạo</summary>
    public long create { get; set; }

    /// <summary>Lặp: 1. Một lần | 2. Hàng tuần</summary>
    public int loop { get; set; }

    /// <summary>Lặp ngày trong tuần</summary>
    public Week loop_week { get; set; }

    /// <summary>Ngảy nghỉ tính lương</summary>
    public bool has_salary { get; set; }

    public class Week
    {
      public bool mon { get; set; }
      public bool tue { get; set; }
      public bool wed { get; set; }
      public bool thu { get; set; }
      public bool fri { get; set; }
      public bool sat { get; set; }
      public bool sun { get; set; }
    }

    /// <summary>Danh sách áp dụng lương cho từng nhân viên</summary>
    public List<TimeOffs> time_off { get; set; } = new();

    public class TimeOffs
    {
      /// <summary>Công ngày nghỉ</summary>
      public double time {get; set;}

      /// <summary>Danh sách id user áp dụng</summary>
      public List<string> users{ get; set; } = new();
    }
  }
}