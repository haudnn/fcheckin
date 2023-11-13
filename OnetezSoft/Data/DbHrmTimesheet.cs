using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbHrmTimesheet
{
   private static string _collection = "hrm_timesheets";

   public static async Task<HrmTimesheetModel> Create(string companyId, HrmTimesheetModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
         model.id = Mongo.RandomId();

      var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
   }


   public static async Task<HrmTimesheetModel> Update(string companyId, HrmTimesheetModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
   }


   public static async Task<bool> Delete(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
         return true;
      else
         return false;
   }

   public static async Task<HrmTimesheetModel> Delete(string companyId, HrmTimesheetModel model)
   {
      model.is_delete = true;
      await Update(companyId, model);
      return model;
   }


   public static async Task<HrmTimesheetModel> Get(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
   }


   public static async Task<List<HrmTimesheetModel>> GetList(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimesheetModel>(_collection);

      return await collection.FindAsync(x => !x.is_delete).Result.ToListAsync();
   }
}
