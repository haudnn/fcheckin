using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbOkr
  {
    private static string _collection = "okrs";

    public static async Task<OkrModel> Create(string companyId, OkrModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date_create = DateTime.Now.Ticks;
      model.delete = false;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<OkrModel> Update(string companyId, OkrModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<OkrModel> Get(string companyId, string id)
    {
      if (string.IsNullOrEmpty(id))
        return null;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<OkrModel> Get(string companyId, string id, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      return await collection.FindAsync(x => x.id == id && x.cycle == cycle).Result.FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<List<OkrModel>> GetAll(string companyId, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      var results = await collection.FindAsync(x => x.cycle == cycle && !x.delete).Result.ToListAsync();

      return results.OrderByDescending(x => x.user_create).ToList();
    }


    public static async Task<List<OkrModel>> GetList(string companyId, string cycle, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      var builder = Builders<OkrModel>.Filter;

      var filtered = builder.Eq("cycle", cycle)
         & builder.Eq("delete", false);

      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user_create", user);

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return result.OrderBy(x => x.date_create).ToList();
    }


    /// <summary>
    /// Danh sách OKRs theo danh sách User
    /// </summary>
    public static async Task<List<OkrModel>> GetList(string companyId, string cycle, List<string> listUser)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      var results = await collection.FindAsync(x => x.cycle == cycle && !x.delete
          && listUser.Contains(x.user_create)).Result.ToListAsync();

      return results.OrderByDescending(x => x.user_create).ToList();
    }


    /// <summary>
    /// Danh sách OKRs mà User là người xem hoặc là người đánh giá
    /// </summary>
    public static async Task<List<OkrModel>> GetListByReview(string companyId, string cycle, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrModel>(_collection);

      var results = await collection.FindAsync(x => x.cycle == cycle && !x.delete
          && (x.review_manager_id == userId || x.review_viewers.Contains(userId))).Result.ToListAsync();

      return results.OrderByDescending(x => x.user_create).ToList();
    }


    /// <summary>
    /// Tính % tiến độ hoàn thành của OKRs
    /// </summary>
    public static double GetProgress(List<OkrModel.KeyResult> keyResults)
    {
      if (keyResults != null && keyResults.Count > 0)
      {
        double total = 0;
        foreach (var kr in keyResults)
        {
          total += Handled.Shared.Progress(kr.result, kr.target);
        }

        return total / keyResults.Count;
      }
      else
        return 0;
    }


    #region Dữ liệu cố định

    // Phân loại: danh sách
    public static List<StaticModel> Type()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 2,
        name = "OKRs cam kết",
        color = "is-danger",
      });

      list.Add(new StaticModel
      {
        id = 1,
        name = "OKRs mở rộng",
        color = "is-primary",
      });

      return list;
    }

    // Phân loại: chi tiết
    public static StaticModel Type(int id)
    {
      var query = from s in Type()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    // Mức độ tự tin: danh sách
    public static List<StaticModel> Confident()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Rất tốt",
        color = "has-text-success",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Ổn",
        color = "has-text-info",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Không ổn",
        color = "has-text-danger",
      });

      return list;
    }

    // Mức độ tự tin: chi tiết
    public static StaticModel Confident(int id)
    {
      var query = from s in Confident()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }


    // Tình trang checkin: danh sách
    public static List<StaticModel> Checkin()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Đúng hạn",
        color = "is-success",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Quá hạn",
        color = "is-danger",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Chưa checkin",
        color = "is-warning",
      });

      return list;
    }

    #endregion
  }
}