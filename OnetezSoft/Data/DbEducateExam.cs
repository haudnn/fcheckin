using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbEducateExam
  {
    private static string _collection = "educate_exam";

    public static async Task<EducateExamModel> Create(string companyId, EducateExamModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateExamModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<EducateExamModel> Update(string companyId, EducateExamModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateExamModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateExamModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<EducateExamModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateExamModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<EducateExamModel> Get(string companyId, string lesson, string learned, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateExamModel>(_collection);

      var builder = Builders<EducateExamModel>.Filter;

      var filtered = builder.Eq("lesson", lesson)
        & builder.Eq("learned", learned)
        & builder.Eq("user", user);

      return await collection.FindAsync(filtered).Result.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Danh sách bài thi tự luận
    /// </summary>
    public static async Task<List<EducateExamModel>> GetList(string companyId, string teacher, string course, string lesson, string user, bool? check)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateExamModel>(_collection);

      var builder = Builders<EducateExamModel>.Filter;

      var filtered = builder.Eq("type", 2);

      // if (!string.IsNullOrEmpty(teacher))
      //   filtered = filtered & builder.Eq("teacher", teacher);
      if (!string.IsNullOrEmpty(course))
        filtered = filtered & builder.Eq("course", course);
      if (!string.IsNullOrEmpty(lesson))
        filtered = filtered & builder.Eq("lesson", lesson);
      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      if (check != null)
        filtered = filtered & builder.Eq("check", check);

      var sorted = Builders<EducateExamModel>.Sort.Descending("date");

      var examEducateList = await collection.FindAsync(filtered).Result.ToListAsync();

      examEducateList = examEducateList.OrderByDescending(x => x.date).ToList();

      if (examEducateList.Count > 0)
      {
        var result = new List<EducateExamModel>();

        foreach (var exam in examEducateList)
        {
          var courseModel = await DbEducateCourse.Get(companyId, exam.course);
          if (courseModel != null)
          {
            if (courseModel.examiner.Contains(teacher) || courseModel.teacher == teacher)
            {
              result.Add(exam);
            }
          }
        }
        return result;
      }

      return examEducateList;
    }
  }
}
