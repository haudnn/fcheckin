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
  public class DbOkrTask
  {
    private static string _collection = "okr_tasks";

    public static async Task<OkrTaskModel> Create(string companyId, OkrTaskModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<OkrTaskModel> Update(string companyId, OkrTaskModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<OkrTaskModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static List<OkrTaskModel> GetAll(string companyId, string cycle, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      var results = collection.Find(x => x.cycle == cycle && x.user == user).ToList();

      return results.OrderBy(x => x.start).ToList();
    }


    public static async Task<List<OkrTaskModel>> GetList(string companyId, string cycle, string user, 
      DateTime? dateStart, DateTime? dateEnd)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      var builder = Builders<OkrTaskModel>.Filter;

      var filtered = builder.Eq("cycle", cycle)
        & builder.Eq("user", user);

      if (dateStart != null && dateEnd != null)
      {
        var start = dateStart.Value.Ticks;
        var end = dateEnd.Value.Ticks;

        var filter = (builder.Gte("start", start) & builder.Lte("start", end))
           | (builder.Gte("end", start) & builder.Lte("end", end))
           | (builder.Lte("start", start) & builder.Gte("end", end));
        filtered = filtered & filter;
      }

      var sorted = Builders<OkrTaskModel>.Sort.Ascending("start");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }


    public static List<OkrTaskModel> GetList(string companyId, string cycle, string okr, string kr)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<OkrTaskModel>(_collection);

      var builder = Builders<OkrTaskModel>.Filter;

      var filtered = builder.Eq("cycle", cycle)
        & builder.Eq("okr", okr)
        & builder.Eq("kr", kr);

      var sorted = Builders<OkrTaskModel>.Sort.Ascending("start");

      return collection.Find(filtered).Sort(sorted).ToList();
    }



    #region Dữ liệu cố định

    // Trạng thái: danh sách
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "TODO",
        color = "",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "DOING",
        color = "has-text-link",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "DONE",
        color = "has-text-success",
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
