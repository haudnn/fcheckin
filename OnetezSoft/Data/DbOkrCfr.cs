using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbOkrCfr
  {
    private static string _collection = "okr_cfrs";

    public static async Task<OkrCfrModel> Create(string companyId, OkrCfrModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date_create = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<OkrCfrModel> Update(string companyId, OkrCfrModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<OkrCfrModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    /// <summary>
    /// Danh sách Phản hồi - Tặng sao
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public static async Task<List<OkrCfrModel>> GetList(string companyId, DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var results = new List<OkrCfrModel>();

      if (start != null && end != null)
      {
        var builder = Builders<OkrCfrModel>.Filter;

        var filtered = builder.Gte("date_create", start.Value.Ticks)
           & builder.Lt("date_create", end.Value.AddDays(1).Ticks);

        results = await collection.FindAsync(filtered).Result.ToListAsync();
      }
      else
      {
        results = collection.FindAsync(new BsonDocument()).Result.ToList();
      }

      return results.OrderByDescending(x => x.date_create).ToList();
    }

    /// <summary>
    /// Danh sách Phản hồi - Tặng sao
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="type">2: ghi nhận; 3: tặng sao</param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public static async Task<List<OkrCfrModel>> GetList(string companyId, int type, DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var results = new List<OkrCfrModel>();

      if (start != null && end != null)
      {
        var builder = Builders<OkrCfrModel>.Filter;

        var filtered = builder.Eq("type", type)
          & builder.Gte("date_create", start.Value.Ticks)
          & builder.Lt("date_create", end.Value.AddDays(1).Ticks);

        results = await collection.FindAsync(filtered).Result.ToListAsync();
      }
      else
      {
        results = await collection.FindAsync(x => x.type == type).Result.ToListAsync();
      }

      return results.OrderByDescending(x => x.date_create).ToList();
    }

    /// <summary>
    /// Phản hồi - Tặng sao đã nhận
    /// </summary>
    public static async Task<List<OkrCfrModel>> GetListReceive(string companyId, string user, int type,
      DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var builder = Builders<OkrCfrModel>.Filter;

      var filtered = builder.Eq("user_receive", user);

      if (type != 0)
        filtered = filtered & builder.Eq("type", type);
      if (start != null)
        filtered = filtered & builder.Gte("date_create", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date_create", end.Value.AddDays(1).Ticks);

      var sorted = Builders<OkrCfrModel>.Sort.Descending("date_create");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date_create descending select x).ToList();
    }

    /// <summary>
    /// Phản hồi - Tặng sao đã tặng
    /// </summary>
    public static async Task<List<OkrCfrModel>> GetListGive(string companyId, string user, int type,
      DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var builder = Builders<OkrCfrModel>.Filter;

      var filtered = builder.Eq("user_create", user);

      if (type != 0)
        filtered = filtered & builder.Eq("type", type);
      if (start != null)
        filtered = filtered & builder.Gte("date_create", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date_create", end.Value.AddDays(1).Ticks);

      var sorted = Builders<OkrCfrModel>.Sort.Descending("date_create");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date_create descending select x).ToList();
    }


    /// <summary>
    /// Hàm lấy dữ liệu để tính Thành tựu
    /// </summary>
    public static async Task<List<OkrCfrModel>> DataAchievement(string companyId, string user, DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCfrModel>(_collection);

      var builder = Builders<OkrCfrModel>.Filter;

      var filtered = builder.Eq("user_receive", user)
        & builder.Eq("type", 2)
        & builder.Gte("date_create", start.Ticks)
        & builder.Lt("date_create", end.AddDays(1).Ticks);

      return await collection.FindAsync(filtered).Result.ToListAsync();
    }


    /// <summary>
    /// Tính thành tựu CFRs
    /// </summary>
    public static async Task Achievement(string companyId, string user, GlobalService globalService)
    {
      Handled.Shared.GetTimeSpan(2, out DateTime start, out DateTime end);

      var list = await DataAchievement(companyId, user, start, end);

      var achievement = await DbAchievement.GetOption(companyId, "cfrs", list.Count);
      if (achievement != null)
      {
        var model = new AchievementModel()
        {
          user = user,
          name = achievement.name,
          desc = achievement.des,
          star = achievement.star,
          type = "cfrs",
          type_id = achievement.id
        };
        await DbAchievement.Create(companyId, model, globalService);
      }
    }


    #region Dữ liệu cố định

    // Câu hỏi: danh sách
    public static List<StaticModel> Type()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 2,
        name = "Ghi nhận"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Tặng sao"
      });

      return list;
    }

    // Câu hỏi: chi tiết
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
