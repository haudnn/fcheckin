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
  public class DbWorkTask
  {
    private static string _collection = "work_tasks";

    public static async Task<WorkPlanModel.Task> Create(string companyId, WorkPlanModel.Task model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<WorkPlanModel.Task> Update(string companyId, WorkPlanModel.Task model)
    {
      model.members = model.members.OrderBy(x => x.role).ToList();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<WorkPlanModel.Task> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách công việc trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInPlan(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.Find(x => x.plan_id == planId && x.parent_id == null).ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInPlan(string companyId, string planId, string sectionId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.Find(x => x.plan_id == planId && x.parent_id == null && x.section_id == sectionId).ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc phụ trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInTask(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.Find(x => x.plan_id == planId && x.parent_id != null).ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc phụ trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInTask(string companyId, string planId, string taskId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.Find(x => x.plan_id == planId && x.parent_id == taskId).ToListAsync();

      return results;
    }
  }
}