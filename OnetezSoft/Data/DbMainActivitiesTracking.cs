using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbMainActivitiesTracking
  {
    // Db Server Beta
    private static MongoCredential credential = MongoCredential.CreateCredential("admin", "myUserAdmin", "=#Vqa6u##88%11N");
    private static MongoClientSettings settings = MongoClientSettings.FromConnectionString("mongodb://103.90.232.168:27017/");

    private static IMongoDatabase DbConnect(string name)
    {
      settings.Credential = credential;
      var mongoClient = new MongoClient(settings);
      return mongoClient.GetDatabase(name);
    }

    private static IMongoDatabase _db = DbConnect("fastdo");
    private static string _collection = "activities_tracking";
    private static string _systemCollection = "system_tracking";
    private static string _catchLogCollection = "catchlog_tracking";

    public static async Task<ActivitiesTrackingModel> Create(ActivitiesTrackingModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<ActivitiesTrackingModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<SystemTracking> Create(SystemTracking model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<SystemTracking>(_systemCollection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<CatchLogModel> Create(CatchLogModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<CatchLogModel>(_catchLogCollection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<List<CatchLogModel>> GetAll()
    {
      var collection = _db.GetCollection<CatchLogModel>(_catchLogCollection);

      var results = await collection.FindAsync(x => true).Result.ToListAsync();

      return results.OrderByDescending(x => x.create).ToList();
    }

    public static async Task<List<ActivitiesTrackingModel>> GetListActivities()
    {
      var collection = _db.GetCollection<ActivitiesTrackingModel>(_collection);

      var results = await collection.FindAsync(x => true).Result.ToListAsync();

      return results.OrderByDescending(x => x.update).ToList();
    }

    public static async Task<List<SystemTracking>> GetListSystem()
    {
      var collection = _db.GetCollection<SystemTracking>(_systemCollection);

      var results = await collection.FindAsync(x => true).Result.ToListAsync();

      return results.OrderByDescending(x => x.create).ToList();
    }

    /// <summary>
    /// Xóa tracking ngày quá khứ
    /// </summary>
    /// <returns></returns>
    public static async Task DeleteActivities()
    {
      var collection = _db.GetCollection<ActivitiesTrackingModel>(_collection);

      await collection.DeleteManyAsync(x => x.update < DateTime.Today.Ticks);
    }
  }
}
