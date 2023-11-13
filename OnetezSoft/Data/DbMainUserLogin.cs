using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbMainUserLogin
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "users_login";

    public static async Task<UserLoginModel> Create(UserLoginModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var collection = _db.GetCollection<UserLoginModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<UserLoginModel> Update(UserLoginModel model)
    {
      model.update = DateTime.Now.Ticks;

      var collection = _db.GetCollection<UserLoginModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<UserLoginModel> Block(UserLoginModel model)
    {
      model.expired = DateTime.Now.AddMinutes(15).Ticks;

      return await Update(model);
    }

    public static async Task<UserLoginModel> Release(UserLoginModel model)
    {
      model.fail_count = 0;
      model.expired = 0;

      return await Update(model);
    }

    public static async Task<UserLoginModel> GetByEmail(string email, GlobalService globalService)
    {
      var user = await DbMainUser.GetbyEmail(email, globalService);

      if (user != null)
      {
        var collection = _db.GetCollection<UserLoginModel>(_collection);

        var result = await collection.FindAsync(x => x.user_id == user.id).Result.FirstOrDefaultAsync();

        return result;
      }
      else
      {
        return null;
      }
    }

    public static async Task<UserLoginModel> Get(string id)
    {
      var collection = _db.GetCollection<UserLoginModel>(_collection);

      var result = await collection.FindAsync(x => x.user_id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    public static async Task<List<UserLoginModel>> GetAll()
    {
      var collection = _db.GetCollection<UserLoginModel>(_collection);

      var result = await collection.FindAsync(x => true).Result.ToListAsync();

      return result;
    }
  }
}
