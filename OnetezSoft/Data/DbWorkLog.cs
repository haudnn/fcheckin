using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách lịch sử cập nhật
    /// </summary>
    public static async Task<List<WorkLogModel>> GetListPlan(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var results = await collection.FindAsync(x => x.plan == planId && string.IsNullOrEmpty(x.task)).Result.ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }

    /// <summary>
    /// Danh sách lịch sử cập nhật
    /// </summary>
    public static async Task<List<WorkLogModel>> GetAllListPlan(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var results = await collection.FindAsync(x => x.plan == planId).Result.ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }

    /// <summary>
    /// Danh sách lịch sử cập nhật
    /// </summary>
    public static async Task<List<WorkLogModel>> GetListTask(string companyId, string planId, string taskId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkLogModel>(_collection);

      var results = await collection.FindAsync(x => x.plan == planId && x.task == taskId).Result.ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }
  }
}