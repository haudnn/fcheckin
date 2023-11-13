using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbChecklist
  {
    private static string _collection = "checklists";

    public static async Task<CheckListModel> Create(string companyId, CheckListModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CheckListModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<CheckListModel> Update(string companyId, CheckListModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CheckListModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CheckListModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<CheckListModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CheckListModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<List<CheckListModel>> GetList(string companyId, string user)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CheckListModel>(_collection);

      var builder = Builders<CheckListModel>.Filter;

      var filtered = builder.Eq("user", user);

      var result = await collection.FindAsync(x => x.user == user).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }
  }
}
