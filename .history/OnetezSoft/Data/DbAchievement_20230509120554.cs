using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbAchievement
  {
    private static string _collection = "achievement";

    public static async Task<AchievementModel> Create(string companyId, AchievementModel model)
    {
      model.id = Guid.NewGuid().ToString();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      await collection.InsertOneAsync(model);

      // Công sao cho nhân viên khi đạt thành tựu
      var user = await DbUser.Get(companyId, model.user);
      if(user != null)
      {
        user.star_total += model.star;
        user.star_receive += model.star;
        await DbUser.Update(companyId, user);
      }

      return model;
    }


    public static async Task<AchievementModel> Update(string companyId, AchievementModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    /// <summary>
    /// Lấy danh sách thành tựu của 1 người
    /// </summary>
    public static async Task<List<AchievementModel>> GetList(string companyId, string user, string type, 
      DateTime start, DateTime end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var builder = Builders<AchievementModel>.Filter;

      var filtered = builder.Gte("date", start.Ticks) & builder.Lt("date", end.AddDays(1).Ticks);

      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      if(!string.IsNullOrEmpty(type))
        filtered = filtered & builder.Eq("type", type);

      var sorted = Builders<AchievementModel>.Sort.Descending("date");

      var results = await collection.Find(filtered).Sort(sorted).ToListAsync();

      return results;
    }


    /// <summary>
    /// Lấy thành tựu chưa xem của 1 người
    /// </summary>
    public static async Task<AchievementModel> GetNew(string companyId, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<AchievementModel>(_collection);

      var sorted = Builders<AchievementModel>.Sort.Descending("date");

      var list = await collection.Find(x => x.user == user && x.view == false).Sort(sorted).ToListAsync();

      if (list.Count > 0)
        return list[0];
      else
        return null;
    }


    #region Dữ liệu cấp sao

    public static List<StaticModel> Todolist()
    {
      var results = new List<StaticModel>();
      results.Add(new StaticModel() { id = 5, name = "Không quên Todolist", color = "5 ngày liên tiếp không trễ todolist", icon = "1" });
      results.Add(new StaticModel() { id = 10, name = "Thích Todolist", color = "10 ngày liên tiếp không trễ todolist", icon = "5" });
      results.Add(new StaticModel() { id = 15, name = "Yêu Todolist", color = "15 ngày liên tiếp không trễ todolist", icon = "10" });
      results.Add(new StaticModel() { id = 20, name = "Ghiền Todolist", color = "20 ngày liên tiếp không trễ todolist", icon = "20" });
      results.Add(new StaticModel() { id = 25, name = "Nghiện Todolist", color = "25 ngày liên tiếp không trễ todolist", icon = "35" });
      return results;
    }

    public static StaticModel Todolist(int count)
    {
      if (count == 0)
        return null;
      else if (count % 25 == 0)
        return Todolist().SingleOrDefault(x => x.id == 25);
      else
        return Todolist().SingleOrDefault(x => x.id == count % 25);
    }


    public static List<StaticModel> Kaizen()
    {
      var results = new List<StaticModel>();
      results.Add(new StaticModel() { id = 3, name = "Gieo hạt", color = "Có 3 góp ý Kaizen có ích", icon = "5" });
      results.Add(new StaticModel() { id = 5, name = "Ươm mầm", color = "Có 5 góp ý Kaizen có ích", icon = "10" });
      results.Add(new StaticModel() { id = 7, name = "Đơm hoa", color = "Có 7 góp ý Kaizen có ích", icon = "20" });
      results.Add(new StaticModel() { id = 10, name = "Kết trái", color = "Có 10 góp ý Kaizen có ích", icon = "35" });
      return results;
    }

    public static StaticModel Kaizen(int count)
    {
      if (count == 0)
        return null;
      else if (count % 10 == 0)
        return Kaizen().SingleOrDefault(x => x.id == 10);
      else
        return Kaizen().SingleOrDefault(x => x.id == count % 10);
    }


    public static List<StaticModel> CFRs()
    {
      var results = new List<StaticModel>();
      results.Add(new StaticModel() { id = 3, name = "Nhân viên chăm chỉ", color = "Được ghi nhận 3 lần", icon = "5" });
      results.Add(new StaticModel() { id = 5, name = "Nhân viên giỏi", color = "Được ghi nhận 5 lần", icon = "10" });
      results.Add(new StaticModel() { id = 7, name = "Nhân viên tuyệt vời", color = "Được ghi nhận 7 lần", icon = "20" });
      results.Add(new StaticModel() { id = 10, name = "Nhân viên xuất sắc", color = "Được ghi nhận 10 lần", icon = "35" });
      return results;
    }

    public static StaticModel CFRs(int count)
    {
      if (count == 0)
        return null;
      else if (count % 10 == 0)
        return CFRs().SingleOrDefault(x => x.id == 10);
      else
        return CFRs().SingleOrDefault(x => x.id == count % 10);
    }


    public static List<StaticModel> Educate()
    {
      var results = new List<StaticModel>();
      results.Add(new StaticModel() { id = 3, name = "Tốt nghiệp Mẫu giáo", color = "Nhận được 3 chứng chỉ Đào tạo", icon = "5" });
      results.Add(new StaticModel() { id = 5, name = "Tốt nghiệp Tiểu học", color = "Nhận được 5 chứng chỉ Đào tạo", icon = "10" });
      results.Add(new StaticModel() { id = 7, name = "Tốt nghiệp Trung học", color = "Nhận được 7 chứng chỉ Đào tạo", icon = "20" });
      results.Add(new StaticModel() { id = 10, name = "Tốt nghiệp Đại học", color = "Nhận được 10 chứng chỉ Đào tạo", icon = "35" });
      return results;
    }

    public static StaticModel Educate(int count)
    {
      if (count == 0)
        return null;
      else if (count % 10 == 0)
        return Educate().SingleOrDefault(x => x.id == 10);
      else
        return Educate().SingleOrDefault(x => x.id == count % 10);
    }

    #endregion
  }
}
