using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbEducateCourse
  {
    private static string _collection = "educate_course";

    public static async Task<EducateCourseModel> Create(string companyId, EducateCourseModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;
      model.reviews = new();
      model.bookmarks = new();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<EducateCourseModel> Update(string companyId, EducateCourseModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<EducateCourseModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<List<EducateCourseModel>> GetList(string companyId, string keyword, string category,
      string creator, string teacher)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      var builder = Builders<EducateCourseModel>.Filter;

      var filtered = builder.Gt("date", 0);

      if (!string.IsNullOrEmpty(category))
        filtered = filtered & builder.Eq("category", category);
      if (!string.IsNullOrEmpty(creator))
        filtered = filtered & builder.Eq("creator", creator);
      if (!string.IsNullOrEmpty(teacher))
        filtered = filtered & builder.Eq("teacher", teacher);

      var list = await collection.FindAsync(filtered).Result.ToListAsync();

      list = list.OrderByDescending(x => x.date).ToList();

      var results = new List<EducateCourseModel>();

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


    public static List<EducateCourseModel> GetList(string companyId, string category, string sortby, int size, out int total)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      var builder = Builders<EducateCourseModel>.Filter;

      var filtered = builder.Eq("show", true);

      if (!string.IsNullOrEmpty(category))
        filtered = filtered & builder.Eq("category", category);

      var sorted = Builders<EducateCourseModel>.Sort.Descending(sortby);

      var results = collection.Find(filtered).Sort(sorted).ToList();

      if (sortby == "random")
        results = results.OrderBy(x => Guid.NewGuid()).ToList();

      total = results.Count;

      if (size > 0)
        return results.Take(size).ToList();
      else
        return results;
    }


    public static List<EducateCourseModel> GetBookmark(string companyId, string user, int size, out int total)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      var builder = Builders<EducateCourseModel>.Filter;

      var filtered = builder.Eq("show", true) & builder.Eq("bookmarks", user);

      var sorted = Builders<EducateCourseModel>.Sort.Descending("date");

      var results = collection.Find(filtered).Sort(sorted).ToList();

      total = results.Count;

      if (size > 0)
        return results.Take(size).ToList();
      else
        return results;
    }

    /// <summary>
    /// Danh sách khóa học được phép chấm thi
    /// </summary>
    public static async Task<List<EducateCourseModel>> GetListByExaminer(string companyId, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCourseModel>(_collection);

      var builder = Builders<EducateCourseModel>.Filter;

      var sorted = Builders<EducateCourseModel>.Sort.Descending("date");

      return await collection.Find(x => x.teacher == userId || x.examiner.Contains(userId)).Sort(sorted).ToListAsync();
    }
  }
}