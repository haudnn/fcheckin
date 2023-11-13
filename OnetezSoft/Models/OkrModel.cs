using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class OkrModel
  {
    [BsonId]
    public string id { get; set; }

    public bool delete { get; set; }

    /// <summary>Tên OKRs</summary>
    public string name { get; set; }

    /// <summary>Chu kỳ</summary>
    public string cycle { get; set; }

    /// <summary>Loại: 1. mở rộng, 2. Cam kết</summary>
    public int type { get; set; }

    /// <summary>OKRs cấp trên</summary>
    public string parent { get; set; }

    /// <summary>Độ tự tin</summary>
    public int confident { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date_create { get; set; }

    /// <summary>Người tạo</summary>
    public string user_create { get; set; }

    /// <summary>Lần checkin cuối cùng</summary>
    public long date_checkin { get; set; }

    /// <summary>Lần checkin đã xác nhận lần trước</summary>
    public long date_checkin_confirm { get; set; }

    /// <summary>Lần checkin tiếp theo</summary>
    public long next_checkin { get; set; }

    /// <summary>Quản lý checkin</summary>
    public string user_checkin { get; set; }

    /// <summary>Trạng thái checkin: 1. Nháp, 2. Đã xác nhận, 3. Đã checkin</summary>
    public int status_checkin { get; set; }

    /// <summary>Trạng thái checkin cũ: 1. Nháp, 2. Đã xác nhận, 3. Đã checkin</summary>
    public int status_checkin_old { get; set; }

    /// <summary>Hoàn thành/Chưa</summary>
    public bool done { get; set; }

    /// <summary>Tiến độ lần checkin trước</summary>
    public double progress_prev { get; set; }

    /// <summary>Kết quả chính</summary>
    public List<KeyResult> key_results { get; set; }

    /// <summary>Đánh giá OKRs</summary>
    public Review review_data { get; set; } = new();

    /// <summary>Thời gian nhân viên gửi đánh giá</summary>
    public long review_send_date { get; set; }

    /// <summary>Trạng thái gửi đánh giá: 1. Nháp, 2. Đã gửi, 3. Đã xác nhận</summary>
    public int review_send_status { get; set; }

    /// <summary>Nhân sự tự nhận xét</summary>
    public string review_staff_comment { get; set; }

    /// <summary>Quản lý nhận xét</summary>
    public string review_manager_comment { get; set; }

    /// <summary>Quản lý đánh giá: ID</summary>
    public string review_manager_id { get; set; }

    /// <summary>Quản lý đánh giá: thời gian</summary>
    public long review_manager_date { get; set; }

    /// <summary>Người xem đánh giá</summary>
    public List<string> review_viewers { get; set; } = new();


    /// <summary>Kết quả chính</summary>
    public class KeyResult
    {
      public string id { get; set; }

      /// <summary>Tên KR</summary>
      public string name { get; set; }

      /// <summary>Mục tiêu</summary>
      public double target { get; set; }

      /// <summary>Kết quả hiện tại</summary>
      public double result { get; set; }

      /// <summary>Thay đổi so với lần trước</summary>
      public double change { get; set; }

      /// <summary>Đơn vị tính</summary>
      public string unit { get; set; }

      /// <summary>Link chi tiết</summary>
      public string link { get; set; }

      /// <summary>Độ tự tin</summary>
      public int confident { get; set; }

      /// <summary>OKRs liên kết chéo</summary>
      public List<CrossLink> cross_links { get; set; }

      /// <summary>Đánh giá OKRs</summary>
      public Review review_data { get; set; } = new();

      public List<string> user_assign {get; set;}= new();
    }

    /// <summary>OKRs liên kết chéo</summary>
    public class CrossLink
    {
      public string id { get; set; }
      public string okr { get; set; }
      public string user { get; set; }
    }

    /// <summary>Đánh giá</summary>
    public class Review
    {
      /// <summary>Nhân sự tự đánh giá</summary>
      public double staff_point { get; set; }

      /// <summary>Quản lý đánh giá</summary>
      public double manager_point { get; set; }

      /// <summary>Nhân sự tự nhận xét</summary>
      public string staff_comment { get; set; }

      /// <summary>Quản lý nhận xét</summary>
      public string manager_comment { get; set; }
    }
  }
}