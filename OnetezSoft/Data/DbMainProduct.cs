using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbMainProduct
   {
      private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
      private static string _collection = "products";


      public static async Task<ProductModel> Create(ProductModel model)
      {
         model.video = new();
         model.images = new();
         model.pricelist = new();
         model.features = new();
         model.modules = new();

         var collection = _db.GetCollection<ProductModel>(_collection);

         await collection.InsertOneAsync(model);

         return model;
      }

      public static async Task<ProductModel> Update(ProductModel model)
      {
         var collection = _db.GetCollection<ProductModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }

      public static async Task<ProductModel> Get(string id)
      {
         var collection = _db.GetCollection<ProductModel>(_collection);

         return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
      }

      public static ProductModel GetById(string id)
      {
         var collection = _db.GetCollection<ProductModel>(_collection);

         return collection.FindAsync(x => x.id == id).Result.FirstOrDefault();
      }

      /// <summary>
      /// Lấy tất cả sản phẩm đang có
      /// </summary>
      public static async Task<List<ProductModel>> GetAll()
      {
         var collection = _db.GetCollection<ProductModel>(_collection);

         return await collection.FindAsync(new BsonDocument()).Result.ToListAsync();
      }

      /// <summary>
      /// Lấy tất cả sản phẩm đang hiện
      /// </summary>
      public static async Task<List<ProductModel>> GetList()
      {
         var collection = _db.GetCollection<ProductModel>(_collection);

         return await collection.FindAsync(x => x.show).Result.ToListAsync();
      }
   }
}
