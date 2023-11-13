using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbBackup
   {
      private static string _collection = "backups";

      public static async Task<BackupModel> Create(string companyId, BackupModel backup)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(backup.id))
            {
               backup.id = Mongo.RandomId();
            }
            backup.create = DateTime.Now.Ticks;
            backup.expired = DateTime.Now.AddHours(24).Ticks;

            var _db = Mongo.DbConnect("fastdo_" + companyId);

            var collection = _db.GetCollection<BackupModel>(_collection);

            await collection.InsertOneAsync(backup);

            return backup;
         }
         catch
         {
            return await Create(companyId, backup);
         }
      }

      public static async Task<bool> Update(string companyId, BackupModel backup)
      {
         try
         {
            var _db = Mongo.DbConnect("fastdo_" + companyId);

            var collection = _db.GetCollection<BackupModel>(_collection);
            var option = new ReplaceOptions { IsUpsert = false };
            var result = await collection.ReplaceOneAsync(x => x.id.Equals(backup.id), backup, option);

            return true;
         }
         catch
         {
            return false;
         }
      }

      public static async Task<bool> Delete(string companyId, BackupModel backup)
      {
         try
         {
            var _db = Mongo.DbConnect("fastdo_" + companyId);

            var collection = _db.GetCollection<BackupModel>(_collection);
            var result = await collection.DeleteOneAsync(x => x.id.Equals(backup.id));

            return true;
         }
         catch
         {
            return false;
         }
      }

      public static async Task<BackupModel> Get(string companyId, string userId, string page)
      {
         try
         {
            var _db = Mongo.DbConnect("fastdo_" + companyId);

            var collection = _db.GetCollection<BackupModel>(_collection);
            var result = await collection.FindAsync(x => x.user_id == userId && x.page == page && !x.restored).Result.FirstOrDefaultAsync();

            return result;
         }
         catch
         {
            return null;
         }
      }

      public static async Task<List<BackupModel>> GetList(string companyId, string userId)
      {
         try
         {
            var _db = Mongo.DbConnect("fastdo_" + companyId);

            var collection = _db.GetCollection<BackupModel>(_collection);

            var result = await collection.FindAsync(x => true).Result.ToListAsync();

            return result.OrderByDescending(x => x.create).ToList();
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.ToString());
            return new();
         }
      }
   }
}

