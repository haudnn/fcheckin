using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbOkrCheckin
  {
    private static string _collection = "okr_checkins";

    public static async Task<OkrCheckinModel> Create(string companyId, OkrCheckinModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      if (model.key_results == null)
        model.key_results = new();
      model.date_create = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<OkrCheckinModel> Update(string companyId, OkrCheckinModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<OkrCheckinModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    /// <summary>
    /// Danh sách check-in nháp của 1 OKRs
    /// </summary>
    public static async Task<List<OkrCheckinModel>> GetDraft(string companyId, string okr)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      var results = await collection.FindAsync(x => x.okr == okr && x.draft).Result.ToListAsync();

      return results.OrderByDescending(x => x.date_create).ToList();
    }

    /// <summary>
    /// Danh sách check-in của 1 OKRs
    /// </summary>
    public static async Task<List<OkrCheckinModel>> GetList(string companyId, string cycle, string okr, bool? checkin)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      var builder = Builders<OkrCheckinModel>.Filter;

      var filtered = builder.Eq("cycle", cycle)
        & builder.Eq("okr", okr);

      if (checkin != null)
        filtered = filtered & builder.Eq("checkin", checkin.Value);

      var sorted = Builders<OkrCheckinModel>.Sort.Descending("date_create");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      result = result.OrderByDescending(x => x.date_create).ToList();

      return result;
    }

    /// <summary>
    /// Danh sách check-in của OKRs theo thời gian
    /// </summary>
    public static async Task<List<OkrCheckinModel>> GetList(string companyId, string cycle,
      DateTime? start, DateTime? end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrCheckinModel>(_collection);

      var builder = Builders<OkrCheckinModel>.Filter;

      var filtered = builder.Eq("cycle", cycle);

      if (start != null)
        filtered = filtered & builder.Gte("date_create", start.Value.Ticks);
      if (end != null)
        filtered = filtered & builder.Lt("date_create", end.Value.AddDays(1).Ticks);

      var sorted = Builders<OkrCheckinModel>.Sort.Descending("date_create");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date_create descending select x).ToList();
    }


    /// <summary>
    /// Tính % tiến độ hoàn thành của OKRs
    /// </summary>
    public static double GetProgress(List<OkrCheckinModel.KeyResult> keyCheckin, List<OkrModel.KeyResult> keyResults)
    {
      if (keyCheckin != null && keyCheckin.Count > 0)
      {
        double total = 0;
        foreach (var kr in keyResults)
        {
          var checkin = keyCheckin.SingleOrDefault(x => x.id == kr.id);
          if (checkin != null)
            total += Handled.Shared.Progress(checkin.result, kr.target);
        }

        return total / keyResults.Count;
      }
      else
        return 0;
    }


    #region Dữ liệu cố định

    // Câu hỏi: danh sách
    public static List<StaticModel> Question()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 0,
        name = "Tiến độ, kết quả công việc?"
      });

      list.Add(new StaticModel
      {
        id = 1,
        name = "Công việc nào đang & sẽ chậm tiến độ?"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Trở ngại, khó khăn là gì?"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Cần làm gì để vượt qua trở ngại?"
      });

      return list;
    }

    // Câu hỏi: chi tiết
    public static StaticModel Question(int id)
    {
      var query = from s in Question()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    #endregion
  }
}
