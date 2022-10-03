using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class WorkTaskModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>Tên công việc</summary>
    public string name { get; set; }

    /// <summary>Mô tả</summary>
    public string detail { get; set; }

    /// <summary>Trạng thái</summary>
    public int status { get; set; }

    /// <summary>Độ ưu tiên</summary>
    public int priority { get; set; }

    /// <summary>Ngày bắt đầu</summary>
    public long date_start { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public long date_end { get; set; }

    /// <summary>Kế hoạch</summary>
    public string plan_id { get; set; }

    /// <summary>Nhóm công việc</summary>
    public string section_id  { get; set; }

    /// <summary>Nhãn công việc</summary>
    public List<string> labels { get; set; }
  }
}