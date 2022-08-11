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
  public class DbGiftBanner
  {
    private static string _collection = "gift_banners";

    public static async Task<GiftBannerModel> Create(string companyId, GiftBannerModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<GiftBannerModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<GiftBannerModel> Update(string companyId, GiftBannerModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<GiftBannerModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<GiftBannerModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<GiftBannerModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<GiftBannerModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<List<GiftBannerModel>> GetList(string companyId, string keyword, string department)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<GiftBannerModel>(_collection);

      var sorted = Builders<GiftBannerModel>.Sort.Descending("pin");

      var list = await collection.Find(new BsonDocument()).Sort(sorted).ToListAsync();

      var results = new List<GiftBannerModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          bool check = Handled.Shared.SearchKeyword(keyword, item.name + item.products);

          if (check)
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }
  }
}
