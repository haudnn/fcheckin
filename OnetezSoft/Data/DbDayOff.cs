using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbDayOff
  {
    private static string _collection = "days_off";

    public static async Task<DayOffModel> Create(string companyId, DayOffModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.create = DateTime.Now.Ticks;

      var collection = _db.GetCollection<DayOffModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<DayOffModel> Update(string companyId, DayOffModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayOffModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<DayOffModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayOffModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayOffModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<List<DayOffModel>> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayOffModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderByDescending(x => x.create).ToList();
    }

    public static async Task<List<HrmDayOffModel>> GetAllWithoutDelete(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDayOffModel>(_collection);

      var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      return results.OrderByDescending(x => x.create).ToList();
    }


    public static bool CheckOff(List<DayOffModel> list, DateTime day)
    {
      // Kiểm tra ngày nghỉ 1 lần
      var days = list.Where(x => x.start <= day.Ticks && day.Ticks <= x.end && x.loop == 1);
      if (days.Count() > 0)
        return true;

      // Kiểm tra ngày nghỉ hàng tuần
      var week = list.Where(x => x.start <= day.Ticks && day.Ticks <= x.end && x.loop == 2).ToList();
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

    /// <summary>
    /// Kiểm tra có phải ngày nghỉ không
    /// </summary>
    public static bool CheckOff(List<DayOffModel> list, DateTime day, out bool has_salary)
    {
      has_salary = false;

      // Kiểm tra ngày nghỉ 1 lần
      var dayOff = list.Where(x => x.start <= day.Ticks && day.Ticks <= x.end && x.loop == 1).FirstOrDefault();
      if (dayOff != null)
      {
        has_salary = dayOff.has_salary;
        return true;
      }

      // Kiểm tra ngày nghỉ hàng tuần
      var week = list.Where(x => x.start <= day.Ticks && day.Ticks <= x.end && x.loop == 2).ToList();
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
