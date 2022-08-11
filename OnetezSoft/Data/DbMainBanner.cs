using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbMainBanner
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "banners";

    public static async Task<BannerModel> Create(BannerModel model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<BannerModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<BannerModel> Update(BannerModel model)
    {
      var collection = _db.GetCollection<BannerModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<bool> Delete(string id)
    {
      var collection = _db.GetCollection<BannerModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    public static async Task<BannerModel> Get(string id)
    {
      var collection = _db.GetCollection<BannerModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<List<BannerModel>> GetList()
    {
      var collection = _db.GetCollection<BannerModel>(_collection);

      var sorted = Builders<BannerModel>.Sort.Descending("date");

      return await collection.Find(new BsonDocument()).Sort(sorted).ToListAsync();
    }
  }
}