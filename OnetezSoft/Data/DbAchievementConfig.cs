using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbAchievementConfig
  {
    private static string _collection = "achievement_config";

    public static async Task<AchievementModel.Option> Create(string companyId, AchievementModel.Option model)
    {
      model.id = Guid.NewGuid().ToString();
      model.update = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel.Option>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<AchievementModel.Option> Update(string companyId, AchievementModel.Option model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel.Option>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<bool> Delete(string companyId, string id)
    {
      var result = await Get(companyId, id);

      if (result != null)
      {
        result.is_delete = true;

        var update = await Update(companyId, result);

        return update != null;
      }
      else
      {
        return false;
      }
    }

    public static async Task<AchievementModel.Option> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel.Option>(_collection);

      var result = await collection.FindAsync(x => !x.is_delete && x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    public static async Task<List<AchievementModel.Option>> GetByType(string companyId, string type)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel.Option>(_collection);

      var result = await collection.FindAsync(x => !x.is_delete && x.parent == type).Result.ToListAsync();

      return result;
    }

    public static async Task<List<AchievementModel.Option>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel.Option>(_collection);

      var result = await collection.FindAsync(x => !x.is_delete).Result.ToListAsync();

      return result;
    }

    public static async Task<List<AchievementModel.Option>> GetAll(string companyId, bool all)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel.Option>(_collection);

      var result = await collection.FindAsync(x => all).Result.ToListAsync();

      return result;
    }
  }
}
