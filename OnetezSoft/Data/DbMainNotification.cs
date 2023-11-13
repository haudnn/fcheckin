using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbMainNotification
   {
      private static string _collection = "notifications";
      private static IMongoDatabase _db = Mongo.DbConnect("fastdo");

      public static async Task<NotificationModel> Create(NotificationModel model)
      {
         if (string.IsNullOrEmpty(model.id))
            model.id = Mongo.RandomId();

         var collection = _db.GetCollection<NotificationModel>(_collection);

         await collection.InsertOneAsync(model);

         return model;
      }


      public static async Task<NotificationModel> Update(NotificationModel model)
      {
         var collection = _db.GetCollection<NotificationModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }


      public static async Task<bool> Delete(string id)
      {
         var collection = _db.GetCollection<NotificationModel>(_collection);

         var result = await collection.DeleteOneAsync(x => x.id == id);

         if (result.DeletedCount > 0)
            return true;
         else
            return false;
      }


      public static async Task<NotificationModel> Get(string id)
      {
         var collection = _db.GetCollection<NotificationModel>(_collection);

         var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

         return result;
      }

      public static async Task<List<NotificationModel>> GetList()
      {
         var collection = _db.GetCollection<NotificationModel>(_collection);

         var results = await collection.FindAsync(x => true).Result.ToListAsync();

         return results.OrderByDescending(x => x.created).ToList();
      }
   }
}
