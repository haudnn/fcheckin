using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.EMMA;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class HrmDeviceModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>id người yêu cầu</summary>
    public string user_request { get; set; }

    /// <summary>id người phê duyệt</summary>
    public string user_approve { get; set; }

    /// <summary>Ngày	yêu cầu</summary>
    public long date_request { get; set; }

    /// <summary>Ngày	phê duyệt</summary>
    public long date_approve { get; set; }

    /// <summary> Trạng thái: 1: pending, 2: accepted, 3: canceled </summary>
    public int status { get; set; }

    /// <summary> Mã device cũ</summary>
    public string old_code { get; set; }

    /// <summary> Tên device cũ</summary>
    public string old_name { get; set; }

    /// <summary> Mã device mới</summary>
    public string new_code { get; set; }

    /// <summary> Tên device cũ</summary>
    public string new_name { get; set; }

    /// <summary> Kiểm tra cập nhật lần đầu </summary>
    public bool is_updated { get; set; }
  }
}