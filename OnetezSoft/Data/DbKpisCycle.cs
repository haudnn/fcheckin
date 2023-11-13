using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OnetezSoft.Data
{
  public class DbKpisCycle
  {
    private static string _collection = "kpis_cycle";

    public static async Task<KpisCycleModel> Create(string companyId, KpisCycleModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<KpisCycleModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<KpisCycleModel> Update(string companyId, KpisCycleModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCycleModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<KpisCycleModel>(_collection);
      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    /// <summary>
    /// Lấy danh sách
    /// </summary>
    public static async Task<List<KpisCycleModel>> GetList(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCycleModel>(_collection);

      var results = new List<KpisCycleModel>();
      results = await collection.FindAsync(x => true).Result.ToListAsync();

      return (from x in results orderby x.start_date, x.end_date select x).ToList();
    }
  }
}
