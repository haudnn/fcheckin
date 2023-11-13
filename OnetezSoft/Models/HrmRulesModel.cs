using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class HrmRulesModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>thiết lập thời gian checkin checkout sớm trễ(phút)</summary>
    public CheckInOut check_in_out { get; set; } = new();

    /// <summary>thiết lập đơn từ</summary>
    public List<Form> forms { get; set; } = new();

    /// <summary>thiết lập cho phép làm ngoài giờ</summary>
    public OverTime overtime { get; set; } = new();

    /// <summary>thiết lập cho phép đăng ký phân ca</summary>
    public RegisterShift register_shift { get; set; } = new();

    public bool is_check_device { get; set; }

    public class CheckInOut
    {
      /// <summary>Thời gian checkin sớm (phút)</summary>
      public int in_early { get; set; }

      /// <summary>Kích hoạt checkin sớm</summary>
      public bool is_in_early { get; set; }

      /// <summary>Thời gian checkin trễ (phút)</summary>
      public int in_late { get; set; }

      /// <summary>Kích hoạt checkin trễ</summary>
      public bool is_in_late { get; set; }

      /// <summary>Thời gian checkout sớm (phút)</summary>
      public int out_early { get; set; }

      /// <summary>Kích hoạt checkout sớm</summary>
      public bool is_out_early { get; set; }
    }
    public class Form
    {
      public string id { get; set; }

      /// <summary>Ký hiệu đơn từ</summary>
      public string sign { get; set; }

      /// <summary>Tên đơn từ</summary>
      public string name { get; set; }

      /// <summary>Có tính công</summary>
      public bool has_shift_work { get; set; }

      /// <summary>Thiết lập ẩn hiện đơn từ</summary>
      public bool is_active { get; set; }

      /// <summary>Màu của đơn từ</summary>
      public string color { get; set; }

      /// <summary>Tấn suất cho phép nghỉ đơn từ</summary>
      public int frequency { get; set; }

      /// <summary>Đơn vị: 1: Theo ngày, 2 Theo tuần, 3: Theo tháng, 4: Theo quý, 5: Theo năm</summary>
      public int units { get; set; } = 3;

      /// <summary>Bật giới hạn đơn từ</summary>
      public bool is_limit { get; set; }
    }

    public class OverTime
    {
      /// <summary>Kích hoạt làm ngoài giờ</summary>
      public bool is_active { get; set; }

      /// <summary>Số phút tối thiểu làm ngoài giờ</summary>
      public int min_minutes { get; set; }

      /// <summary>Hiện thị trên bảng công</summary>
      public bool is_show { get; set; }
    }

    public class RegisterShift
    {
      /// <summary>thiết lập cho phép đăng ký phân ca</summary>
      public bool has_register_shifts { get; set; }

      /// <summary>id user áp dụng</summary>
      public List<string> users { get; set; } = new();
    }
  }
}
