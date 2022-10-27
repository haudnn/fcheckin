﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbNotify
  {
    private static string _collection = "notifys";

    public static async Task<NotifyModel> Create(string companyId, NotifyModel model)
    {
      model.id = Guid.NewGuid().ToString();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<NotifyModel> Update(string companyId, NotifyModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<NotifyModel> GetbyKey(string companyId, string key)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var result = await collection.Find(x => x.key == key).FirstOrDefaultAsync();

      return result;
    }


    /// <summary>
    /// Lấy thông báo của một người
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="user"></param>
    public static async Task<List<NotifyModel>> GetList(string companyId, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var builder = Builders<NotifyModel>.Filter;

      var filtered = builder.Gt("date", DateTime.Now.AddDays(-14).Ticks)
        & builder.Eq("user", user);

      var sorted = Builders<NotifyModel>.Sort.Descending("date");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }


    /// <summary>
    /// Lấy thông báo mới trong vào 30s của một người
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="user"></param>
    public static async Task<List<NotifyModel>> GetNews(string companyId, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var builder = Builders<NotifyModel>.Filter;

      var filtered = builder.Gt("date", DateTime.Now.AddSeconds(-30).Ticks)
        & builder.Eq("user", user);

      var sorted = Builders<NotifyModel>.Sort.Descending("date");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }


    /// <summary>
    /// Mẫu thông báo
    /// </summary>
    public static async Task<NotifyModel> Create(string companyId, int type, string key, string target, string create)
    {
      var user = await DbUser.Get(companyId, create);
      var creator = "<b>" + (user != null ? user.FullName : create) + "</b>";
      var name = string.Empty;
      var link = string.Empty;

      #region Các loại thông tạo

      if (type == 10)
      {
        var current = await DbDepartment.Get(companyId, key);
        name = $"{creator} đã thêm bạn vào phòng ban <b>{current.name}</b>";
      }
      else if (type == 11)
      {
        var current = await DbDepartment.Get(companyId, key);
        name = $"{creator} đã thêm bạn thành {current.manager} <b>{current.name}</b>";
      }
      else if (type == 12)
      {
        var current = await DbDepartment.Get(companyId, key);
        name = $"{creator} đã thêm bạn thành {current.deputy} <b>{current.name}</b>";
      }
      else if (type == 20)
      {
        var current = await DbMainCompany.Get(companyId);
        name = $"{creator} đã thay đổi cấu hình thời gian của Todolist (check-in: {current.todolist.time_checkin}, check-out: {current.todolist.time_checkout})";
      }
      else if (type == 21)
      {
        name = $"{creator} đã thêm một phòng ban <b>{key}</b>";
        link = "/config/system/department/list";
      }
      else if (type == 22)
      {
        name = $"{creator} đã chỉnh sửa thông tin phòng ban <b>{key}</b>";
        link = "/config/system/department/list";
      }
      else if (type == 23)
      {
        name = $"{creator} đã xóa phòng ban <b>{key}</b>";
        link = "/config/system/department/list";
      }
      else if (type == 30)
      {
        name = "<b>Góp ý hệ thống</b> của bạn có phản hồi mới.";
        link = $"/feedback/{key}";
      }
      else if (type == 100)
      {
        name = $"{creator} đã đăng tải một góp ý Kaizen mới. Hãy phản hồi họ nhé!";
        link = $"/kaizen/{key}";
      }
      else if (type == 101)
      {
        name = $"Góp ý Kaizen của bạn đã bị gỡ.";
        link = "/kaizen";
      }
      else if (type == 102)
      {
        name = $"{creator} đã phản hồi góp ý Kaizen của bạn!";
        link = $"/kaizen/{key}";
      }
      else if (type == 103)
      {
        name = $"Phản hồi Kaizen của bạn đã bị xóa!";
        link = $"/kaizen/{key}";
      }
      else if (type == 104)
      {
        var current = await DbKaizen.Get(companyId, key);
        var status = DbKaizen.Status(current.status);
        name = $"Góp ý Kaizen của bạn được đánh giá là <b>{status.name}</b>";
        link = $"/kaizen/{key}";
      }
      else if (type == 105)
      {
        name = $"{creator} đã đánh giá một góp ý Kaizen.";
        link = $"/kaizen/{key}";
      }
      else if (type == 106)
      {
        name = $"{creator} đã xoá một góp ý Kaizen.";
        link = "/kaizen";
      }
      // TODOLIST
      else if (type == 200)
      {
        var current = await DbTodolist.Get(companyId, key);
        name = $"{creator} đã xác nhận Todolist của bạn";
        link = "/todolist/" + string.Format("{0:yyyy-MM-dd}", new DateTime(current.date));
      }
      // ĐỔI QUÀ
      else if (type == 300)
      {
        name = $"{creator} đã yêu cầu đổi quà, cần quản lý phê duyệt.";
        link = "/config/gift/exchange";
      }
      else if (type == 301)
      {
        name = $"{creator} đã phê duyệt yêu cầu đổi quà của bạn.";
        link = "/gift/exchange";
      }
      else if (type == 302)
      {
        name = $"{creator} đã từ chối yêu cầu đổi quà của bạn.";
        link = "/gift/exchange";
      }
      else if (type == 303)
      {
        name = $"Bạn đã nhận được một món quà từ {creator}. Hãy khám phá xem đây là món quà gì nào?";
        link = "/gift/exchange?tab=give";
      }
      // CFRS
      else if (type == 500)
      {
        name = Convert.ToInt32(key) > 0 ? $"Bạn đã được cấp {key} sao." : $"Bạn đã bị trừ {key} sao.";
        link = "/cfr/star";
      }
      else if (type == 501)
      {
        name = $"Bạn đã nhận được ghi nhận từ {creator}.";
        link = "/cfr/star";
      }
      else if (type == 502)
      {
        name = $"{creator} đã tặng cho bạn {key} sao.";
        link = "/cfr/star";
      }
      else if (type == 503)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"{creator} đã tạo bản Checkin nháp. Hãy xác nhận bản checkin nháp của họ nhé!";
        link = $"/cfr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 504)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"{creator} đã xác nhận bản checkin nháp của bạn.";
        link = $"/cfr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 505)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"Đã hoàn tất buổi checkin OKRs cùng {creator}.";
        link = $"/cfr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 506)
      {
        name = $"{creator} đã thêm một phản hồi mới về mục tiêu của họ.";
        link = $"/cfr/checkin/{key}/feedback";
      }
      else if (type == 507)
      {
        name = $"{creator} đã thêm một phản hồi mới về mục tiêu của bạn.";
        link = $"/cfr/checkin/{key}/feedback";
      }
      // ĐÀO TẠO
      else if (type == 600)
      {
        var current = await DbEducateLearned.Get(companyId, key);
        name = $"Bạn đã nhận được chứng chỉ cho khóa học '{current.course_name}'";
        link = "/educate/certificate/list";
      }
      else if (type == 601)
      {
        var current = await DbEducateExam.Get(companyId, key);
        name = $"{creator} đã nộp bài thi của khóa học '{current.course_name}'";
        link = "/educate/exam/manager/" + key;
      }
      else if (type == 602)
      {
        var current = await DbEducateCourse.Get(companyId, key);
        name = $"Bạn được quyền quản lý khoá học <b>{current.name}</b> từ {creator}";
        link = "/educate/course/manager/info/" + key;
      }
      else if (type == 603)
      {
        var current = await DbEducateExam.Get(companyId, key);
        name = $"Bài thi của bạn ở Khoá học '{current.course_name}' đã được chấm.";
        link = "/educate/course/list/learn/" + current.lesson;
      }

      #endregion

      if (create != target)
      {
        var model = new NotifyModel();
        model.name = name;
        model.link = link;
        model.user = target;
        model.type = type;
        model.key = key;

        return await Create(companyId, model);
      }
      else
        return null;
    }

    /// <summary>
    /// Thông báo dành riêng cho kế hoạch
    /// </summary>
    public static async Task<NotifyModel> ForPlan(string companyId, int type, string planId, string itemId, string target, string create)
    {
      var user = await DbUser.Get(companyId, create);
      var creator = "<b>" + (user != null ? user.FullName : create) + "</b>";
      var name = string.Empty;
      var link = string.Empty;

      var plan = await DbWorkPlan.Get(companyId, planId);
      if (type == 700)
      {
        name = $"Kế hoạch <b>{plan.name}</b> đã được tạo bởi {create}";
        link = "/work/" + planId;
      }
      else if (type == 701)
      {
        name = $"Kế hoạch <b>{plan.name}</b> đã được cập nhật tiêu đề mới.";
        link = "/work/" + planId;
      }
      else if (type == 702)
      {
        var data = plan.sections.SingleOrDefault(x => x.id == itemId);
        name = $"<b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã được thêm mới.";
        link = "/work/" + planId;
      }
      else if (type == 703)
      {
        var data = plan.sections.SingleOrDefault(x => x.id == itemId);
        name = $"<b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã được cập nhật tiêu đề mới.";
        link = "/work/" + planId;
      }
      else if (type == 704)
      {
        var data = plan.sections.SingleOrDefault(x => x.id == itemId);
        name = $"<b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã bị xóa bởi {creator}.";
        link = "/work/" + planId;
      }
      else if (type == 705)
      {
        name = $"Kế hoạch <b>{plan.name}</b> đã bị xóa bởi {creator}.";
        link = "/work/" + planId;
      }
      else if (type == 706)
      {
        name = $"<b>{creator}</b> đã thêm bạn vào kế hoạch <b>{plan.name}</b>.";
        link = "/work/" + planId;
      }
      else if (type == 707)
      {
        name = $"<b>{creator}</b> đã xóa bạn khỏi kế hoạch <b>{plan.name}</b>.";
      }
      else if (type == 708)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã thêm mới bởi {creator}.";
        link = $"/work/{planId}/task";
      }
      else if (type == 709)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã cập nhật tiêu đề mới.";
        link = $"/work/{planId}/task";
      }
      else if (type == 710)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã cập nhật trạng thái Done.";
        link = $"/work/{planId}/task";
      }
      else if (type == 711)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> cần được bạn Review.";
        link = $"/work/{planId}/task";
      }
      else if (type == 712)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> có bình luận mới.";
        link = $"/work/{planId}/task";
      }
      else if (type == 713)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Bạn đã được thêm vào công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task";
      }
      else if (type == 714)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Bạn đã bị xóa khỏi công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task";
      }
      else if (type == 715)
      {
        var child = await DbWorkTask.Get(companyId, itemId);
        var task = await DbWorkTask.Get(companyId, child.parent_id);
        name = $"Bạn đã được thêm vào công việc phụ <b>{child.name}</b> của công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task";
      }
      else if (type == 716)
      {
        var child = await DbWorkTask.Get(companyId, itemId);
        var task = await DbWorkTask.Get(companyId, child.parent_id);
        name = $"Bạn đã bị xóa khỏi công việc phụ <b>{child.name}</b> của công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task";
      }
      else if (type == 717)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã bị xóa bởi {creator}.";
        link = $"/work/{planId}/task";
      }

      if (create != target)
      {
        var model = new NotifyModel();
        model.name = name;
        model.link = link;
        model.user = target;
        model.type = type;
        model.key = planId;

        return await Create(companyId, model);
      }
      else
        return null;
    }


    public static StaticModel Type(int type)
    {
      var result = new StaticModel();
      result.id = type;

      if (100 <= type && type < 200)
      {
        result.name = "Kaizen";
        result.color = "is-primary";
      }
      else if (200 <= type && type < 300)
      {
        result.name = "TodoList";
        result.color = "is-link";
      }
      else if (300 <= type && type < 400)
      {
        result.name = "Đổi quà";
        result.color = "is-success";
      }
      else if (400 <= type && type < 500)
      {
        result.name = "OKRs";
        result.color = "is-danger";
      }
      else if (500 <= type && type < 600)
      {
        result.name = "CFRs";
        result.color = "is-warning";
      }
      else if (600 <= type && type < 700)
      {
        result.name = "Đào Tạo";
        result.color = "is-warning";
      }
      else if (0 < type && type < 100)
      {
        result.name = "Khác";
        result.color = "is-dark";
      }


      return result;
    }
  }
}
