using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách công việc trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInPlan(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.FindAsync(x => x.plan_id == planId && x.parent_id == null).Result.ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInPlan(string companyId, string planId, string sectionId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.FindAsync(x => x.plan_id == planId && x.parent_id == null && x.section_id == sectionId).Result.ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc phụ trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInTask(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.FindAsync(x => x.plan_id == planId && x.parent_id != null).Result.ToListAsync();

      return results;
    }


    /// <summary>
    /// Danh sách công việc phụ trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListTaskByPlan(string companyId, string planId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var results = await collection.FindAsync(x => x.plan_id == planId).Result.ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc phụ trong kế hoạch
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListInTask(string companyId, string planId, string taskId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var builder = Builders<WorkPlanModel.Task>.Filter;

      var filtered = builder.Eq("plan_id", planId) & builder.Eq("parent_id", taskId);

      var sorted = Builders<WorkPlanModel.Task>.Sort.Ascending("date_start");

      var results = await collection.Find(filtered).Sort(sorted).ToListAsync();

      return results;
    }


    /// <summary>
    /// Danh sách công việc đang tham gia trong kế hoạch (bao gồm cả công việc chỉ tham gia CVP)
    /// </summary>
    public static async Task<List<WorkPlanModel.Task>> GetListJoin(string companyId, string planId, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Task>(_collection);

      var list = await collection.FindAsync(x => x.plan_id == planId && x.parent_id == null).Result.ToListAsync();
      var subTask = await collection.FindAsync(x => x.plan_id == planId && !string.IsNullOrEmpty(x.parent_id)).Result.ToListAsync();

      var results = new List<WorkPlanModel.Task>();
      foreach (var item in list)
      {
        if (item.members.Where(x => x.id == userId).Count() > 0)
          results.Add(item);
        else
        {
          if (subTask.Where(x => x.parent_id == item.id && x.members.Find(x => x.id == userId) != null).Count() > 0)
          {
            results.Add(item);
          }
        }
      }

      return results;
    }
  }
}