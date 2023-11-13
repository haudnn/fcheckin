using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class KpisCheckinModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Chu kỳ</summary>
    public string cycle { get; set; }

    /// <summary>Id KPIs gốc</summary>
    public string kpis_root { get; set; }

    /// <summary>Id KPIs</summary>
    public string kpis { get; set; }

    /// <summary>Ngày tạo</summary>
    public long date_create { get; set; }

    /// <summary>Ngày xác nhận</summary>
    public long date_confirm { get; set; }

    /// <summary>Người tạo</summary>
    public string user_create { get; set; }

    /// <summary>Người phê duyệt</summary>
    public string user_confirm { get; set; }

    /// <summary>Giá trị</summary>
    public double value { get; set; }

    /// <summary>Giá trị biến thiên</summary>
    public double value_fluctuate { get; set; }

    /// <summary>Nhận xét</summary>
    public string comment { get; set; }

    /// <summary>Trạng thái check in: 1. Chờ phê duyệt, 2. Đã phê duyệt</summary>
    public int status_checkin { get; set; }

    /// <summary>File đính kèm</summary>
    public List<string> files { get; set; } = new();
  }
}
