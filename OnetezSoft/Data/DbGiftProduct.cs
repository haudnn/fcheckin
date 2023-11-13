using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbGiftProduct
   {
      private static string _collection = "gift_products";

      public static async Task<GiftProductModel> Create(string companyId, GiftProductModel model)
      {
         if (string.IsNullOrEmpty(model.id))
            model.id = Mongo.RandomId();

         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         await collection.InsertOneAsync(model);

         return model;
      }


      public static async Task<GiftProductModel> Update(string companyId, GiftProductModel model)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }


      public static async Task<bool> Delete(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         var result = await collection.DeleteOneAsync(x => x.id == id);

         if (result.DeletedCount > 0)
            return true;
         else
            return false;
      }


      public static async Task<GiftProductModel> Get(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

         return result;
      }


      public static async Task<List<GiftProductModel>> GetList(string companyId, string keyword, string department)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         var list = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

         list = list.OrderByDescending(x => x.name).ToList();

         var results = new List<GiftProductModel>();

         if (!string.IsNullOrEmpty(keyword))
         {
            foreach (var item in list)
            {
               bool check = Handled.Shared.SearchKeyword(keyword, item.name);
               if (check)
                  results.Add(item);
            }
         }
         else
            results = list;

         return results;
      }

      /// <summary>
      /// Lấy danh sách sản phẩm của một danh mục
      /// </summary>
      public static async Task<List<GiftProductModel>> GetListOfCategory(string companyId, int categoryId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         var results = await collection.FindAsync(x => x.category == categoryId).Result.ToListAsync();

         return (from x in results orderby x.sold descending, x.price_list descending select x).ToList();
      }


      public static async Task<List<GiftProductModel>> GetListShow(string companyId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftProductModel>(_collection);

         var results = await collection.FindAsync(x => x.show).Result.ToListAsync();

         return (from x in results orderby x.sold descending, x.price_list descending select x).ToList();
      }


      #region Dữ liệu cố định


      // Chức vụ: danh sách
      public static List<StaticModel> Category()
      {
         var list = new List<StaticModel>();

         list.Add(new StaticModel
         {
            id = 1,
            name = "Đồ ăn, đồ uống",
            icon = "/images/icon-1.png"
         });

         list.Add(new StaticModel
         {
            id = 2,
            name = "Quần áo",
            icon = "/images/icon-3.png"
         });

         list.Add(new StaticModel
         {
            id = 3,
            name = "Văn phòng phẩm",
            icon = "/images/icon-3.png"
         });

         list.Add(new StaticModel
         {
            id = 4,
            name = "Khác",
            icon = "/images/icon-4.png"
         });

         return list;
      }

      // Chức vụ: chi tiết
      public static StaticModel Category(int id)
      {
         var query = from s in Category()
                     where s.id == id
                     select s;
         if (query.Count() > 0)
            return query.FirstOrDefault();
         else
            return new StaticModel();
      }

      #endregion
   }
}
