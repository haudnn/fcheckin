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
  public class DbWorkPlan
  {
    private static string _collection = "work_plans";

    public static async Task<WorkPlanModel> Create(string companyId, WorkPlanModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<WorkPlanModel> Update(string companyId, WorkPlanModel model)
    {
      model.members = model.members.OrderBy(x => x.role).ToList();
      model.sections = model.sections.OrderByDescending(x => x.pos).ToList();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<WorkPlanModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }


    /// <summary>
    /// Danh sách kế hoạch có thể xem
    /// Là thành viên hoặc kế hoạch công khai
    /// </summary>
    public static async Task<List<WorkPlanModel>> GetListView(string companyId, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var list = await collection.Find(new BsonDocument()).ToListAsync();

      var results = new List<WorkPlanModel>();
      foreach (var item in list)
      {
        if(!item.is_private)
          results.Add(item);
        else if(item.members.Where(x => x.id == userId).Count() > 0)
          results.Add(item);
      }
      return results;
    }


    /// <summary>
    /// Danh sách kế hoạch đang tham gia
    /// </summary>
    public static async Task<List<WorkPlanModel>> GetListJoin(string companyId, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var list = await collection.Find(new BsonDocument()).ToListAsync();

      var results = new List<WorkPlanModel>();
      foreach (var item in list)
      {
        if(item.members.Where(x => x.id == userId).Count() > 0)
          results.Add(item);
      }
      return results;
    }


    /// <summary>
    /// Tất cả kế hoạch đang có
    /// </summary>
    public static async Task<List<WorkPlanModel>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      return await collection.Find(new BsonDocument()).ToListAsync();
    }
  }
}