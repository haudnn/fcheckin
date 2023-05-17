using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class OkrCfrModel
  {
    [BsonId]
    public string id { get; set; }
    // Chu kỳ
    public string cycle { get; set; }
    // Ngày tạo
    public long date_create { get; set; }
    // Người tạo
    public string user_create { get; set; }
    // Người nhận
    public string user_receive { get; set; }
    // Loại: 2. Ghi nhận, 3. Tặng sao
    public int type { get; set; }
    // Số sao
    public int star { get; set; }
    // Id OKRs
    public string okr { get; set; }
    public string okr_name { get; set; }
    // Nội dung
    public string content { get; set; }
    // ID tiêu chí
    public string evaluate { get; set; }
    public string evaluate_name { get; set; }
    // Ghi nhận dặt biệt
    public bool special { get; set; } 
    // Riêng tư
    public bool privacy { get; set; }
    
    /// <summary>Thả tim</summary>
    public bool like { get; set; }
  }
}