using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OnetezSoft.Data
{
  public class DbKpis
  {
    private static string _collection = "kpis";

    public static async Task<KpisModel> Create(string companyId, KpisModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      model.date_create = DateTime.Now.Ticks;

      var collection = _db.GetCollection<KpisModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<KpisModel> Update(string companyId, KpisModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<bool> CreateOrUpdate(string companyId, KpisModel model)
    {
      var check = await Get(companyId, model.id);
      if(check == null)
      { 
        await Create(companyId, model);
        return true;
      }

      else
      { 
        await Update(companyId, model);
        return false;
      }
    }


    public static async Task<bool> CreateOrUpdateParent(string companyId, KpisModel model)
    {
      var check = await GetByParent(companyId, model.cycle);
      if (check == null)
      {
        await Create(companyId, model);
        return true;
      }

      else
      {
        await Update(companyId, model);
        return false;
      }
    }


    public static async Task<bool> Delete(string companyId, List<string> list)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<KpisModel>(_collection);
      var result = await collection.DeleteManyAsync(x => list.Contains(x.id));

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    public static async Task<KpisModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisModel>(_collection);

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    public static async Task<KpisModel> GetByParent(string companyId, string idCycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisModel>(_collection);

      var result = await collection.FindAsync(x => x.parent == idCycle).Result.FirstOrDefaultAsync();

      return result;
    }

    public static async Task<List<KpisModel>> GetList(string companyId, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisModel>(_collection);

      var results = new List<KpisModel>();
      results = await collection.FindAsync(i => i.cycle == cycle).Result.ToListAsync();

      return results;
    }

    public static async Task<List<KpisModel>> GetListByKpisRoot(string companyId, string cycle, string kpisRootId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisModel>(_collection);

      var results = new List<KpisModel>();
      results = await collection.FindAsync(i => i.cycle == cycle && i.root == kpisRootId).Result.ToListAsync();

      return results;
    }

    /// <summary>
    /// Lấy danh sách
    /// </summary>
    public static async Task<List<KpisModel>> GetListForCheckin(string companyId, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<KpisModel>(_collection);

      var results = new List<KpisModel>();
      results = await collection.FindAsync(i => i.cycle == cycle).Result.ToListAsync();

      return (from x in results orderby x.start_date descending, x.end_date descending select x).ToList(); ;
    }
  }
}
