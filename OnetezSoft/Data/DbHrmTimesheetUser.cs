using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbHrmTimesheetUser
   {
      private static string _collection = "hrm_timesheet_user";

      public static async Task<HrmTimesheetUserModel> Create(string companyId, HrmTimesheetUserModel model)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         if (string.IsNullOrEmpty(model.id))
            model.id = Mongo.RandomId();
         model.updated = DateTime.Now.Ticks;

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         await collection.InsertOneAsync(model);

         return model;
      }


      public static async Task<HrmTimesheetUserModel> Update(string companyId, HrmTimesheetUserModel model)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         model.updated = DateTime.Now.Ticks;

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }


      public static async Task<bool> Delete(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         var result = await collection.DeleteOneAsync(x => x.id == id);

         if (result.DeletedCount > 0)
            return true;
         else
            return false;
      }


      public static async Task<HrmTimesheetUserModel> Get(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
      }


      public static async Task<List<HrmTimesheetUserModel>> GetList(string companyId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

         return results.OrderByDescending(x => x.updated).ToList();
      }

      /// <summary>Lấy tất cả dữ liệu bảng công theo range kể cả bị xoá</summary>
      public static async Task<List<HrmTimesheetUserModel>> GetAllByRangeUserId(string companyId, long start, long end, string userId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.user == userId && ((x.from >= start && x.from <= end) || (x.from <= start && x.to >= start))).Result.ToListAsync();
      }

      /// <summary>Lấy tất cả dữ liệu bảng công theo range theo danh sách user => lấy danh sách để cập nhật bảng công</summary>
      public static async Task<List<HrmTimesheetUserModel>> GetAllByRangeUpdate(string companyId, long start, long end, List<string> userList, List<string> existList)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => !existList.Contains(x.id) && userList.Contains(x.user)
                                                && ((x.from >= start && x.from <= end) || (x.from <= start && x.to >= start))).Result.ToListAsync();
      }

      /// <summary>Lấy tất cả dữ liệu bảng công sheet id và user id</summary>
      public static async Task<HrmTimesheetUserModel> GetByTimesheetId(string companyId, string userId, string timesheetId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.user == userId && x.timesheet_id == timesheetId).Result.FirstOrDefaultAsync();
      }

      /// <summary>Lấy tất cả dữ liệu bảng công sheet id</summary>
      public static async Task<List<HrmTimesheetUserModel>> GetAllByTimesheetId(string companyId, string timesheetId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.timesheet_id == timesheetId).Result.ToListAsync();
      }

      /// <summary>Lấy tất cả dữ liệu bảng công sheet id và chưa bị xoá trong bảng công(TH xảy ra khi chỉnh sửa ngày bảng công)</summary>
      public static async Task<List<HrmTimesheetUserModel>> GetAllShowByTimesheetId(string companyId, string timesheetId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.timesheet_id == timesheetId && !x.is_delete).Result.ToListAsync();
      }

      /// <summary>Lấy tất cả dữ liệu bảng công sheet id và chưa bị xoá trong bảng công(TH xảy ra khi chỉnh sửa ngày bảng công)</summary>
      public static async Task<List<HrmTimesheetUserModel>> GetAllShowUserId(string companyId, string userId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.user == userId && !x.is_delete).Result.ToListAsync();
      }

      /// <summary>Lấy dữ liệu bao gồm ngày chấm công => lấy dữ liệu khi chấm công</summary>
      public static async Task<List<HrmTimesheetUserModel>> GetAllByDate(string companyId, string userId, long date)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmTimesheetUserModel>(_collection);

         return await collection.FindAsync(x => x.user == userId && x.from <= date && date <= x.to).Result.ToListAsync();
      }
   }
}