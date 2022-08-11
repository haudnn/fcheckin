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
  public class DbQuotes
  {
    private static string _collection = "quotes";

    public static async Task<QuotesModel> Create(string companyId, QuotesModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<QuotesModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<QuotesModel> Update(string companyId, QuotesModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<QuotesModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<QuotesModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<QuotesModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<QuotesModel>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }


    public static async Task<List<QuotesModel>> GetList(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<QuotesModel>(_collection);

      var sorted = Builders<QuotesModel>.Sort.Descending("date");

      return await collection.Find(new BsonDocument()).Sort(sorted).ToListAsync();
    }


    public static QuotesModel GetRandom(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<QuotesModel>(_collection);

      var list = collection.Find(new BsonDocument()).ToList();

      list = list.OrderBy(emp => Guid.NewGuid()).ToList();
      if (list.Count > 0)
        return list[0];
      else
      {
        return new QuotesModel()
        {
          name = "Hoàn thành thì tốt hơn hoàn hảo",
          author = "Sưu tầm"
        };
      }
    }
  }
}
