using MongoDB.Driver;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbOkrTimeline
  {
    private static string _collection = "okr_timelines";

    public static async Task<TimelineModel> Create(string companyId, TimelineModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.status = 1;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TimelineModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<TimelineModel> Update(string companyId, TimelineModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TimelineModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<TimelineModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TimelineModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TimelineModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<List<TimelineModel>> GetAll(string companyId, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TimelineModel>(_collection);

      var results = await collection.FindAsync(x => x.cycle == cycle).Result.ToListAsync();

      return results.OrderBy(x => x.start).ToList();
    }


    public static async Task<List<TimelineModel>> GetList(string companyId, string cycle, string department)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TimelineModel>(_collection);

      var results = await collection.FindAsync(x => x.cycle == cycle && x.department == department).Result.ToListAsync();

      return results.OrderBy(x => x.start).ToList();
    }



    #region Dữ liệu cố định

    // Trạng thái: danh sách
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Todo",
        color = "",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Doing",
        color = "is-link",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Done",
        color = "is-success",
      });

      list.Add(new StaticModel
      {
        id = 4,
        name = "Cancel",
        color = "is-dark",
      });

      return list;
    }

    // Trạng thái: chi tiết
    public static StaticModel Status(int id)
    {
      var query = from s in Status()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    #endregion
  }
}
