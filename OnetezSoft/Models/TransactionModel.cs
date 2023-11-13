using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class TransactionModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary> Ngày giao dịch </summary>
    public long date { get; set; }

    /// <summary> Khách hàng </summary>
    public MemberModel customer { get; set; }

    /// <summary> Người tạo </summary>
    public MemberModel creator { get; set; }

    /// <summary> Số tiền giao dịch </summary>
    public int money { get; set; }

    /// <summary> Số dư sau giao dịch </summary>
    public int balance { get; set; }

    /// <summary> Nội dung giao dịch </summary>
    public string content { get; set; }

    /// <summary> Trạng thái </summary>
    public int status { get; set; }

    /// <summary> Loại giao dịch </summary>
    public int type { get; set; }

    /// <summary> ID Sản phẩm </summary>
    public string product { get; set; }

    /// <summary> ID tổ chức </summary>
    public string Company { get; set; }
    
  }
}