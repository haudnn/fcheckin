using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
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


    public static async Task<List<SuggestModel>> GetAll(string companyId, string cycle)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<SuggestModel>(_collection);

      var results = await collection.FindAsync(x => x.cycle == cycle && !x.draft).Result.ToListAsync();

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

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
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

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date select x).ToList();
    }
  }
}
