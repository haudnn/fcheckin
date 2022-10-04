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
  public class DbWorkComment
  {
    private static string _collection = "work_comments";

    public static async Task<WorkPlanModel.Comment> Create(string companyId, WorkPlanModel.Comment model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Comment>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<WorkPlanModel.Comment> Update(string companyId, WorkPlanModel.Comment model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Comment>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Comment>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<WorkPlanModel.Comment> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Comment>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách bình luận trong công việc
    /// </summary>
    public static async Task<List<WorkPlanModel.Comment>> GetList(string companyId, string planId, string taskId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Comment>(_collection);

      var results = await collection.Find(x => x.plan_id == planId && x.task_id == taskId).ToListAsync();

      return results.OrderBy(x => x.date).ToList();
    }
  }
}