﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
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


    public static List<DayOffModel> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DayOffModel>(_collection);

      var results = collection.Find(new BsonDocument()).ToList();

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
  }
}
