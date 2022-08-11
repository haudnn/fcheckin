using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbDayCheckin
  {
    private static string _collection = "days_checkin";

    public static async Task<DayCheckinModel> Create(string companyId, DayCheckinModel model)
    {
      model.id = Guid.NewGuid().ToString();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayCheckinModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<DayCheckinModel> Update(string companyId, DayCheckinModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayCheckinModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayCheckinModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<DayCheckinModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayCheckinModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<DayCheckinModel> GetbyOkr(string companyId, string okr)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayCheckinModel>(_collection);

      var result = await collection.Find(x => x.okr == okr && x.status == 0).FirstOrDefaultAsync();

      return result;
    }


    public static async Task<List<DayCheckinModel>> GetList(string companyId, string cycle, 
      DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayCheckinModel>(_collection);

      var builder = Builders<DayCheckinModel>.Filter;

      var filtered = builder.Eq("cycle", cycle);

      if (start != null)
        filtered = filtered & builder.Gte("date_checkin", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date_checkin", end.Value.AddDays(1).Ticks);

      var sorted = Builders<DayCheckinModel>.Sort.Descending("date_checkin");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }
  }
}
