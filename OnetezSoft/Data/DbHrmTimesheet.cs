using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;

namespace OnetezSoft.Data;

public class DbHrmTimesheet
{
  private static string _collection = "hrm_timesheets";

  public static async Task<HrmTimesheetModel> Create(string companyId, HrmTimesheetModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }


  public static async Task<HrmTimesheetModel> Update(string companyId, HrmTimesheetModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

    var option = new ReplaceOptions { IsUpsert = false };

    var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

    return model;
  }


  public static async Task<bool> Delete(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

    var result = await collection.DeleteOneAsync(x => x.id == id);

    if (result.DeletedCount > 0)
      return true;
    else
      return false;
  }


  public static async Task<HrmTimesheetModel> Get(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

    return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
  }


  public static async Task<List<HrmTimesheetModel>> GetList(string companyId, long month)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

    return await collection.Find(x => x.month == month).ToListAsync();
  }
}
