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
  public class DbConfig
  {
    #region Data Configs

    private static IMongoDatabase _db;
    private static string _collection = "configs";

    public static async Task<ConfigModel> Create()
    {
      _db = DbMongo.DbConnect("company_ID");

      var current = new ConfigModel()
      {
        id = ConfigMongo.RandomId(),
        type_room = new(),
        type_rental = new(),
        utilities_house = new(),
        utilities_room = new()
      };

      return await Insert(current);
    }


    public static async Task<ConfigModel> Insert(ConfigModel model)
    {
      var collection = _db.GetCollection<ConfigModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<ConfigModel> Update(ConfigModel model)
    {
      if (model.kpi != null)
        model.kpi = model.kpi.OrderByDescending(x => x.revenue).ToList();

      var collection = _db.GetCollection<ConfigModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id == model.id, model, option);

      return model;
    }


    public static async Task<ConfigModel> Get()
    {
      var collection = _db.GetCollection<ConfigModel>(_collection);

      var result = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();

      if (result != null)
        return result;
      else
        return await Create();
    }


    #endregion


    #region Dữ liệu cố định


    public static List<StaticModel> FilterPrice()
    {
      var results = new List<StaticModel>();

      for (double i = 1; i <= 10; i += 0.5)
      {
        results.Add(new StaticModel
        {
          id = Convert.ToInt32(i * 1000000),
          name = i + " triệu"
        });
      }

      return results;
    }

    #endregion
  }
}
