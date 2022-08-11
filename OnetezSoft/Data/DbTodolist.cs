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
  public class DbTodolist
  {
    private static string _collection = "todolist";

    public static async Task<TodolistModel> Create(string companyId, TodolistModel model)
    {
      var date = string.Format("{0:yyMMdd}", new DateTime(model.date));
      model.id = date + model.user_create;

      model.todos = new();
      model.date_create = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<TodolistModel> Update(string companyId, TodolistModel model)
    {
      // Cập nhật điểm Todolist
      if (model.todos.Count > 0)
      {
        int point = 10;

        var day = string.Format("{0:yyyy-MM-dd}", new DateTime(model.date));
        var config = await DbMainCompany.Get(companyId);
        var checkin = Convert.ToDateTime(day + " " + config.todolist.time_checkin).AddDays(-1);
        var checkout = Convert.ToDateTime(day + " " + config.todolist.time_checkout);

        // Checkin đúng hạn
        if (model.status >= 2 && model.date_checkin <= checkin.Ticks)
        {
          model.ontime_checkin = true;
        }
        else // Checkin trễ hạn
        {
          model.ontime_checkin = false;
          point -= 2;
        }

        // Checkout trễ hạn
        if (model.status == 3 && model.date_checkout <= checkout.Ticks)
        {
          model.ontime_checkout = true;
        }
        else
        {
          model.ontime_checkout = false;
          point -= 2;
        }

        // Todo chưa hoàn thành
        point -= model.todos.Where(x => x.status < 4).Count();

        model.point = point > 0 ? point : 0;
      }
      else
      {
        model.point = 0;
        model.status = 0;
      }

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

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }


    public static async Task<TodolistModel> GetbyDay(string companyId, string user, DateTime date)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var builder = Builders<TodolistModel>.Filter;

      var filtered = builder.Eq("user_create", user)
         & builder.Eq("date", date.Ticks);

      var result = await collection.Find(filtered).FirstOrDefaultAsync();

      return result;
    }


    public static async Task<List<TodolistModel>> GetList(string companyId, string user, DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel>(_collection);

      var builder = Builders<TodolistModel>.Filter;

      var filtered = builder.Eq("user_create", user)
        & builder.Gt("status", 1);

      if (start != null)
        filtered = filtered & builder.Gte("date", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date", end.Value.AddDays(1).Ticks);

      var sorted = Builders<TodolistModel>.Sort.Descending("date");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
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

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
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

      var database = await collection.Find(filtered).ToListAsync();
      var daysOff = DbDayOff.GetAll(companyId);

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
    public static async Task Achievement(string companyId, string user)
    {
      Handled.Shared.GetTimeSpan(2, out DateTime start, out DateTime end);

      var day = await DataAchievement(companyId, user, start, end);

      var achievement = DbAchievement.Todolist(day);
      if (achievement != null)
      {
        var model = new AchievementModel()
        {
          user = user,
          name = achievement.name,
          desc = achievement.color,
          star = Convert.ToInt32(achievement.icon),
          type = "todolist"
        };
        await DbAchievement.Create(companyId, model);
      }
    }


    #region Dữ liệu cố định

    // Trạng thái: danh sách
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Todo",
        color = "",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Pending",
        color = "is-warning",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Doing",
        color = "is-link",
      });

      list.Add(new StaticModel
      {
        id = 4,
        name = "Done",
        color = "is-success",
      });

      list.Add(new StaticModel
      {
        id = 5,
        name = "Cancel",
        color = "is-dark",
      });

      return list;
    }

    // Trạng thái: chi tiết
    public static StaticModel Status(int id)
    {
      var query = from s in Status()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }


    // Độ ưu tiên: danh sách
    public static List<StaticModel> Level()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Bình thường",
        color = "has-text-success",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Quan trọng",
        color = "has-text-warning",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Rất quan trọng",
        color = "has-text-danger",
      });

      return list;
    }

    // Độ ưu tiên: chi tiết
    public static StaticModel Level(int id)
    {
      var query = from s in Level()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }


    // Phân loại: danh sách
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

    // Phân loại: chi tiết
    public static StaticModel Type(int id)
    {
      var query = from s in Type()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    #endregion
  }
}
