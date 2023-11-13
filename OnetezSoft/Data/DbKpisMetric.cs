using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbKpisMetric
  {
    private static string _collection = "kpis_metric";

    public static async Task<KpisMetricModel> Create(string companyId, KpisMetricModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      model.date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<KpisMetricModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<KpisMetricModel> Update(string companyId, KpisMetricModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      model.date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<KpisMetricModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<KpisMetricModel>(_collection);

      var filter = Builders<KpisMetricModel>.Filter.Eq(x => x.id, id);
      var update = Builders<KpisMetricModel>.Update.Set(x => x.is_deleted, true);

      var result = await collection.FindOneAndUpdateAsync(filter, update);
      if (result != null)
        return true;
      else
        return false;
    }

    /// <summary>
    /// Lấy danh sách
    /// </summary>
    /// <param name="isAll">Có lấy dữ liệu đã xoá không? true: có | false: không</param>
    public static async Task<List<KpisMetricModel>> GetList(string companyId, bool isAll = false)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisMetricModel>(_collection);

      var results = new List<KpisMetricModel>();
      if (isAll)
        results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();
      else
        results = await collection.FindAsync(x => !x.is_deleted).Result.ToListAsync();

      return results.OrderByDescending(i => i.date).ToList();
    }
  }
}
