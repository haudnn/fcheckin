using System;
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
      var creator = await DbUser.Get(companyId, create);
      var creatorName = "<b>" + (creator != null ? creator.FullName : create) + "</b>";
      var name = string.Empty;
      var link = string.Empty;

      #region Các loại thông tạo

      if (type == 10)
      {
        var current = await DbDepartment.Get(companyId, key);
        name = $"{creatorName} đã thêm bạn vào phòng ban <b>{current.name}</b>";
      }
      else if (type == 11)
      {
        var current = await DbDepartment.Get(companyId, key);
        name = $"{creatorName} đã thêm bạn thành {current.manager} <b>{current.name}</b>";
      }
      else if (type == 12)
      {
        var current = await DbDepartment.Get(companyId, key);
        name = $"{creatorName} đã thêm bạn thành {current.deputy} <b>{current.name}</b>";
      }
      else if (type == 20)
      {
        var current = await DbMainCompany.Get(companyId);
        name = $"{creatorName} đã thay đổi cấu hình thời gian của Todolist (check-in: {current.todolist.time_checkin}, check-out: {current.todolist.time_checkout})";
      }
      else if (type == 21)
      {
        name = $"{creatorName} đã thêm một phòng ban <b>{key}</b>";
        link = "/config/system/department/list";
      }
      else if (type == 22)
      {
        name = $"{creatorName} đã chỉnh sửa thông tin phòng ban <b>{key}</b>";
        link = "/config/system/department/list";
      }
      else if (type == 23)
      {
        name = $"{creatorName} đã xóa phòng ban <b>{key}</b>";
        link = "/config/system/department/list";
      }
      else if (type == 30)
      {
        name = "<b>Góp ý hệ thống</b> của bạn có phản hồi mới.";
        link = $"/feedback/{key}";
      }
      else if (type == 100)
      {
        name = $"{creatorName} đã đăng tải một góp ý Kaizen mới. Hãy phản hồi họ nhé!";
        link = $"/kaizen/{key}";
      }
      else if (type == 101)
      {
        name = $"Góp ý Kaizen của bạn đã bị gỡ.";
        link = "/kaizen";
      }
      else if (type == 102)
      {
        name = $"{creatorName} đã phản hồi góp ý Kaizen của bạn!";
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
        name = $"{creatorName} đã đánh giá một góp ý Kaizen.";
        link = $"/kaizen/{key}";
      }
      else if (type == 106)
      {
        name = $"{creatorName} đã xoá một góp ý Kaizen.";
        link = "/kaizen";
      }
      else if (type == 200)
      {
        var current = await DbTodolist.Get(companyId, key);
        name = $"{creatorName} đã xác nhận Todolist của bạn";
        link = "/todolist/" + string.Format("{0:yyyy-MM-dd}", new DateTime(current.date));
      }
      else if (type == 300)
      {
        name = $"{creatorName} đã yêu cầu đổi quà, cần quản lý phê duyệt.";
        link = "/config/gift/exchange";
      }
      else if (type == 301)
      {
        name = $"{creatorName} đã phê duyệt yêu cầu đổi quà của bạn.";
        link = "/gift/exchange";
      }
      else if (type == 302)
      {
        name = $"{creatorName} đã từ chối yêu cầu đổi quà của bạn.";
        link = "/gift/exchange";
      }
      else if (type == 303)
      {
        name = $"Bạn đã nhận được một món quà từ {creatorName}. Hãy khám phá xem đây là món quà gì nào?";
        link = "/gift/exchange?tab=give";
      }
      else if (type == 500)
      {
        name = Convert.ToInt32(key) > 0 ? $"Bạn đã được cấp {key} sao." : $"Bạn đã bị trừ {key} sao.";
        link = "/cfr/star";
      }
      else if (type == 501)
      {
        name = $"Bạn đã nhận được ghi nhận từ {creatorName}.";
        link = "/cfr/star";
      }
      else if (type == 502)
      {
        name = $"{creatorName} đã tặng cho bạn {key} sao.";
        link = "/cfr/star";
      }
      else if (type == 503)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"{creatorName} đã tạo bản Checkin nháp. Hãy xác nhận bản checkin nháp của họ nhé!";
        link = $"/cfr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 504)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"{creatorName} đã xác nhận bản checkin nháp của bạn.";
        link = $"/cfr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 505)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"Đã hoàn tất buổi checkin OKRs cùng {creatorName}.";
        link = $"/cfr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 506)
      {
        name = $"{creatorName} đã thêm một phản hồi mới về mục tiêu của họ.";
        link = $"/cfr/checkin/{key}/feedback";
      }
      else if (type == 507)
      {
        name = $"{creatorName} đã thêm một phản hồi mới về mục tiêu của bạn.";
        link = $"/cfr/checkin/{key}/feedback";
      }
      else if (type == 600)
      {
        var current = await DbEducateLearned.Get(companyId, key);
        name = $"Bạn đã nhận được chứng chỉ cho khóa học '{current.course_name}'";
        link = "/educate/certificate/list";
      }
      else if (type == 601)
      {
        var current = await DbEducateExam.Get(companyId, key);
        name = $"{creatorName} đã nộp bài thi của khóa học '{current.course_name}'";
        link = "/educate/exam/manager/" + key;
      }
      else if (type == 602)
      {
        var current = await DbEducateCourse.Get(companyId, key);
        name = $"Bạn được quyền quản lý khoá học <b>{current.name}</b> từ {creatorName}";
        link = "/educate/course/manager/info/" + key;
      }
      else if (type == 603)
      {
        var current = await DbEducateExam.Get(companyId, key);
        name = $"Bài thi của bạn ở Khoá học '{current.course_name}' đã được chấm.";
        link = "/educate/course/list/learn/" + current.lesson;
      }

      #endregion

      if (!string.IsNullOrEmpty(name))
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
