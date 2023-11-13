using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
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
        if (item.members.Where(x => x.id == userId).Count() > 0)
          results.Add(item);
      }

      return results.OrderBy(x => x.date_end).ToList();
    }


    /// <summary>
    /// Danh sách kế hoạch đang tham gia, theo trạng thái
    /// </summary>
    public static async Task<List<WorkPlanModel>> GetListJoin(string companyId, string userId, int status)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var list = await collection.FindAsync(x => x.status == status).Result.ToListAsync();

      var results = new List<WorkPlanModel>();
      foreach (var item in list)
      {
        if (item.members.Where(x => x.id == userId).Count() > 0)
          results.Add(item);
      }

      return results.OrderBy(x => x.date_end).ToList();
    }


    /// <summary>
    /// Tất cả kế hoạch đang có
    /// </summary>
    public static async Task<List<WorkPlanModel>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderBy(x => x.date_end).ToList();
    }


    public static async Task UpdateSection(string companyId, string modelId, WorkPlanModel.Section item)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var filter = Builders<WorkPlanModel>.Filter.Eq(x => x.id, modelId);

      var update = Builders<WorkPlanModel>.Update.Push(x => x.sections, item);

      await collection.UpdateOneAsync(filter, update);

      return;
    }

    public static async Task AddSheet(string companyId, string modelId, WorkPlanModel.Sheet item)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var filter = Builders<WorkPlanModel>.Filter.Eq(x => x.id, modelId);

      var update = Builders<WorkPlanModel>.Update.Push(x => x.sheets, item);

      await collection.UpdateOneAsync(filter, update);

      return;
    }

    public static async Task UpdateSheet(string companyId, string modelId, WorkPlanModel.Sheet item)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<WorkPlanModel>(_collection);

      var builderFilter = Builders<WorkPlanModel>.Filter;
      var builder = Builders<WorkPlanModel>.Update;

      var filter = builderFilter.Eq(x => x.id, modelId) & builderFilter.Where(x => x.sheets.Any(y => y.id == item.id));

      var update = builder.Set("sheets.$", item);

      await collection.UpdateOneAsync(filter, update);

      return;
    }
  }
}