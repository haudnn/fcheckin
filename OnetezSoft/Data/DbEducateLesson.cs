﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbEducateLesson
  {
    private static string _collection = "educate_lesson";

    public static async Task<EducateLessonModel> Create(string companyId, EducateLessonModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.pos = GetPos(companyId, model.course) + 1;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLessonModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<EducateLessonModel> Update(string companyId, EducateLessonModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLessonModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLessonModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<EducateLessonModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLessonModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    /// <summary>
    /// Danh sách bài học của khoá học
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="course"></param>
    /// <returns></returns>
    public static async Task<List<EducateLessonModel>> GetList(string companyId, string course)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLessonModel>(_collection);

      var sorted = Builders<EducateLessonModel>.Sort.Ascending("pos");

      var results = await collection.Find(x => x.course == course).Sort(sorted).ToListAsync();

      // Xóa khóa học chưa nhập
      var delete = results.Where(x => string.IsNullOrEmpty(x.name)).ToList();
      foreach (var item in delete)
      {
        await Delete(companyId, item.id);
      }

      return results.Where(x => !string.IsNullOrEmpty(x.name)).ToList();
    }

    public static int GetPos(string companyId, string course)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLessonModel>(_collection);

      return collection.Find(x => x.course == course).ToList().Count;
    }

    #region Dữ liệu cố định

    public static List<StaticModel> Type()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel { id = 1, name = "Bài giảng", color = "" });
      list.Add(new StaticModel { id = 2, name = "Bài tự luận", color = "" });
      list.Add(new StaticModel { id = 3, name = "Bài trắc nghiệm", color = "" });

      return list;
    }

    public static StaticModel Type(int id)
    {
      var result = Type().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      else
        return new StaticModel { id = id };
    }

    #endregion
  }
}
