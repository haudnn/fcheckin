using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbGiftExchange
   {
      private static string _collection = "gift_exchanges";

      public static async Task<GiftExchangeModel> Create(string companyId, GiftExchangeModel model)
      {
         if (string.IsNullOrEmpty(model.id))
            model.id = Mongo.RandomId();
         model.date = DateTime.Now.Ticks;
         model.view = model.user_buy == model.user_give;

         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         await collection.InsertOneAsync(model);

         return model;
      }


      public static async Task<GiftExchangeModel> Update(string companyId, GiftExchangeModel model)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }


      public static async Task<bool> Delete(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var result = await collection.DeleteOneAsync(x => x.id == id);

         if (result.DeletedCount > 0)
            return true;
         else
            return false;
      }


      public static async Task<GiftExchangeModel> Get(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

         return result;
      }


      public static async Task<List<GiftExchangeModel>> GetList(string companyId, string keyword, int status)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var builder = Builders<GiftExchangeModel>.Filter;

         var filtered = builder.Eq("buy", true);

         if (status != 0)
            filtered = filtered & builder.Eq("status", status);

         var list = await collection.FindAsync(filtered).Result.ToListAsync();

         list = list.OrderByDescending(x => x.date).ToList();

         var results = new List<GiftExchangeModel>();

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

         return (from x in results orderby x.status, x.date descending select x).ToList();
      }

      /// <summary>
      /// Lịch sử đổi quà
      /// </summary>
      public static async Task<List<GiftExchangeModel>> GetListBuy(string companyId, string user, bool buy)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var builder = Builders<GiftExchangeModel>.Filter;

         var filtered = builder.Eq("user_buy", user) & builder.Eq("buy", buy);

         var result = await collection.FindAsync(filtered).Result.ToListAsync();

         return (from x in result orderby x.date descending select x).ToList();
      }

      /// <summary>
      /// Quà nhận được 
      /// </summary>
      public static async Task<List<GiftExchangeModel>> GetListGive(string companyId, string user)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var sorted = Builders<GiftExchangeModel>.Sort.Descending("date");

         var result = await collection.FindAsync(x => x.buy && x.user_give == user && x.user_buy != user).Result.ToListAsync();

         return (from x in result orderby x.date descending select x).ToList();
      }


      /// <summary>
      /// Quà nhận được nhưng chưa xem
      /// </summary>
      public static async Task<GiftExchangeModel> GetGiveNew(string companyId, string user)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<GiftExchangeModel>(_collection);

         var sorted = Builders<GiftExchangeModel>.Sort.Descending("date");

         var list = await collection.FindAsync(x => x.buy && x.status == 2 && x.view == false
           && x.user_give == user && x.user_buy != user
           ).Result.ToListAsync();

         list = list.OrderByDescending(x => x.date).ToList();

         if (list.Count > 0)
            return list[0];
         else
            return null;
      }


      #region Dữ liệu cố định


      // Chức vụ: danh sách
      public static List<StaticModel> Status()
      {
         var list = new List<StaticModel>();

         list.Add(new StaticModel
         {
            id = 1,
            name = "Chờ xử lý",
            color = "is-warning",
         });

         list.Add(new StaticModel
         {
            id = 2,
            name = "Đã duyệt",
            color = "is-success",
         });

         list.Add(new StaticModel
         {
            id = 3,
            name = "Đã hủy",
            color = "is-dark",
         });

         return list;
      }

      // Chức vụ: chi tiết
      public static StaticModel Status(int id)
      {
         var query = from s in Status()
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
