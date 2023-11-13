using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbMainCategory
   {
      private static string _collection = "category";
      private static IMongoDatabase _db = Mongo.DbConnect("fastdo");

      public static async Task<CategoryModel> Create(CategoryModel model)
      {
         if (string.IsNullOrEmpty(model.id))
            model.id = Mongo.RandomId();

         var collection = _db.GetCollection<CategoryModel>(_collection);

         await collection.InsertOneAsync(model);

         return model;
      }


      public static async Task<CategoryModel> Update(CategoryModel model)
      {
         var collection = _db.GetCollection<CategoryModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }


      public static async Task<CategoryModel> Get(string id)
      {

         var collection = _db.GetCollection<CategoryModel>(_collection);

         var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

         return result;
      }


      public static async Task<bool> Delete(string id)
      {
         var collection = _db.GetCollection<CategoryModel>(_collection);

         var filter = Builders<CategoryModel>.Filter.Eq(x => x.id, id);

         var update = Builders<CategoryModel>.Update.Set(x => x.isDeleted, true);

         var result = await collection.FindOneAndUpdateAsync(filter, update);
         if (result != null)
            return true;
         else
            return false;
      }



      public static async Task<List<CategoryModel>> GetList()
      {
         var collection = _db.GetCollection<CategoryModel>(_collection);

         var results = await collection.FindAsync(x => true).Result.ToListAsync();

         return results;
      }
   }
}
