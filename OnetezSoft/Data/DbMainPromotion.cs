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

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<List<PromotionModel>> GetList()
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      var results = await collection.Find(new BsonDocument()).ToListAsync();

      return (from x in results orderby x.type, x.condition descending select x).ToList();
    }


    /// <summary>
    /// Danh sách khuyến mãi
    /// </summary>
    /// <param name="type">1: thời gian | 2: người dùng</param>
    public static List<PromotionModel> GetList(int type)
    {
      var collection = _db.GetCollection<PromotionModel>(_collection);

      var results = collection.Find(x => x.type == type).ToList();

      return (from x in results orderby x.condition descending select x).ToList();
    }
  }
}
