using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  public class TodolistModel
  {
    [BsonId]
    public string id { get; set; }
    // Ngày của todolist
    public long date { get; set; }
    // Người tạo
    public string user_create { get; set; }
    // Ngày tạo
    public long date_create { get; set; }
    // Ngày checkin
    public long date_checkin { get; set; }
    // Ngày checkout
    public long date_checkout { get; set; }
    // Ngày xác nhận
    public long date_confirm { get; set; }
    // Tình trạng checkin
    public bool ontime_checkin { get; set; }
    // Tình trạng checkout
    public bool ontime_checkout { get; set; }
    // Ngày nghỉ/ngày thường
    public bool day_off { get; set; }
    // Người xác nhận  = auto → tự động xác nhận
    public string user_confirm { get; set; }
    // Trạng thái: 1. Mới tạo, 2. Đã checkin, 3. Đã checkout
    public int status { get; set; }
    // Điểm để xếp hạng
    public int point { get; set; }
    // Danh sách công việc
    public List<Todo> todos { get; set; }

    public class Todo
    {
      public string id { get; set; }
      // Tên công việc
      public string name { get; set; }
      // Chi tiết
      public string detail { get; set; }
      // Kết quả
      public string result { get; set; }
      // Bắt đầu
      public string start { get; set; }
      // Kết thúc
      public string end { get; set; }
      // Phân loại
      public int type { get; set; }
      // Độ ưu tiên
      public int level { get; set; }
      // Trạng thái
      public int status { get; set; }
      // Đã được xác nhận
      public bool confirm { get; set; }
    }
  }
}