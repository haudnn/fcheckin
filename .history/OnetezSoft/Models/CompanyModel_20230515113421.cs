using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class CompanyModel
  {
    [BsonId]
    public string id { get; set; }

    public bool delete { get; set; }

    /// <summary>Ngày tạo</summary>
    public long create_date { get; set; }

    /// <summary>ID chủ tổ chức</summary>
    public string admin_id { get; set; }

    /// <summary>Trạng thái</summary>
    public bool status { get; set; }

    /// <summary>Tên tổ chức</summary>
    public string name { get; set; }

    /// <summary>Điện thoại</summary>
    public string phone { get; set; }

    /// <summary>Email</summary>
    public string email { get; set; }

    /// <summary>Website</summary>
    public string website { get; set; }

    /// <summary>Địa chỉ</summary>
    public string address { get; set; }

    /// <summary>Mô tả / ghi chú</summary>
    public string note { get; set; }

    /// <summary>Logo</summary>
    public string logo { get; set; }

    /// <summary>Số người dùng</summary>
    public int members { get; set; }

    /// <summary>Mã xác minh</summary>
    public string verify { get; set; }

    /// <summary>Cấu hình Todolist</summary>
    public Todolist todolist { get; set; }

    /// <summary>Cấu hình danh mục Kaizen</summary>
    public List<Kaizen> kaizen { get; set; }

    /// <summary>Chức năng → Bỏ</summary>
    public Module module { get; set; }

    /// <summary>Sản phẩm sở hữu</summary>
    public List<Product> products { get; set; }



    /// <summary>Cấu hình Todolist</summary>
    public class Todolist
    {
      /// <summary>Thời gian nộp HH:mm</summary>
      public string time_checkin { get; set; }

      /// <summary>Thời gian checkout HH:mm</summary>
      public string time_checkout { get; set; }
      
      /// <summary>Thời gian tự động xác nhận</summary>
      public string time_confirm { get; set; }
    }

    /// <summary>Cấu hình danh mục Kaizen</summary>
    public class Kaizen
    {
      public string id { get; set; }
      public string name { get; set; }
      public string image { get; set; }
      /// <summary>
      /// Chứa id của cha 
      /// </summary>
      public string parent { get; set; }
    }

    /// <summary>Sản phẩm sở hữu</summary>
    public class Product
    {
      public string id { get; set; }

      /// <summary>Tên sản phẩm</summary>
      public string title { get; set; }

      /// <summary>Kích hoạt</summary>
      public bool active { get; set; }

      /// <summary>Số người tối đa</summary>
      public int total { get; set; }

      /// <summary>Số người đã dùng</summary>
      public int used { get; set; }

      /// <summary>Số tiền lúc mua, gia hạn</summary>
      public int price { get; set; }

      /// <summary>Ngày mua</summary>
      public long start { get; set; }

      /// <summary>Ngày hết hạn</summary>
      public long end { get; set; }
    }

    /// <summary>Chức năng → Thay bằng Product</summary>
    public class Module
    {
      public bool okrs { get; set; }
      public bool todo { get; set; }
      public bool kaizen { get; set; }
      public bool educate { get; set; }
    }

      public class Category
      {
        public string id { get; set; }
        public string name { get; set; }
        public string color { get; set; }
      }
  }
}