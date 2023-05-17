using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;

namespace OnetezSoft.Data;

public class DbHrmTimekeeping
{
  private static string _collection = "hrm_timekeepings";

  public static async Task<HrmTimekeepingModel> Create(string companyId, HrmTimekeepingModel model)
  {
    model.id = DateTime.Today.ToString("yyMMdd") + model.user;
    model.date = DateTime.Today.Ticks;

    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }


  public static async Task<HrmTimekeepingModel> Update(string companyId, HrmTimekeepingModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

    var option = new ReplaceOptions { IsUpsert = false };

    var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

    return model;
  }


  public static async Task<bool> Delete(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

    var result = await collection.DeleteOneAsync(x => x.id == id);

    if (result.DeletedCount > 0)
      return true;
    else
      return false;
  }


  public static async Task<HrmTimekeepingModel> Get(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

    return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
  }


  public static async Task<HrmTimekeepingModel> Get(string companyId, string userId, long date)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

    return await collection.Find(x => x.user == userId && x.date == date).FirstOrDefaultAsync();
  }


  public static async Task<List<HrmTimekeepingModel>> GetList(string companyId, string userId, long dateS, long dateE)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

    return await collection.Find(x => x.user == userId
      && x.date >= dateS && x.date <= dateE
      ).ToListAsync();
  }
}
