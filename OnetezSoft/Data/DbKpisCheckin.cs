using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace OnetezSoft.Data
{
  public class DbKpisCheckin
  {
    private static string _collection = "kpis_checkin";

    public static async Task<KpisCheckinModel> Create(string companyId, KpisCheckinModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      model.date_create = DateTime.Now.Ticks;

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<KpisCheckinModel> Update(string companyId, KpisCheckinModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      model.date_create = DateTime.Now.Ticks;

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

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
    public static async Task<List<KpisCheckinModel>> GetList(string companyId, string cycle, string kpis)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      var results = new List<KpisCheckinModel>();
      results = await collection.FindAsync(x => x.cycle == cycle && x.kpis == kpis).Result.ToListAsync();

      return results.OrderByDescending(i => i.date_create).ToList();
    }

    /// <summary>
    /// Lấy danh sách checkin cần xác nhận
    /// </summary>
    public static async Task<List<KpisCheckinModel>> GetListUpdate(string companyId, string cycle, List<string> kpisList)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      var results = new List<KpisCheckinModel>();
      results = await collection.FindAsync(x => x.cycle == cycle && kpisList.Contains(x.kpis)
                                                && x.status_checkin == 1).Result.ToListAsync();

      return results.OrderByDescending(i => i.date_create).ToList();
    }

    /// <summary>
    /// Lấy danh sách
    /// </summary>
    public static async Task<List<KpisCheckinModel>> GetListHistory(string companyId, string cycle, List<string> kpisList)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      var results = new List<KpisCheckinModel>();
      results = await collection.FindAsync(x => x.cycle == cycle && kpisList.Contains(x.kpis)
                                                && x.status_checkin == 2 && x.date_confirm > 0).Result.ToListAsync();

      return results.OrderByDescending(i => i.date_confirm).ToList();
    }

    public static async Task<List<KpisCheckinModel>> GetListCheckinConfirm(string companyId, string cycle, string kpisRoot)
    {
      // sau này thêm vào:  && x.kpis_root == kpisRoot
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      var results = new List<KpisCheckinModel>();
      results = await collection.FindAsync(x => x.cycle == cycle && x.status_checkin == 2 
                                                && !string.IsNullOrEmpty(x.user_confirm)).Result.ToListAsync();

      return results;
    }

    public static async Task<KpisCheckinModel> GetByKpisIdAndDate(string companyId, string kpisId, long date)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      var result = await collection.FindAsync(x => x.kpis == kpisId && x.date_confirm == date).Result.FirstOrDefaultAsync();

      return result;
    }

    public static async Task<List<KpisCheckinModel>> GetByKpisRoot(string companyId, string cycleId, string kpisRoot)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisCheckinModel>(_collection);

      var results = new List<KpisCheckinModel>();
      results = await collection.FindAsync(x => x.cycle == cycleId && x.kpis_root == kpisRoot).Result.ToListAsync();

      return results;
    }
  }
}
