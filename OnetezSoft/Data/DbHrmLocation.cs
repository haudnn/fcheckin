using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OnetezSoft.Data;

public class DbHrmLocation
{
   private static string _collection = "hrm_locations";

   public static async Task<HrmLocationModel> Create(string companyId, HrmLocationModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
         model.id = Mongo.RandomId();
      model.created = DateTime.Now.Ticks;

      var collection = _db.GetCollection<HrmLocationModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
   }


   public static async Task<HrmLocationModel> Update(string companyId, HrmLocationModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      model.created = DateTime.Now.Ticks;

      var collection = _db.GetCollection<HrmLocationModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
   }


   public static async Task<bool> Delete(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmLocationModel>(_collection);

      var filter = Builders<HrmLocationModel>.Filter.Eq(x => x.id, id);
      var update = Builders<HrmLocationModel>.Update.Set(x => x.is_deleted, true);

      var result = await collection.FindOneAndUpdateAsync(filter, update);
      if (result != null)
         return true;
      else
         return false;
   }

   /// <summary>+
   /// Lấy danh sách địa điểm chấm công
   /// </summary>
   /// <param name="isAll">Có lấy địa điểm đã xoá không? true: có | false: không</param>
   public static async Task<List<HrmLocationModel>> GetList(string companyId, bool isAll = false)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmLocationModel>(_collection);

      var results = new List<HrmLocationModel>();
      if (isAll)
         results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();
      else
         results = await collection.FindAsync(x => !x.is_deleted).Result.ToListAsync();

      return results.OrderByDescending(x => x.created).ToList();
   }

   /// <summary>
   /// Lấy danh sách địa điểm user áp dụng
   /// </summary>
   /// <param name="companyId"></param>
   /// <param name="userId">Id user</param>
   /// <returns></returns>
   public static async Task<List<HrmLocationModel>> GetListForUser(string companyId, string userId)
   {
      if (string.IsNullOrEmpty(userId))
         return new List<HrmLocationModel>();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmLocationModel>(_collection);

      var results = await collection.FindAsync(x => x.members_id.Contains(userId)).Result.ToListAsync();

      return results.OrderByDescending(x => x.created).ToList();
   }
}