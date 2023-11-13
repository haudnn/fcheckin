using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
   }


   public static async Task<List<HrmWorkShiftModel>> GetList(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

      var results = await collection.FindAsync(x => !x.is_deleted).Result.ToListAsync();

      var sortedResults = results.OrderBy(x => TimeSpan.Parse(x.checkin)).ToList();

      return sortedResults;
   }

   public static async Task<List<HrmWorkShiftModel>> GetWorkList(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

      var results = await collection.FindAsync(x => !x.is_deleted).Result.ToListAsync();

      var sortedResults = results.OrderByDescending(x => x.created).ToList();

      return sortedResults;
   }

   public static async Task<List<HrmWorkShiftModel>> GetListWithoutDelete(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmWorkShiftModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderByDescending(x => x.created).ToList();
   }


   #region Dữ liệu cố định
   /// <summary>
   /// Màu sắc của ca làm
   /// </summary>

   public static List<StaticModel> ColorList()
   {
      var list = new List<StaticModel>();

      list.Add(new() { id = 1, color = "#484848" });
      list.Add(new() { id = 2, color = "#6B8FE0" });
      list.Add(new() { id = 3, color = "#7047EB" });
      list.Add(new() { id = 4, color = "#F3B677" });
      list.Add(new() { id = 5, color = "#11734b" });
      return list;
   }
   #endregion
}