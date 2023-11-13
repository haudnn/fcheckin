using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class OkrCheckinModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Chu kỳ</summary>
    public string cycle { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date_create { get; set; }

    /// <summary>Người tạo</summary>
    public string user_create { get; set; }

    /// <summary>Ngày hoàn tất check-in</summary>
    public long date_checkin { get; set; }

    /// <summary>Trạng thái check in: 1. Đúng hạn, 2. Trễ hạn</summary>
    public int status_checkin { get; set; }

    /// <summary>Người checkin</summary>
    public string user_checkin { get; set; }

    /// <summary>ID OKRs</summary>
    public string okr { get; set; }

    /// <summary>Độ tự tin</summary>
    public int confident { get; set; }

    /// <summary>Tiến độ lần checkin trước</summary>
    public double progress_prev { get; set; }

    /// <summary>Checkin nháp/Xác nhận</summary>
    public bool draft { get; set; }

    /// <summary>Checkin xong/chưa</summary>
    public bool checkin { get; set; }

    /// <summary>Checkin Kết quả chính</summary>
    public List<KeyResult> key_results { get; set; } = new();

    /// <summary>Phản hồi checkin</summary>
    public List<Feedback> feedbacks { get; set; } = new();

    /// <summary>File đính kèm</summary>
    public List<string> files { get; set; } = new();


    public class KeyResult
    {
      public string id { get; set; }
      public double result { get; set; }
      public double change { get; set; }
      public int confident { get; set; }
      public List<string> questions { get; set; }
      public string feedback { get; set; }
    }

    public class Feedback
    {
      public string id { get; set; }
      public string user { get; set; }
      public string content { get; set; }
      public string kr { get; set; }
      public long date { get; set; }
    }
  }
}