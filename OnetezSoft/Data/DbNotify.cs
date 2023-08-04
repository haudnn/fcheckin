using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Services;

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
      else if (type == 201)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã giao cho bạn công việc <b>{current.name}</b>";
        link = "/todolist";
      }
      else if (type == 202)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã xác nhận yêu cầu công việc <b>{current.name}</b>";
        link = "/todolist#assigned_list";
      }
      else if (type == 203)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã từ chối yêu cầu công việc <b>{current.name}</b>";
        link = "/todolist#assigned_list";
      }
      else if (type == 212)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã cập nhật trạng thái Pending cho công việc <b>{current.name}</b>";
        link = "/todolist#assigned_list";
      }
      else if (type == 214)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã cập nhật trạng thái Done cho công việc <b>{current.name}</b>";
        link = "/todolist#assigned_list";
      }
      else if (type == 215)
      {
        var current = await DbTodoItem.Get(companyId, key);
        name = $"{creator} đã cập nhật trạng thái Cancel cho công việc <b>{current.name}</b>";
        link = "/todolist#assigned_list";
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
      // OKRs & CFRS
      else if (type == 500)
      {
        name = Convert.ToInt32(key) > 0 ? $"Bạn đã được cấp {key} sao." : $"Bạn đã bị trừ {key} sao.";
        link = "/cfr";
      }
      else if (type == 501)
      {
        name = $"Bạn đã nhận được ghi nhận từ {creator}.";
        link = "/cfr";
      }
      else if (type == 502)
      {
        name = $"{creator} đã tặng cho bạn {key} sao.";
        link = "/cfr";
      }
      else if (type == 512)
      {
        var cfrType = key == "2" ? "ghi nhận" : "tặng sao";
        name = $"{creator} đã thả tim cho hành động {cfrType} của bạn.";
        link = $"/cfr?type={key}&send=true";
      }
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
        link = $"/hrm/setup/work-shift";
      }
      else if (type == 801)
      {
        name = $"{creator} đã chỉnh sửa thông tin ca làm <b>{key}</b>";
        link = $"/hrm/setup/work-shift";
      }
      else if (type == 802)
      {
        name = $"{creator} đã xóa ca làm <b>{key}</b>";
        link = $"/hrm/setup/work-shift";
      }
      else if (type == 803)
      {
        name = $"{creator} đã tạo địa điểm chấm công <b>{key}</b>";
        link = $"/hrm/setup/locations";
      }
      else if (type == 804)
      {
        name = $"{creator} đã chỉnh sửa thông tin của <b>{key}</b>";
        link = $"/hrm/setup/locations";
      }
      else if (type == 805)
      {
        name = $"{creator} đã xóa địa điểm chấm công <b>{key}</b>";
        link = $"/hrm/setup/locations";
      }
      else if (type == 806)
      {
        name = $"{creator} đã chỉnh sửa thông tin <b>{key}</b>";
        link = $"/hrm/setup/rules";
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
        link = $"/hrm/setup/dayoff";
      }
      else if (type == 811)
      {
        name = $"{creator} đã chỉnh sửa ngày nghỉ <b>{key}</b>";
        link = $"/hrm/setup/dayoff";
      }
      else if (type == 812)
      {
        name = $"{creator} đã xoá ngày nghỉ <b>{key}</b>";
        link = $"/hrm/setup/dayoff";
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
        link = $"/hrm/setup/device";
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

      #endregion

      if (create != target && !string.IsNullOrEmpty(name))
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


      return result;
    }
  }
}
