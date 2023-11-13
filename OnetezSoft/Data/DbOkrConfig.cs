using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Threading.Tasks;

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

      var result = await collection.FindAsync(new BsonDocument()).Result.FirstOrDefaultAsync();

      if (result != null)
      {
        result.cycles ??= new();
        result.suggests ??= new();

        return result;
      }
      else
        return await Create(companyId);
    }
  }
}