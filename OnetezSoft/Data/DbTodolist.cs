using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbTodolist
  {
    private static string _collection = "todolist";

    public static async Task<TodolistModel> Create(string companyId, TodolistModel model)
    {
      var date = string.Format("{0:yyMMdd}", new DateTime(model.date));
      model.id = date + model.user_create;
      model.date_create = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<TodolistModel> Update(string companyId, TodolistModel model)
    {
      int point = 10;

      var day = string.Format("{0:yyyy-MM-dd}", new DateTime(model.date));

      var config = await DbMainCompany.Get(companyId);
      var checkin = Convert.ToDateTime(day + " " + config.todolist.time_checkin).AddDays(config.todolist.day_checkin);
      var checkout = Convert.ToDateTime(day + " " + config.todolist.time_checkout);

      if (model.status >= 2 && model.date_checkin <= checkin.Ticks)
      {
        // Checkin đúng hạn
        model.ontime_checkin = true;
      }
      else
      {
        // Checkin trễ hạn
        model.ontime_checkin = false;
        point -= 2;
      }

      if (model.status == 3 && model.date_checkout <= checkout.Ticks)
      {
        // Checkout trễ hạn
        model.ontime_checkout = true;
      }
      else
      {
        // Checkout trễ hạn
        model.ontime_checkout = false;
        point -= 2;
      }

      // Todo chưa hoàn thành thì trừ điểm
      var todoItems = await DbTodoItem.GetList(companyId, model.id);
      model.total = todoItems.Count;
      model.done = todoItems.Where(x => x.status == 4).Count();
      point -= todoItems.Where(x => x.status < 4).Count();

      // Cập nhật điểm Todolist
      model.point = point > 0 ? point : 0;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<TodolistModel> UpdateData(string companyId, TodolistModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<TodolistModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }


    public static async Task<TodolistModel> GetbyDay(string companyId, string user, DateTime date)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var builder = Builders<TodolistModel>.Filter;

      var filtered = builder.Eq("user_create", user)
         & builder.Eq("date", date.Ticks);

      var result = await collection.FindAsync(filtered).Result.FirstOrDefaultAsync();

      // Chuẩn hóa dữ liệu cũ và mới
      if (result != null && result.todos != null && result.todos.Count > 0)
      {
        foreach (var item in result.todos)
        {
          item.date = result.date;
          item.user = result.user_create;
          item.todolist = result.id;
          await DbTodoItem.Create(companyId, item);
        }
        result.todos = null;
        await UpdateData(companyId, result);
      }

      return result;
    }


    public static async Task<List<TodolistModel>> GetList(string companyId, string user, DateTime? start, DateTime? end, bool isCalendar = false)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var builder = Builders<TodolistModel>.Filter;


      var filtered = builder.Eq("user_create", user);
      if (!isCalendar)
        filtered &= builder.Gt("status", 1);

      if (start != null)
        filtered &= builder.Gte("date", start.Value.Ticks);
      if (end != null)
        filtered &= builder.Lt("date", end.Value.AddDays(1).Ticks);

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }


    public static async Task<List<TodolistModel>> GetList(string companyId, DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var builder = Builders<TodolistModel>.Filter;

      var filtered = builder.Gt("status", 1);

      if (start != null)
        filtered = filtered & builder.Gte("date", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date", end.Value.AddDays(1).Ticks);

      var sorted = Builders<TodolistModel>.Sort.Descending("date");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return result.OrderByDescending(x => x.date).ToList();
    }


    /// <summary>
    /// Hàm lấy dữ liệu để tính Thành tựu
    /// </summary>
    public static async Task<int> DataAchievement(string companyId, string user, DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var builder = Builders<TodolistModel>.Filter;

      var filtered = builder.Eq("user_create", user)
        & builder.Gte("date", start.Ticks)
        & builder.Lt("date", end.AddDays(1).Ticks)
        & builder.Gte("status", 2)
        & builder.Eq("ontime_checkin", true);

      var database = await collection.FindAsync(filtered).Result.ToListAsync();
      var daysOff = await DbDayOff.GetAll(companyId);

      int day = 0;
      var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
      for (DateTime date = today; date >= start; date = date.AddDays(-1))
      {
        // Kiểm tra nếu không phải ngày nghỉ
        if (DbDayOff.CheckOff(daysOff, date) == false)
        {
          var item = database.SingleOrDefault(x => x.date == date.Ticks);
          if (item != null)
          {
            // Nếu hôm nay chưa checkout thì chưa tính hô nay
            if (item.status == 2 && item.date == today.Ticks)
              continue;
            // Nếu đã checkout và đúng hạn thì cộng ngày
            else if (item.status == 3 & item.ontime_checkout)
              day++;
            else
              break;
          }
          else
            break;
        }
      }

      return day;
    }




    /// <summary>
    /// Tính thành tựu Todolist
    /// </summary>
    public static async Task<bool> Achievement(string companyId, string user, GlobalService globalService)
    {
      Handled.Shared.GetTimeSpan(2, out DateTime start, out DateTime end);

      var day = await DataAchievement(companyId, user, start, end);

      var achievement = await DbAchievement.GetOption(companyId, "todolist", day);

      if (achievement != null)
      {
        var model = new AchievementModel()
        {
          user = user,
          name = achievement.name,
          desc = achievement.des,
          star = achievement.star,
          type_id = achievement.id,
          type = "todolist",
        };
        await DbAchievement.Create(companyId, model, globalService);
        return true;
      }
      return false;
    }


    #region Dữ liệu cố định

    /// <summary>
    /// Trạng thái: danh sách
    /// </summary>
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Todo",
        color = "",
        icon = "#fff"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Pending",
        color = "is-warning",
        icon = "#ffe08a"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Doing",
        color = "is-link",
        icon = "#355caa"
      });

      list.Add(new StaticModel
      {
        id = 4,
        name = "Done",
        color = "is-success",
        icon = "#48c78e"
      });

      list.Add(new StaticModel
      {
        id = 5,
        name = "Cancel",
        color = "is-dark",
        icon = "#363636"
      });

      return list;
    }

    /// <summary>
    /// Trạng thái: chi tiết
    /// </summary>
    public static StaticModel Status(int id)
    {
      var query = from s in Status()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel() { name = "Tất cả" };
    }


    /// <summary>
    /// Độ ưu tiên: danh sách
    /// </summary>
    public static List<StaticModel> Level()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Bình thường",
        color = "has-text-success",
        icon = "#48c78e"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Quan trọng",
        color = "has-text-warning",
        icon = "#ffe08a"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Rất quan trọng",
        color = "has-text-danger",
        icon = "#f14668"
      });

      return list;
    }

    /// <summary>
    /// Độ ưu tiên: chi tiết
    /// </summary>
    public static StaticModel Level(int id)
    {
      var query = from s in Level()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    /// <summary>
    /// Phân loại: danh sách
    /// </summary>
    public static List<StaticModel> Type()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Kế hoạch",
        color = "",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Cấp trên giao",
        color = "has-text-link",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Đột xuất",
        color = "has-text-danger",
      });

      return list;
    }

    /// <summary>
    /// Phân loại: chi tiết
    /// </summary>
    public static StaticModel Type(int id)
    {
      var query = from s in Type()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    /// <summary>
    /// Trạng thái giao việc: danh sách
    /// </summary>
    public static List<StaticModel> AssignStatus()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Chờ xác nhận",
        color = "has-text-dark",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Đã xác nhận",
        color = "has-text-success",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Đã từ chối",
        color = "has-text-danger",
      });

      return list;
    }

    /// <summary>
    /// Trạng thái giao việc: chi tiết
    /// </summary>
    public static StaticModel AssignStatus(int id)
    {
      var query = from s in AssignStatus()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel() { name = "Tất cả trạng thái" };
    }


    public static List<StaticModel> ViewList()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Danh sách",
        icon = "line_weight"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Bảng",
        icon = "dashboard"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Lịch",
        icon = "calendar_today"
      });

      return list;
    }

    public static StaticModel ViewList(int id)
    {
      var query = from s in AssignStatus()
                  where s.id == id
                  select s;
      return query.FirstOrDefault();
    }


    public static List<StaticModel> Cycles()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "ngày",

      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "tuần",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "tháng",
      });

      list.Add(new StaticModel
      {
        id = 4,
        name = "năm",
      });

      return list;
    }


    public static List<StaticModel> Weeks()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 2,
        name = "T2",

      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "T3",
      });

      list.Add(new StaticModel
      {
        id = 4,
        name = "T4",
      });

      list.Add(new StaticModel
      {
        id = 5,
        name = "T5",
      });

      list.Add(new StaticModel
      {
        id = 6,
        name = "T6",
      });

      list.Add(new StaticModel
      {
        id = 7,
        name = "T7",
      });


      list.Add(new StaticModel
      {
        id = 8,
        name = "CN",
      });
      return list;
    }
    #endregion
  }
}
