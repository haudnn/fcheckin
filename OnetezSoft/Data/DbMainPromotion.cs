using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbMainPromotion
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "promotions";


    public static async Task<PromotionModel> Create(PromotionModel model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<PromotionModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<PromotionModel> Update(PromotionModel model)
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<bool> Delete(string id)
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    public static async Task<PromotionModel> Get(string id)
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<List<PromotionModel>> GetList()
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return (from x in results orderby x.type, x.condition descending select x).ToList();
    }


    /// <summary>
    /// Danh sách khuyến mãi
    /// </summary>
    /// <param name="type">1: thời gian | 2: người dùng</param>
    public static async Task<List<PromotionModel>> GetList(int type)
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      var results = await collection.FindAsync(x => x.type == type).Result.ToListAsync();

      return (from x in results orderby x.condition descending select x).ToList();
    }
  }
}
