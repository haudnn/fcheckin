using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbAchievement
  {
    private static string _collection = "achievement";

    public static async Task<AchievementModel> Create(string companyId, AchievementModel model, GlobalService globalService)
    {
      model.id = Guid.NewGuid().ToString();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      await collection.InsertOneAsync(model);

      // Công sao cho nhân viên khi đạt thành tựu
      var user = await DbUser.Get(companyId, model.user, globalService);
      if (user != null)
      {
        user.star_total += model.star;
        user.star_receive += model.star;
        await DbUser.Update(companyId, user, globalService);
      }

      return model;
    }

    public static async Task<AchievementModel> Update(string companyId, AchievementModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    /// <summary>
    /// Lấy danh sách thành tựu của 1 người
    /// </summary>
    public static async Task<List<AchievementModel>> GetList(string companyId, string user, string type,
      DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var builder = Builders<AchievementModel>.Filter;

      var filtered = builder.Gte("date", start.Ticks) & builder.Lt("date", end.AddDays(1).Ticks);

      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      if (!string.IsNullOrEmpty(type))
        filtered = filtered & builder.Eq("type", type);

      var results = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in results orderby x.date descending select x).ToList();
    }

    public static async Task<List<AchievementModel>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var results = await collection.FindAsync(x => true).Result.ToListAsync();

      return results;
    }

    /// <summary>
    /// Lấy thành tựu chưa xem của 1 người
    /// </summary>
    public static async Task<AchievementModel> GetNew(string companyId, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var list = await collection.FindAsync(x => x.user == user && x.view == false).Result.ToListAsync();

      list = (from x in list orderby x.date descending select x).ToList();

      if (list.Count > 0)
        return list[0];
      else
        return null;
    }

    #region Dữ liệu cấp sao

    public static async Task<List<AchievementModel.Option>> GetListByType(string companyId, string type)
    {
      var results = await DbAchievementConfig.GetByType(companyId, type);

      results = results.Where(x => x.is_active).ToList();

      return results;
    }

    public static async Task<AchievementModel.Option> GetOption(string companyId, string type, int count)
    {
      if (count == 0)
        return null;
      else
      {
        var list = await GetListByType(companyId, type);
        return list.FirstOrDefault(x => x.apply == count);
      }
    }
    #endregion
  }
}