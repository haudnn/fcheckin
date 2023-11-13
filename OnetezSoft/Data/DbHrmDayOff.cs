using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbHrmDayOff
  {
    private static string _collection = "hrm_daysoff";

    public static async Task<HrmDayOffModel> Create(string companyId, HrmDayOffModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.create = DateTime.Now.Ticks;

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      await collection.InsertOneAsync(model);

      if (model.non_salary_users.Count > 0 || model.salary_users.Count > 0)
      {
        await DbHrmTimeList.HandleDayOff(companyId, model);
        await DbHrmTimeListRegister.HandleDayOff(companyId, model);
      }

      return model;
    }


    public static async Task<HrmDayOffModel> Update(string companyId, HrmDayOffModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);


      // Xoá cũ
      await DeleteTimeList(companyId, model.id, false, model.loop == 1 ? true : false);
      // Thêm mới
      if (model.non_salary_users.Count > 0 || model.salary_users.Count > 0)
      {
        await DbHrmTimeList.HandleDayOff(companyId, model);
        await DbHrmTimeListRegister.HandleDayOff(companyId, model);
      }

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<HrmDayOffModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      return await collection.FindAsync(x => x.id == id && !x.isDeleted).Result.FirstOrDefaultAsync();
    }

    // Ngày nghỉ chưa diễn ra
    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      var dayOff = await Get(companyId, id);

      await DeleteTimeList(companyId, id, false, dayOff.loop == 1 ? true : false);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    public static async Task<bool> SoftDelete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      await DeleteTimeList(companyId, id, true);

      var filter = Builders<HrmDayOffModel>.Filter.Eq(x => x.id, id);
      var update = Builders<HrmDayOffModel>.Update.Set(x => x.isDeleted, true);

      var result = await collection.FindOneAndUpdateAsync(filter, update);
      if (result != null)
        return true;
      else
        return false;
    }


    public static async Task<List<HrmDayOffModel>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      var results = await collection.FindAsync(x => !x.isDeleted).Result.ToListAsync();

      var sortedResults = results.OrderBy(x => x.start).ToList();

      return sortedResults;
    }

    public static async Task<List<HrmDayOffModel>> GetAll(string companyId, bool isAll)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderByDescending(x => x.create).ToList();
    }

    // Hàm xử lý xoá bên phân ca

    public static async Task DeleteTimeList(string companyId, string id, bool isSoftDelete = false, bool isAll = false)
    {
      var data = await Get(companyId, id);
      var start = new DateTime(data.start);
      var end = new DateTime(data.end);

      if (data.salary_users.Count > 0)
      {
        for (DateTime date = end; date >= start; date = date.AddDays(-1))
        {
          foreach (var userid in data.salary_users)
          {
            if (isAll || isSoftDelete)
            {
              await DbHrmTimeList.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
              await DbHrmTimeListRegister.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
            }
            else
            {
              var getDay = await DbHrmTimeList.GetByDay(companyId, userid, date.Ticks);
              if (getDay != null && getDay.dayoff_id != null)
              {
                var dayOff = await Get(companyId, getDay.dayoff_id);
                var isHoliday = dayOff.loop == 1;
                if (isHoliday)
                  continue;
                await DbHrmTimeList.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
              }

              // đăng ký phân ca
              var getDayRegister = await DbHrmTimeListRegister.GetByDay(companyId, userid, date.Ticks);
              if (getDayRegister != null && getDayRegister.dayoff_id != null)
              {
                var dayOff = await DbHrmDayOff.Get(companyId, getDayRegister.dayoff_id);
                var isHoliday = dayOff.loop == 1;
                if (isHoliday)
                  continue;
                await DbHrmTimeListRegister.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
              }
            }
          }
        }
      }

      if (data.non_salary_users.Count > 0)
      {
        for (DateTime date = end; date >= start; date = date.AddDays(-1))
        {
          foreach (var userid in data.non_salary_users)
          {
            if (isAll || isSoftDelete)
            {
              await DbHrmTimeList.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
              await DbHrmTimeListRegister.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
            }
            else
            {
              var getDay = await DbHrmTimeList.GetByDay(companyId, userid, date.Ticks);
              if (getDay != null && getDay.dayoff_id != null)
              {
                var dayOff = await Get(companyId, getDay.dayoff_id);
                var isHoliday = dayOff.loop == 1;
                if (isHoliday)
                  continue;
                await DbHrmTimeList.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
              }

              // đăng ký phân ca
              var getDayRegister = await DbHrmTimeListRegister.GetByDay(companyId, userid, date.Ticks);
              if (getDayRegister != null && getDayRegister.dayoff_id != null)
              {
                var dayOff = await Get(companyId, getDayRegister.dayoff_id);
                var isHoliday = dayOff.loop == 1;
                if (isHoliday)
                  continue;
                await DbHrmTimeListRegister.DeleteDayOff(companyId, userid, data.id, date, isSoftDelete);
              }
            }
          }
        }
      }
    }

    public static async Task<List<HrmDayOffModel>> GetAllWithoutDelete(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderByDescending(x => x.create).ToList();
    }

    public static async Task<bool> CheckOff(string companyId, DateTime day, string userId)
    {
      var list = await GetAll(companyId);
      // Kiểm tra ngày nghỉ 1 lần
      var days = list.Where(x => x.start <= day.Ticks && day.Ticks <= x.end && x.loop == 1 && (x.salary_users.Contains(userId) || x.non_salary_users.Contains(userId))).ToList();
      if (days.Count() > 0)
        return true;

      // Kiểm tra ngày nghỉ hàng tuần
      var week = list.Where(x => x.start <= day.Ticks && day.Ticks <= x.end && x.loop == 2 && (x.salary_users.Contains(userId) || x.non_salary_users.Contains(userId))).ToList();
      foreach (var item in week)
      {
        if (day.DayOfWeek == DayOfWeek.Monday && item.loop_week.mon)
          return true;
        else if (day.DayOfWeek == DayOfWeek.Tuesday && item.loop_week.tue)
          return true;
        else if (day.DayOfWeek == DayOfWeek.Wednesday && item.loop_week.wed)
          return true;
        else if (day.DayOfWeek == DayOfWeek.Thursday && item.loop_week.thu)
          return true;
        else if (day.DayOfWeek == DayOfWeek.Friday && item.loop_week.fri)
          return true;
        else if (day.DayOfWeek == DayOfWeek.Saturday && item.loop_week.sat)
          return true;
        else if (day.DayOfWeek == DayOfWeek.Sunday && item.loop_week.sun)
          return true;
      }

      return false;
    }

  }
}
