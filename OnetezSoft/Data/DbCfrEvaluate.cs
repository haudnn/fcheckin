using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbCfrEvaluate
  {
    private static string _collection = "cfr_evaluates";

    public static async Task<CfrEvaluateModel> Create(string companyId, CfrEvaluateModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrEvaluateModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<CfrEvaluateModel> Update(string companyId, CfrEvaluateModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrEvaluateModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrEvaluateModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<CfrEvaluateModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrEvaluateModel>(_collection);

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }


    public static async Task<List<CfrEvaluateModel>> GetList(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CfrEvaluateModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.ToList();
    }


  }
}