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
  public class DbOkrConfig
  {
    private static string _collection = "okr_config";

    public static async Task<OkrConfigModel> Create(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var model = new OkrConfigModel()
      {
        id = companyId,
        cycles = new(),
        suggests = new(),
        evaluates = new()
      };

      var collection = _db.GetCollection<OkrConfigModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<OkrConfigModel> Update(string companyId, OkrConfigModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrConfigModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<OkrConfigModel> Get(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrConfigModel>(_collection);

      var result = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();

      if (result != null)
      {
        if (result.cycles == null)
          result.cycles = new();
        if (result.suggests == null)
          result.suggests = new();
        if (result.evaluates == null)
          result.evaluates = new();

        return result;
      }
      else
        return await Create(companyId);
    }
  }
}
