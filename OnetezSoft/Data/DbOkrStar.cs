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
  public class DbOkrStar
  {
    private static string _collection = "okr_stars";

    public static async Task<OkrStarModel> Create(string companyId, OkrStarModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.create_date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<OkrStarModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<OkrStarModel> Update(string companyId, OkrStarModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrStarModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<OkrStarModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrStarModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrStarModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static List<OkrStarModel> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrStarModel>(_collection);

      var results = collection.Find(new BsonDocument()).ToList();

      return results.OrderByDescending(x => x.create_date).ToList();
    }


    public static async Task<List<OkrStarModel>> GetList(string companyId, string user, bool isPlus, DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrStarModel>(_collection);

      var builder = Builders<OkrStarModel>.Filter;

      var filtered = builder.Gte("create_date", start.Ticks)
         & builder.Lt("create_date", end.AddDays(1).Ticks);

      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      if (isPlus)
        filtered = filtered & builder.Gt("star", 0);
      else
        filtered = filtered & builder.Lt("star", 0);

      var sorted = Builders<OkrStarModel>.Sort.Descending("create_date");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }
  }
}