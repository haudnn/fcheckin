using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;

namespace OnetezSoft.Data;

public class DbHrmWorkShift
{
  private static string _collection = "hrm_workshifts";

  public static async Task<HrmWorkShiftModel> Create(string companyId, HrmWorkShiftModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    if (string.IsNullOrEmpty(model.id))
      model.id = Mongo.RandomId();
    model.created = DateTime.Now.Ticks;

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }


  public static async Task<HrmWorkShiftModel> Update(string companyId, HrmWorkShiftModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var option = new ReplaceOptions { IsUpsert = false };

    var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

    return model;
  }


  public static async Task<bool> Delete(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var result = await collection.DeleteOneAsync(x => x.id == id);

    if (result.DeletedCount > 0)
      return true;
    else
      return false;
  }


  public static async Task<HrmWorkShiftModel> Get(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
  }


  public static async Task<List<HrmWorkShiftModel>> GetList(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var results = await collection.Find(new BsonDocument()).ToListAsync();

    return results.OrderBy(x => x.created).ToList();
  }
}
