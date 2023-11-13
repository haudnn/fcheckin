using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbOkrLog
{
   private static string _collection = "okr_log";

   public static async Task<OkrLogModel> Create(string companyId, OkrLogModel model)
   {
      if (string.IsNullOrEmpty(model.id))
         model.id = Mongo.RandomId();
      model.created = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrLogModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
   }


   public static async Task<OkrLogModel> Update(string companyId, OkrLogModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrLogModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
   }


   public static async Task<List<OkrLogModel>> GetList(string companyId, string userId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrLogModel>(_collection);

      var results = await collection.FindAsync(x => x.user_id == userId).Result.ToListAsync();

      return results.OrderByDescending(x => x.created).ToList();
   }
}