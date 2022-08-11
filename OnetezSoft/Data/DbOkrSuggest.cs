using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbOkrSuggest
  {
    private static string _collection = "okr_suggests";

    public static async Task<SuggestModel> Create(string companyId, SuggestModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<SuggestModel> Update(string companyId, SuggestModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<SuggestModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static List<SuggestModel> GetAll(string companyId, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      var results = collection.Find(x => x.cycle == cycle && !x.draft).ToList();

      return results.OrderByDescending(x => x.date).ToList();
    }


    public static async Task<List<SuggestModel>> GetList(string companyId, string cycle, string department, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      var builder = Builders<SuggestModel>.Filter;

      var filtered = builder.Eq("cycle", cycle)
         & builder.Eq("draft", false);

      if (!string.IsNullOrEmpty(user))
        filtered = filtered & builder.Eq("user", user);
      else
        filtered = filtered & builder.Eq("department", department);

      var sorted = Builders<SuggestModel>.Sort.Descending("date");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }


    public static async Task<List<SuggestModel>> GetDrafts(string companyId, string cycle, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      var builder = Builders<SuggestModel>.Filter;

      var filtered = builder.Eq("cycle", cycle)
         & builder.Eq("draft", true)
         & builder.Eq("user", user);

      var sorted = Builders<SuggestModel>.Sort.Ascending("date");

      return await collection.Find(filtered).Sort(sorted).ToListAsync();
    }
  }
}
