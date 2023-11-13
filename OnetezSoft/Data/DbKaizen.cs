using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbKaizen
  {
    private static string _collection = "kaizen";

    public static async Task<KaizenModel> Create(string companyId, KaizenModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date_create = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<KaizenModel> Update(string companyId, KaizenModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<KaizenModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }


    public static async Task<KaizenModel> GetById(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<List<KaizenModel>> GetList(string companyId, string type, string status, DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      var builder = Builders<KaizenModel>.Filter;

      var filtered = builder.Gt("date_create", 0);

      if (!string.IsNullOrEmpty(type))
        filtered = filtered & builder.Eq("type", type);
      if (!string.IsNullOrEmpty(status))
        filtered = filtered & builder.Eq("status", Convert.ToInt32(status));
      if (start != null)
        filtered = filtered & builder.Gte("date_create", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date_create", end.Value.AddDays(1).Ticks);

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date_create descending select x).ToList();
    }

    /// <summary>
    /// Hàm lấy dữ liệu để tính Thành tựu
    /// </summary>
    public static async Task<List<KaizenModel>> DataAchievement(string companyId, string user, DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KaizenModel>(_collection);

      var builder = Builders<KaizenModel>.Filter;

      var filtered = builder.Eq("user_create", user)
        & builder.Gt("status", 0)
        & builder.Gte("date_create", start.Ticks)
        & builder.Lt("date_create", end.AddDays(1).Ticks);

      return await collection.FindAsync(filtered).Result.ToListAsync();
    }

    /// <summary>
    /// Tính thành tựu Kaizen
    /// </summary>
    public static async Task Achievement(string companyId, string user, GlobalService globalService)
    {
      Handled.Shared.GetTimeSpan(2, out DateTime start, out DateTime end);

      var list = await DataAchievement(companyId, user, start, end);

      var achievement = await DbAchievement.GetOption(companyId, "kaizen", list.Count);
      if (achievement != null)
      {
        var model = new AchievementModel()
        {
          user = user,
          name = achievement.name,
          desc = achievement.des,
          star = achievement.star,
          type = "kaizen",
          type_id = achievement.id
        };
        await DbAchievement.Create(companyId, model, globalService);
      }
    }

    /// <summary>
    /// Thêm bình luận vào Kaizen
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="kaizenId"></param>
    /// <param name="userId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task<KaizenModel> Comment(string companyId, string kaizenId, string userId, string content)
    {
      var kaizen = await Get(companyId, kaizenId);
      if (kaizen != null)
      {
        kaizen.comments.Add(new()
        {
          id = Mongo.RandomId(),
          user = userId,
          date = DateTime.Now.Ticks,
          content = content.Replace("[img]", "<p><img src=\"").Replace("[/img]", "\" alt=\"IMG\" /></p>")
        });
        return await Update(companyId, kaizen);
      }
      else
        return null;
    }


    #region Dữ liệu cố định

    // Trạng thái: danh sách
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 5,
        name = "Sẽ áp dụng",
        color = "has-text-success",
        icon = "thumb_up"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Sẽ xem xét",
        color = "has-text-warning",
        icon = "sms"
      });

      list.Add(new StaticModel
      {
        id = 1,
        name = "Đã ghi nhận",
        color = "has-text-link",
        icon = "task_alt"
      });

      list.Add(new StaticModel
      {
        id = 0,
        name = "Chưa phù hợp",
        color = "has-text-grey",
        icon = "tour"
      });

      return list;
    }

    // Trạng thái: chi tiết
    public static StaticModel Status(int id)
    {
      if (id == -1)
        return new StaticModel()
        {
          id = id,
          name = "Chưa đánh giá",
          color = "has-text-danger",
          icon = "help_outline"
        };

      var query = from s in Status()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      else
        return new StaticModel();
    }

    #endregion
  }
}
