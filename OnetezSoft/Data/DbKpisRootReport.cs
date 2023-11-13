using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbKpisRootReport
  {
    private static string _collection = "kpis_root_report";

    public static async Task<KpisRootReportModel> Create(string companyId, KpisRootReportModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<KpisRootReportModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<KpisRootReportModel> Update(string companyId, KpisRootReportModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisRootReportModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    /// <summary>
    /// Lấy danh sách
    /// </summary>
    public static async Task<List<KpisRootReportModel>> GetList(string companyId, string cycle, string kpisRoot)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisRootReportModel>(_collection);

      var results = new List<KpisRootReportModel>();
      results = await collection.FindAsync(x => x.cycle == cycle && x.kpis_root == kpisRoot).Result.ToListAsync();

      return results.OrderBy(i => i.date).ToList();
    }

    public static async Task<KpisRootReportModel> GetByKpisIdAndDate(string companyId, string kpisRoot, long date)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisRootReportModel>(_collection);

      var result = await collection.FindAsync(x => x.kpis_root == kpisRoot && x.date == date).Result.FirstOrDefaultAsync();

      return result;
    }
  }
}
