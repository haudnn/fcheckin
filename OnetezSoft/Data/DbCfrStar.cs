using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbCfrStar
  {
    private static string _collection = "cfr_stars";

    public static async Task<CfrStarModel> Create(string companyId, CfrStarModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.create_date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<CfrStarModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<CfrStarModel> Update(string companyId, CfrStarModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrStarModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<CfrStarModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrStarModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrStarModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<List<CfrStarModel>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrStarModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderByDescending(x => x.create_date).ToList();
    }


    public static async Task<List<CfrStarModel>> GetList(string companyId, string user, bool isPlus, DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrStarModel>(_collection);

      var builder = Builders<CfrStarModel>.Filter;

      var filtered = builder.Gte("create_date", start.Ticks)
         & builder.Lt("create_date", end.AddDays(1).Ticks);

      if (!string.IsNullOrEmpty(user))
        filtered &= builder.Eq("user", user);
      if (isPlus)
        filtered &= builder.Gt("star", 0);
      else
        filtered = filtered & builder.Lt("star", 0);

      var sorted = Builders<CfrStarModel>.Sort.Descending("create_date");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.create_date descending select x).ToList();
    }
  }
}