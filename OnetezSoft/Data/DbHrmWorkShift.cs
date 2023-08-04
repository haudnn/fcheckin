using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;

namespace OnetezSoft.Data;

public class DbHrmWorkShift
{
  private static string _collection = "hrm_workshifts";

  public static async Task<HrmWorkShiftModel> Create(string companyId, HrmWorkShiftModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    if (string.IsNullOrEmpty(model.id))
      model.id = Mongo.RandomId();
    model.created = DateTime.Now.Ticks;

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }


  public static async Task<HrmWorkShiftModel> Update(string companyId, HrmWorkShiftModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    model.created = DateTime.Now.Ticks;

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var option = new ReplaceOptions { IsUpsert = false };

    var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

    return model;
  }


  public static async Task<bool> Delete(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
				var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);
				await DbHrmTimeList.DeleteShift(companyId, id);
				await DbHrmTimeListRegister.DeleteShift(companyId, id);
				
				var filter = Builders<HrmWorkShiftModel>.Filter.Eq(x => x.id, id);
				var update = Builders<HrmWorkShiftModel>.Update.Set(x => x.is_deleted, true)
                                                       .Set(x => x.time_delete, DateTime.Now.Date.Ticks);

			var result = await collection.FindOneAndUpdateAsync(filter, update);
			if (result != null)
				return true;
			else
				return false;
  }


  public static async Task<HrmWorkShiftModel> Get(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
  }


  public static async Task<List<HrmWorkShiftModel>> GetList(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var results = await collection.Find(x => !x.is_deleted).ToListAsync();

    var sortedResults = results.OrderBy(x => TimeSpan.Parse(x.checkin)).ToList();

    return sortedResults;
  }

  public static async Task<List<HrmWorkShiftModel>> GetWorkList(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var results = await collection.Find(x => !x.is_deleted).ToListAsync();

    var sortedResults = results.OrderByDescending(x => x.created).ToList();

    return sortedResults;
  }

  public static async Task<List<HrmWorkShiftModel>> GetListWithoutDelete(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

    var results = await collection.Find(new BsonDocument()).ToListAsync();

    return results.OrderByDescending(x => x.created).ToList();
  }
}