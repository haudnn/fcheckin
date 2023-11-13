using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class KpisModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>id user thực hiện</summary>
    public string user { get; set; }

    /// <summary>Danh sách quản lý</summary>
    public List<string> managers { get; set; } = new();

    /// <summary>Danh sách người xem</summary>
    public List<string> viewers { get; set; } = new();  

    /// <summary>Danh sách người xem: Key: id user, Value: 1-Tổng quan, 2-Cây, 3-Báo cáo</summary>
    public Dictionary<string, List<int>> viewerList { get; set; } = new();

    /// <summary>Tiêu đề</summary>
    public string name { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date_create { get; set; }

    /// <summary>id chu kỳ</summary>
    public string cycle { get; set; }

    /// <summary>id kpis root</summary>
    public string root { get; set; }

    /// <summary>id KPIs liên kết</summary>
    public string parent { get; set; }

    /// <summary>Ngày bắt đầu</summary>
    public long start_date { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public long end_date { get; set; }

    /// <summary>link kế hoạch</summary>
    public string plan_link { get; set; }

    /// <summary>link kế hoạch</summary>
    public List<Plan> plan_list { get; set; } = new();

    /// <summary>loại: 1 - Nhập liệu, 2 - Tự động (Mặc định là tự động)</summary>
    public int type { get; set; } = 2;

    /// <summary>Trạng thái check in: 1. Chờ xác nhận, 2. Đã check-in</summary>
    public int status_checkin { get; set; }

    /// <summary>Lần xác nhận cuối cùng</summary>
    public long date_confirm { get; set; }

    /// <summary>Lần checkin cuối cùng</summary>
    public long date_checkin { get; set; }

    /// <summary>Số lượng bình luận</summary>
    public int comment { get; set; }

    /// <summary>File đính kèm</summary>
    public List<string> files { get; set; } = new();

    /// <summary>Chỉ số: đã bao gồm Mục tiêu và Thực đạt</summary>
    public List<KpisMetricModel> metrics { get; set; } = new();

    public class Plan
    {
      /// <summary>1: plan link web, 2: plan từ kết hoạch</summary>
      public int type { get; set;}

      /// <summary>id kế hoạch liên kết</summary>
      public string id_plan { get; set;}

      /// <summary>Mô tả link</summary>
      public string description { get; set; }

      /// <summary>liên kết</summary>
      public string link { get; set; }
    }
  }
}
