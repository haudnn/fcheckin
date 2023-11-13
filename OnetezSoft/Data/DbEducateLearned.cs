using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbEducateLearned
  {
    private static string _collection = "educate_learned";

    public static async Task<EducateLearnedModel> Create(string companyId, EducateLearnedModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.status = 1;
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<EducateLearnedModel> Update(string companyId, EducateLearnedModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<EducateLearnedModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<EducateLearnedModel> Get(string companyId, string course, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      return await collection.FindAsync(x => x.course == course && x.user == user).Result.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Danh sách kỳ học của học viên theo giảng viên
    /// <param name="status">1. đang học, 2. Cấp chứng chỉ, 3. Không đạt</param>
    /// </summary>
    public static async Task<List<EducateLearnedModel>> GetList(string companyId, string teacher, string course, string user, int status)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      var builder = Builders<EducateLearnedModel>.Filter;

      var filtered = builder.Gt("date", 0);

      if (!string.IsNullOrEmpty(teacher))
        filtered = filtered & builder.Eq("teacher", teacher);
      if (!string.IsNullOrEmpty(course))
        filtered = filtered & builder.Eq("course", course);
      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      if (status > 0)
        filtered = filtered & builder.Eq("status", status);

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }

    /// <summary>
    /// Danh sách kỳ học của học viên theo giảng viên
    /// <param name="status">1. đang học, 2. Cấp chứng chỉ, 3. Không đạt</param>
    /// </summary>
    public static async Task<List<EducateLearnedModel>> GetListByExaminer(string companyId, string examiner, string course, string user, int status)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      var builder = Builders<EducateLearnedModel>.Filter;

      var filtered = builder.Gt("date", 0);

      if (!string.IsNullOrEmpty(course))
        filtered = filtered & builder.Eq("course", course);
      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      if (status > 0)
        filtered = filtered & builder.Eq("status", status);

      var educateLearnedList = await collection.FindAsync(filtered).Result.ToListAsync();

      educateLearnedList = educateLearnedList.OrderByDescending(x => x.date).ToList();

      if (educateLearnedList.Count > 0)
      {
        var result = new List<EducateLearnedModel>();

        foreach (var learned in educateLearnedList)
        {
          var courseModel = await DbEducateCourse.Get(companyId, learned.course);
          if (courseModel != null)
          {
            if (courseModel.examiner.Contains(examiner) || courseModel.teacher == examiner)
            {
              result.Add(learned);
            }
          }
        }
        return result;
      }

      return educateLearnedList;
    }

    /// <summary>
    /// Danh sách kỳ học của một người
    /// <param name="status">1. đang học, 2. Cấp chứng chỉ, 3. Không đạt</param>
    /// </summary>
    public static async Task<List<EducateLearnedModel>> GetList(string companyId, string user, int status)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      var builder = Builders<EducateLearnedModel>.Filter;

      var filtered = builder.Eq("user", user);

      if (status > 0)
        filtered = filtered & builder.Eq("status", status);

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }


    /// <summary>
    /// Hàm lấy dữ liệu để tính Thành tựu
    /// </summary>
    public static async Task<List<EducateLearnedModel>> DataAchievement(string companyId, string user, DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateLearnedModel>(_collection);

      var builder = Builders<EducateLearnedModel>.Filter;

      var filtered = builder.Eq("status", 2)
        & builder.Gte("certificate_date", start.Ticks)
        & builder.Lt("certificate_date", end.AddDays(1).Ticks);
      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);

      return await collection.FindAsync(filtered).Result.ToListAsync();
    }


    /// <summary>
    /// Tính thành tựu Đào tạo
    /// </summary>
    public static async Task Achievement(string companyId, string user, GlobalService globalService)
    {
      Handled.Shared.GetTimeSpan(2, out DateTime start, out DateTime end);

      var list = await DataAchievement(companyId, user, start, end);

      var achievement = await DbAchievement.GetOption(companyId, "educate", list.Count);
      if (achievement != null)
      {
        var model = new AchievementModel()
        {
          user = user,
          name = achievement.name,
          desc = achievement.des,
          star = achievement.star,
          type = "educate",
          type_id = achievement.id
        };
        await DbAchievement.Create(companyId, model, globalService);
      }
    }


    #region Dữ liệu cố định

    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel { id = 1, name = "Đang học", color = "is-link" });
      list.Add(new StaticModel { id = 2, name = "Cấp chứng chỉ", color = "is-success" });
      list.Add(new StaticModel { id = 3, name = "Không đạt", color = "is-dark" });

      return list;
    }

    public static StaticModel Status(int id)
    {
      var result = Status().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      else
        return new StaticModel { id = id };
    }

    #endregion
  }
}
