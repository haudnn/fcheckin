using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbHrmOption
{
   private static string _collection = "hrm_options";

   public static async Task<HrmOptionModel> Create(string companyId, HrmOptionModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
         model.id = Mongo.RandomId();

      var collection = _db.GetCollection<HrmOptionModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
   }


   public static async Task<HrmOptionModel> Update(string companyId, HrmOptionModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmOptionModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
   }


   public static async Task<bool> Delete(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmOptionModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
         return true;
      else
         return false;
   }


   public static async Task<HrmOptionModel> Get(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmOptionModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
   }


   public static async Task<List<HrmOptionModel>> GetAll(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmOptionModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderBy(x => x.pos).ToList();
   }


   public static async Task<List<HrmOptionModel>> GetList(string companyId, string type)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmOptionModel>(_collection);

      var results = await collection.FindAsync(x => x.type == type).Result.ToListAsync();

      return results.OrderBy(x => x.pos).ToList();
   }
}
