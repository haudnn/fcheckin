using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbWorkLog
  {
    private static string _collection = "work_logs";

    public static async Task<WorkLogModel> Create(string companyId, WorkLogModel model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<WorkLogModel> Update(string companyId, WorkLogModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<WorkLogModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách lịch sử cập nhật
    /// </summary>
    public static async Task<List<WorkLogModel>> GetListPlan(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var results = await collection.Find(x => x.plan == planId && string.IsNullOrEmpty(x.task)).ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }

    /// <summary>
    /// Danh sách lịch sử cập nhật
    /// </summary>
    public static async Task<List<WorkLogModel>> GetListTask(string companyId, string planId, string taskId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var results = await collection.Find(x => x.plan == planId && x.task == taskId).ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }
  }
}