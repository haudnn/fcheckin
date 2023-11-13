using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

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

      var result = await collection.FindAsync(x => x.id == id && !x.delete).Result.FirstOrDefaultAsync();

      if (result != null)
      {
        result.products ??= new();
        result.todolist ??= new();
        result.kaizen ??= new();
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

      var results = await collection.FindAsync(x => x.delete == false).Result.ToListAsync();

      return (from x in results orderby x.create_date descending select x).ToList();
    }


    /// <summary>
    /// Danh sách công ty của một khách hàng
    /// </summary>
    public static async Task<List<CompanyModel>> GetListOfCustomer(string customerId)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var results = await collection.FindAsync(x => x.delete == false && x.admin_id == customerId).Result.ToListAsync();

      results = results.OrderByDescending(x => x.create_date).ToList();

      foreach (var item in results)
      {
        item.products ??= new();
      }

      return results;
    }

    /// <summary>
    /// Filter công ty còn được kích hoạt hoặc chưa được xoá
    /// </summary>

    public static async Task<List<CompanyModel>> GetActiveCompany(List<UserModel.Company> companys)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);
      var builder = Builders<CompanyModel>.Filter;
      var companyIds = companys.Select(c => c.id).ToList();

      var filter = builder.In(x => x.id, companyIds) & builder.Eq(x => x.delete, false) & builder.Eq(x => x.status, true);

      var result = await collection.FindAsync(filter).Result.ToListAsync();

      return result;

    }

    public static async Task<bool> ChangeStateSync(string company)
    {
      var collection = _db.GetCollection<CompanyModel>(_collection);

      var builder = Builders<CompanyModel>.Filter;

      // Find one and update
      var filter = builder.Eq(x => x.id, company);
      var update = Builders<CompanyModel>.Update.Set(x => x.is_sync_plan, true);
      var result = await collection.UpdateOneAsync(filter, update);
      return true;

    }
  }
}
