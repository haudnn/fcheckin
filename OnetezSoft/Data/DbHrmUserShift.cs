using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbHrmUserShift
{
   private static string _collection = "hrm_usershifts";

   public static async Task<HrmUserShiftModel> Create(string companyId, HrmUserShiftModel model)
   {
      var check = await Get(companyId, model.id);

      if (check == null)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmUserShiftModel>(_collection);

         await collection.InsertOneAsync(model);
      }
      else
      {
         await Update(companyId, model);
      }

      return model;
   }


   public static async Task<HrmUserShiftModel> Update(string companyId, HrmUserShiftModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmUserShiftModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
   }


   public static async Task<bool> Delete(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmUserShiftModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
         return true;
      else
         return false;
   }


   public static async Task<HrmUserShiftModel> Get(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmUserShiftModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
   }


   public static async Task<List<HrmUserShiftModel>> GetList(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmUserShiftModel>(_collection);

      return await collection.FindAsync(new BsonDocument()).Result.ToListAsync();
   }
}