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
  public class DbMainCompany
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "companys";

    public static async Task<CompanyModel> Create(CompanyModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.create_date = DateTime.Now.Ticks;
      model.status = true;
      model.delete = false;
      model.todolist = new() { time_checkin = "22:00", time_checkout = "22:30", time_confirm = "10:00" };
      model.kaizen = new();
      model.module = new();
      model.products = new();

      var collection = _db.GetCollection<CompanyModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<bool> Delete(string id)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<CompanyModel> Update(CompanyModel model)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<CompanyModel> Get(string id)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var result = await collection.Find(x => x.id == id && !x.delete).FirstOrDefaultAsync();

      if (result != null)
      {
        if (result.products == null)
          result.products = new();
        if (result.todolist == null)
          result.todolist = new();
        if (result.kaizen == null)
          result.kaizen = new();
      }

      return result;
    }


    public static CompanyModel GetById(string id)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var result = collection.Find(x => x.id == id && !x.delete).FirstOrDefault();

      if (result != null)
      {
        if (result.products == null)
          result.products = new();
        if (result.todolist == null)
          result.todolist = new();
        if (result.kaizen == null)
          result.kaizen = new();
      }

      return result;
    }


    /// <summary>
    /// Tất cả các công ty có trong hệ thống
    /// </summary>
    public static async Task<List<CompanyModel>> GetAll()
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var sorted = Builders<CompanyModel>.Sort.Descending("create_date");

      var results = await collection.Find(x => x.delete == false).Sort(sorted).ToListAsync();

      return results;
    }


    /// <summary>
    /// Danh sách công ty của một khách hàng
    /// </summary>
    public static async Task<List<CompanyModel>> GetListOfCustomer(string customerId)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var sorted = Builders<CompanyModel>.Sort.Descending("create_date");

      var results = await collection.Find(x => x.delete == false && x.admin_id == customerId).Sort(sorted).ToListAsync();

      foreach (var item in results)
      {
        if(item.products == null)
          item.products = new();
      }

      return results;
    }
  }
}
