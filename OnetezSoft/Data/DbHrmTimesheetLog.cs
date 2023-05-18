using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;

namespace OnetezSoft.Data;

public class DbHrmTimesheetLog
{
  private static string _collection = "hrm_timesheet_logs";

  public static async Task<HrmTimesheetLogModel> Create(string companyId, HrmTimesheetLogModel model)
  {
    model.id = Mongo.RandomId();

    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetLogModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }


  public static async Task<List<HrmTimesheetLogModel>> GetList(string companyId, string timesheetId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimesheetLogModel>(_collection);

    var results = await collection.Find(x => x.timesheet == timesheetId).ToListAsync();

    return (from x in results orderby x.day, x.is_morning descending, x.edit_date select x).ToList();
  }
}