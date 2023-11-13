using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class KpisRootReportModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Chu kỳ</summary>
    public string cycle { get; set; }

    /// <summary>Id KPIs gốc</summary>
    public string kpis_root { get; set; }

    /// <summary>Phần trăm kpis tổng</summary>
    public double value { get; set; }

    /// <summary>Ngày thay đổi dữ liệu</summary>
    public long date { get; set; }
  }
}
