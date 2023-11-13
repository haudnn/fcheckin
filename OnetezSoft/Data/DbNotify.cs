using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbNotify
  {
    private static string _collection = "notifys";

    public static async Task<NotifyModel> Create(string companyId, NotifyModel model)
    {
      model.id = Guid.NewGuid().ToString();

      if (model.date == 0)
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


    public static async Task<List<NotifyModel>> GetbyKey(string companyId, string key)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var result = await collection.FindAsync(x => x.key == key).Result.ToListAsync();

      return result;
    }

    public static async Task<bool> GetbyKeyAndDelete(string companyId, string key)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var notifications = await GetbyKey(companyId, key);
      if (notifications == null)
        return false;
      else
      {
        foreach (var item in notifications)
        {
          await collection.DeleteOneAsync(x => x.id == item.id);
        }
        return true;
      }
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

      var filtered = builder.Gt("date", DateTime.Now.AddDays(-30).Ticks)
        & builder.Eq("user", user);

      var sorted = Builders<NotifyModel>.Sort.Descending("date");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      await DeleteOverTime(companyId);

      return (from x in result orderby x.date descending select x).ToList();
    }

    public static async Task DeleteOverTime(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var builder = Builders<NotifyModel>.Filter;

      var filtered = builder.Lte("date", DateTime.Now.AddDays(-30).Ticks);

      var sorted = Builders<NotifyModel>.Sort.Descending("date");

      var result = await collection.DeleteManyAsync(filtered);
    }


    public static async Task<List<NotifyModel>> GetListByType(string companyId, List<int> type)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var builder = Builders<NotifyModel>.Filter;

      var result = await collection.FindAsync(x => type.Contains(x.type)).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
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

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }

    /// <summary>
    /// Lấy thông báo mới trong 4p của tất cả user trong công ty
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="user"></param>
    public static async Task<List<NotifyModel>> GetNews(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<NotifyModel>(_collection);

      var builder = Builders<NotifyModel>.Filter;

      var filtered = builder.Gt("date", DateTime.Now.AddMinutes(-4).Ticks);

      var sorted = Builders<NotifyModel>.Sort.Descending("date");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }

    /// <summary>
    /// Lấy thông báo mới trong vào 30s của cms
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="user"></param>
    public static async Task<NotifyModel> GetNewsCMS(string companyId, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<NotifyModel>(_collection);
      var builder = Builders<NotifyModel>.Filter;

      var filtered = builder.Gt("date", DateTime.Now.AddSeconds(-30).Ticks) &
                     builder.Eq("user", user) &
                     builder.Eq("isCMS", true) &
                     builder.Eq("type", 400) &
                     builder.Eq("read", false);

      var latestNotification = await collection.FindAsync(filtered).Result.ToListAsync();
      return latestNotification.OrderByDescending(x => x.date).FirstOrDefault();
    }


    /// <summary>
    /// Mẫu thông báo
    /// </summary>
    public static async Task<NotifyModel> Create(string companyId, int type, string key, string target, string create)
    {
      var user = await DbUser.Get(companyId, create, null);
      var creator = "<b>" + (user != null ? user.FullName : create) + "</b>";
      var name = string.Empty;
      var link = string.Empty;

      #region Các loại thông tạo

      if (type == 9)
      {
        var current = await DbBlog.Get(companyId, key);
        name = $"{creator} đã đăng một tin tức mới";
        link = string.IsNullOrEmpty(current.link) ? $"/blogs/{key}" : current.link;
      }
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
        link = "/configs/system/department/list";
      }
      else if (type == 22)
      {
        name = $"{creator} đã chỉnh sửa thông tin phòng ban <b>{key}</b>";
        link = "/configs/system/department/list";
      }
      else if (type == 23)
      {
        name = $"{creator} đã xóa phòng ban <b>{key}</b>";
        link = "/configs/system/department/list";
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
      else if (type == 201)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã giao cho bạn công việc <b>{current.name}</b>";
        link = "/todolist/receive";
      }
      else if (type == 202)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã xác nhận yêu cầu công việc <b>{current.name}</b>";
        link = "/todolist/send";
      }
      else if (type == 203)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã từ chối yêu cầu công việc <b>{current.name}</b>";
        link = "/todolist/send";
      }
      else if (type == 212)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã cập nhật trạng thái Pending cho công việc <b>{current.name}</b>";
        link = "/todolist/send";
      }
      else if (type == 214)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã cập nhật trạng thái Done cho công việc <b>{current.name}</b>";
        link = "/todolist/send";
      }
      else if (type == 215)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã cập nhật trạng thái Cancel cho công việc <b>{current.name}</b>";
        link = "/todolist/send";
      }
      // ĐỔI QUÀ
      else if (type == 300)
      {
        name = $"{creator} đã yêu cầu đổi quà, cần quản lý phê duyệt.";
        link = "/configs/gift/exchange";
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
      // OKRs
      else if (type == 503)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"{creator} đã tạo bản Checkin nháp. Hãy xác nhận bản checkin nháp của họ nhé!";
        link = $"/okr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 504)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"{creator} đã xác nhận bản checkin nháp của bạn.";
        link = $"/okr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 505)
      {
        var current = await DbOkrCheckin.Get(companyId, key);
        name = $"Đã hoàn tất buổi checkin OKRs cùng {creator}.";
        link = $"/okr/checkin/{current.okr}/info?checkin={key}";
      }
      else if (type == 506)
      {
        name = $"{creator} đã thêm một phản hồi mới về mục tiêu của họ.";
        link = $"/okr/checkin/{key}/feedback";
      }
      else if (type == 507)
      {
        name = $"{creator} đã thêm một phản hồi mới về mục tiêu của bạn.";
        link = $"/okr/checkin/{key}/feedback";
      }
      else if (type == 508)
      {
        var current = await DbOkr.Get(companyId, key);
        name = $"{creator} đã gửi đánh giá OKR. Bạn hãy thực hiện đánh giá nhé!";
        link = $"/okr/review/{current.user_create}/{key}";
      }
      else if (type == 509)
      {
        var current = await DbOkr.Get(companyId, key);
        name = $"Bản đánh giá OKR của bạn đã được {creator} mở lại. Bạn hãy thực hiện lại bản bánh giá nhé!";
        link = $"/okr/review/{current.user_create}/{key}";
      }
      else if (type == 510)
      {
        var current = await DbOkr.Get(companyId, key);
        name = $"{creator} đã xác nhận bản đánh giá OKR của bạn!";
        link = $"/okr/review/{current.user_create}/{key}";
      }
      else if (type == 511)
      {
        var current = await DbOkr.Get(companyId, key);
        name = $"{creator} đã gửi đánh giá OKR. Bạn hãy xem đánh giá nhé!";
        link = $"/okr/review/{current.user_create}/{key}";
      }
      else if (type == 513)
      {
        name = $"{creator} đã thêm bạn vào hành động thuộc OKRs!";
        link = $"/okr/tasks/user/{create}";
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

      else if (type == 800)
      {
        name = $"{creator} đã tạo ca làm <b>{key}</b>";
        link = $"/configs/timekeeping/work-shift";
      }
      else if (type == 801)
      {
        name = $"{creator} đã chỉnh sửa thông tin ca làm <b>{key}</b>";
        link = $"/configs/timekeeping/work-shift";
      }
      else if (type == 802)
      {
        name = $"{creator} đã xóa ca làm <b>{key}</b>";
        link = $"/configs/timekeeping/work-shift";
      }
      else if (type == 803)
      {
        name = $"{creator} đã tạo địa điểm chấm công <b>{key}</b>";
        link = $"/configs/timekeeping/locations";
      }
      else if (type == 804)
      {
        name = $"{creator} đã chỉnh sửa thông tin của <b>{key}</b>";
        link = $"/configs/timekeeping/locations";
      }
      else if (type == 805)
      {
        name = $"{creator} đã xóa địa điểm chấm công <b>{key}</b>";
        link = $"/configs/timekeeping/locations";
      }
      else if (type == 806)
      {
        name = $"{creator} đã chỉnh sửa thông tin <b>{key}</b>";
        link = $"/configs/timekeeping/rules";
      }
      else if (type == 807)
      {
        name = $"{creator} đã tạo bảng công <b>{key}</b>";
        link = $"/hrm/timesheets";
      }
      else if (type == 808)
      {
        name = $"{creator} đã chỉnh sửa thông tin của bảng công <b>{key}</b>";
        link = $"/hrm/timesheets";
      }
      else if (type == 809)
      {
        name = $"{creator} đã xóa bảng công <b>{key}</b>";
        link = $"/hrm/timesheets";
      }

      // Ngày nghỉ - chấm công
      else if (type == 810)
      {
        name = $"{creator} đã tạo mới ngày nghỉ <b>{key}</b>";
        link = $"/configs/timekeeping/dayoff";
      }
      else if (type == 811)
      {
        name = $"{creator} đã chỉnh sửa ngày nghỉ <b>{key}</b>";
        link = $"/configs/timekeeping/dayoff";
      }
      else if (type == 812)
      {
        name = $"{creator} đã xoá ngày nghỉ <b>{key}</b>";
        link = $"/configs/timekeeping/dayoff";
      }

      // đơn từ
      else if (type == 813)
      {
        name = $"{creator} đã đăng ký đơn từ";
        link = $"/hrm/form/2";
      }
      else if (type == 814)
      {
        name = $"{creator} đã {key} đơn từ của bạn";
        link = $"/hrm/form/1";
      }
      else if (type == 815)
      {
        name = $"{creator} đã {key} đơn từ của bạn nhưng có khoảng thời gian bị khoá bảng công";
        link = $"/hrm/form/1";
      }

      // Đăng ký thiết bị
      else if (type == 816)
      {
        name = $"{creator} đã yêu cầu đăng ký thiết bị mới";
        link = $"/configs/timekeeping/device";
      }
      else if (type == 817)
      {
        name = $"{creator} đã phê duyệt yêu cầu đăng ký thiết bị mới của bạn";
      }
      else if (type == 818)
      {
        name = $"{creator} đã từ chối yêu cầu đăng ký thiết bị mới của bạn";
      }
      else if (type == 819)
      {
        name = $"{creator} đã phê duyệt bảng đăng ký ca làm {key}";
        link = $"/hrm/timelist";
      }
      else if (type == 820)
      {
        name = $"{creator} đã yêu cầu phân địa điểm chấm công, vui lòng cân nhắc thực hiện nó";
        link = $"/configs/timekeeping/locationassign";
      }
      else if (type == 821)
      {
        var form = await DbHrmForm.Get(companyId, key);
        var userForm = await DbUser.Get(companyId, form.user, null);
        var userTemp = "<b>" + (userForm != null ? userForm.FullName : key) + "</b>";

        if (form != null)
        {
          if (target == form.user)
            name = $"Đơn từ <b>{form.form.name}</b> của bạn có bình luận mới.";
          else
            name = $"Đơn từ <b>{form.form.name}</b> của {userTemp} có bình luận mới.";

          link = $"/hrm/form/1?form={key}&tab=2";
        }
      }
      else if (type == 822)
      {
        var form = await DbHrmForm.Get(companyId, key);
        var userForm = await DbUser.Get(companyId, form.user, null);
        var userTemp = "<b>" + (userForm != null ? userForm.FullName : key) + "</b>";
        if (form != null)
        {
          if (target == form.user)
            name = $"{creator} đã nhắc đến bạn trong 1 bình luận tại đơn từ <b>{form.form.name}</b>!";
          else
            name = $"{creator} đã nhắc đến bạn trong 1 bình luận tại đơn từ <b>{form.form.name}</b> của <b>{userTemp}</b>!";

          link = $"/hrm/form/1?form={key}&tab=2";
        }
      }

      // CFRs
      else if (type == 900)
      {
        name = Convert.ToInt32(key) > 0 ? $"Bạn đã được cấp {key} sao." : $"Bạn đã bị trừ {key} sao.";
        link = "/cfr";
      }
      else if (type == 901)
      {
        name = $"Bạn đã nhận được ghi nhận từ {creator}.";
        link = "/cfr";
      }
      else if (type == 902)
      {
        name = $"{creator} đã tặng cho bạn {key} sao.";
        link = "/cfr";
      }
      else if (type == 903)
      {
        var cfrType = key == "2" ? "ghi nhận" : "tặng sao";
        name = $"{creator} đã thả tim cho hành động {cfrType} của bạn.";
        link = $"/cfr?type={key}&send=true";
      }


      // KPIs
      // Chu kỳ
      else if (type == 1000)
      {
        name = $"{creator} đã tạo chu kỳ <b>{key}</b>";
        link = $"/configs/kpis/cycle";
      }
      else if (type == 1001)
      {
        name = $"{creator} đã chỉnh sửa thông tin của chu kỳ <b>{key}</b>";
        link = $"/configs/kpis/cycle";
      }
      else if (type == 1002)
      {
        name = $"{creator} đã xóa chu kỳ <b>{key}</b>";
        link = $"/configs/kpis/cycle";
      }
      else if (type == 1003)
      {
        name = $"{creator} đã tạo chỉ số <b>{key}</b>";
        link = $"/configs/kpis/metric";
      }
      else if (type == 1004)
      {
        name = $"{creator} đã chỉnh sửa thông tin của chỉ số <b>{key}</b>";
        link = $"/configs/kpis/metric";
      }
      else if (type == 1005)
      {
        name = $"{creator} đã xóa chỉ số <b>{key}</b>";
        link = $"/configs/kpis/metric";
      }

      // Cây KPIs
      // content[0]: Tên KPIs, content[1]: Tên chu kỳ, content[2]: Id KPIs
      // tạo KPIs
      // Người quản lý và người xem
      else if (type == 1006)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã tạo KPIs {content[0]} trong cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}?id={content[1]}&tab_overview=2";
        }
      }
      // Người giám sát
      else if (type == 1007)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã giao cho bạn KPIs quản lý {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/manager/list?id={content[1]}";
        }
      }
      // Người thực hiện
      else if (type == 1008)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã giao cho bạn KPIs nhập liệu {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/person/{content[1]}";
        }
      }
      // chỉnh sửa KPIs
      // Người quản lý và người xem
      else if (type == 1009)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã chỉnh sửa thông tin của KPIs {content[0]} trong cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}?id={content[1]}&tab_overview=2";
        }
      }
      // Người giám sát
      else if (type == 1010)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã chỉnh sửa thông tin của KPIs quản lý {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/manager/list?id={content[1]}";
        }
      }
      // Người thực hiện
      else if (type == 1011)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã chỉnh sửa thông tin của KPIs nhập liệu {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/person/{content[1]}";
        }
      }
      // xoá KPIs
      // Người quản lý và người xem
      else if (type == 1012)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã xóa KPIs {content[0]} trong cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}?tab_overview=2";
        }
      }
      // Người giám sát
      else if (type == 1013)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã xóa KPIs quản lý {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/manager/list";
        }
      }
      // Người thực hiện
      else if (type == 1014)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã xóa KPIs nhập liệu {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/person/{content[1]}";
        }
      }

      //checkin KPIs
      else if (type == 1015)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã gửi bản check-in của KPIs nhập liệu {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/manager/confirm?id={content[1]}&confirm=true";
        }
      }
      else if (type == 1016)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã cập nhật bản check-in của KPIs nhập liệu {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/manager/confirm?id={content[1]}&confirm=true";
        }
      }
      // Phê duyệt
      else if (type == 1017)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã xác nhận bản check-in của KPIs {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/person/{content[1]}";
        }
      }

      // tạo cây kpis trong 1 chu kỳ 
      // quản lý chu kỳ
      else if (type == 1018)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} tạo cây KPIs {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}";
        }
      }
      // quản lý cây kpis
      else if (type == 1019)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã cấp cho bạn quyền quản lý cây KPIs {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}";
        }
      }
      // quản lý người xem
      else if (type == 1020)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã cấp cho bạn quyền người xem cây KPIs {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}";
        }
      }
      // chỉnh sửa cây kpis
      else if (type == 1021)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã chỉnh sửa thông tin của cây KPIs {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}";
        }
      }
      // xoá cây KPIs
      else if (type == 1022)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã xóa cây KPIs {content[0]} trong chu kỳ {content[4]}";
          link = $"/kpis/root";
        }
      }

      // bình luận KPIs
      // người tham gia bình luận nhập liệu
      else if (type == 1023)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã bình luận trong KPIs nhập liệu {content[0]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/person/{content[1]}?tab=2";
        }
      }
      // người tham gia bình luận giám sát
      else if (type == 1024)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã bình luận trong KPIs tự động {content[0]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/manager/list?id={content[1]}&tab=3";
        }
      }
      // quản lý phía trên
      else if (type == 1025)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã bình luận tại KPIs {content[0]} của cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/manager/list?id={content[1]}&tab=3";
        }
      }
      // quản lý cây kpis, quản lý chu kỳ, người xem
      else if (type == 1026)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã bình luận tại KPIs {content[0]} của cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}?id={content[1]}&tab=3&tab_overview=2";
        }
      }

      // người được tag trong bình luận
      // người sở hữu nhập liệu
      else if (type == 1027)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã nhắc đến bạn trong 1 bình luận tại KPIs {content[0]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/person/{content[1]}?tab=2";
        }
      }
      // người sở hữu giám sát
      else if (type == 1028)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã nhắc đến bạn trong 1 bình luận tại KPIs {content[0]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/manager/list?id={content[1]}&tab=3";
        }
      }
      // quản lý phía trên
      else if (type == 1029)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã nhắc đến bạn trong 1 bình luận tại KPIs {content[0]} của cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/manager/list?id={content[1]}&tab=3";
        }
      }
      // quản lý kpis, quản lý chu kỳ, người xem
      else if (type == 1030)
      {
        var content = key.Split("@@@");
        if (content.Length >= 6)
        {
          name = $"{creator} đã nhắc đến bạn trong 1 bình luận tại KPIs {content[0]} của cây KPIs {content[2]} thuộc chu kỳ {content[4]}";
          link = $"/kpis/root/{content[3]}?id={content[1]}&tab=3&tab_overview=2";
        }
      }
      #endregion

      if (create != target && !string.IsNullOrEmpty(name))
      {
        var model = new NotifyModel
        {
          name = name,
          link = link,
          user = target,
          type = type,
          key = key,
          user_send = user.id
        };

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
      var user = await DbUser.Get(companyId, create, null);
      var creator = "<b>" + (user != null ? user.FullName : create) + "</b>";
      var name = string.Empty;
      var link = string.Empty;

      var plan = await DbWorkPlan.Get(companyId, planId);

      if (plan == null)
        return null;

      if (type == 700)
      {
        name = $"Kế hoạch <b>{plan.name}</b> đã được tạo bởi {creator}";
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
        name = $"<b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã được thêm bởi {creator}.";
        link = $"/work/{planId}/task";
      }
      else if (type == 703)
      {
        var data = plan.sections.SingleOrDefault(x => x.id == itemId);
        name = $"<b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã được cập nhật tiêu đề mới.";
        link = $"/work/{planId}/task";
      }
      else if (type == 704)
      {
        var data = plan.sections.SingleOrDefault(x => x.id == itemId);
        name = $"<b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã bị xóa bởi {creator}.";
        link = $"/work/{planId}/task";
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
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã thêm mới bởi {creator}.";
        link = $"/work/{planId}/task?task={task.id}&tab=1";
      }
      else if (type == 709)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã cập nhật tiêu đề mới.";
        link = $"/work/{planId}/task?task={task.id}&tab=1";
      }
      else if (type == 710)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã cập nhật trạng thái Done.";
        link = $"/work/{planId}/task?task={task.id}&tab=1";
      }
      else if (type == 711)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b> cần được bạn Review.";
        link = $"/work/{planId}/task?task={task.id}&tab=1";
      }
      else if (type == 712)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b> có bình luận mới.";
        link = $"/work/{planId}/task?task={task.id}&tab=4";
      }
      else if (type == 713)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Bạn đã được thêm vào công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task?task={task.id}&tab=1";
      }
      else if (type == 714)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"Bạn đã bị xóa khỏi công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task";
      }
      else if (type == 715)
      {
        var child = await DbWorkTask.Get(companyId, itemId);
        var task = await DbWorkTask.Get(companyId, child.parent_id);
        name = $"Bạn đã được thêm vào công việc phụ <b>{child.name}</b> của công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task?task={task.id}&tab=2";
      }
      else if (type == 716)
      {
        var child = await DbWorkTask.Get(companyId, itemId);
        var task = await DbWorkTask.Get(companyId, child.parent_id);
        name = $"Bạn đã bị xóa khỏi công việc phụ <b>{child.name}</b> của công việc <b>{task.name}</b> thuộc kế hoạch <b>{plan.name}</b>.";
        link = $"/work/{planId}/task?task={task.id}&tab=2";
      }
      else if (type == 717)
      {
        var data = await DbWorkTask.Get(companyId, itemId);
        name = $"Công việc <b>{data.name}</b> thuộc kế hoạch <b>{plan.name}</b> đã bị xóa bởi {creator}.";
        link = $"/work/{planId}/task";
      }
      else if (type == 718)
      {
        name = $"<b>{creator}</b> đã rời khỏi kế hoạch <b>{plan.name}</b>!";
        link = $"/work/{planId}";
      }
      else if (type == 719)
      {
        name = $"<b>{creator}</b> đã phân quyền bạn trở thành quản lý tại kế hoạch <b>{plan.name}</b>.";
        link = "/work/" + planId;
      }
      else if (type == 720)
      {
        var data = WorkService.StatusPlan(Convert.ToInt32(itemId));
        name = $"Kế hoạch <b>{plan.name}</b> đã được chuyển trạng thái thành <b>{data.name}</b>.";
        link = $"/work/{planId}";
      }
      // Tag tên kế hoạch
      else if (type == 721)
      {
        var task = await DbWorkTask.Get(companyId, itemId);
        name = $"<b>{creator}</b> đã nhắc đến bạn trong 1 bình luận tại kế hoạch <b>{plan.name}</b>!";
        link = $"/work/{planId}/task?task={task.id}&tab=4";
      }

      if (create != target)
      {
        var model = new NotifyModel();
        model.name = name;
        model.link = link;
        model.user = target;
        model.type = type;
        model.key = planId;
        model.user_send = user.id;

        return await Create(companyId, model);
      }
      else
        return null;
    }


    public static StaticModel Type(int type)
    {
      var result = new StaticModel();
      result.id = type;

      if (0 < type && type < 100)
      {
        result.name = "Khác";
        result.color = "is-dark";
      }
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
      else if (700 <= type && type < 800)
      {
        result.name = "Kế hoạch";
        result.color = "is-link";
      }
      else if (800 <= type && type < 900)
      {
        result.name = "HRM";
        result.color = "is-link";
      }
      else if (900 <= type && type < 999)
      {
        result.name = "Chấm công";
        result.color = "is-link";
      }
      else if (1000 <= type && type < 1099)
      {
        result.name = "KPIs";
        result.color = "is-link";
      }


      return result;
    }
  }
}