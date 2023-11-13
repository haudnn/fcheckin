using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbMainTransaction
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "transactions";


    public static async Task<TransactionModel> Create(TransactionModel model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<TransactionModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }

    public static async Task<TransactionModel> Update(TransactionModel model)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }

    public static async Task<bool> Delete(string id)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    public static async Task<TransactionModel> Get(string id)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    /// <summary>
    /// Lấy dữ liệu yêu cầu nạp tiền
    /// </summary>
    public static async Task<List<TransactionModel>> GetListRequest(string keyword, int status,
      DateTimeOffset? dateS, DateTimeOffset? dateE)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var builder = Builders<TransactionModel>.Filter;

      var filtered = builder.Eq("type", 1);

      if (status != 0)
        filtered = filtered & builder.Eq("status", status);
      if (dateS != null)
        filtered = filtered & builder.Gte("date", dateS.Value.Ticks);
      if (dateE != null)
        filtered = filtered & builder.Lt("date", dateE.Value.AddDays(1).Ticks);

      var sorted = Builders<TransactionModel>.Sort.Descending("date");

      var list = await collection.FindAsync(filtered).Result.ToListAsync();

      list = list.OrderByDescending(x => x.date).ToList();

      var results = new List<TransactionModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          if (Handled.Shared.SearchKeyword(keyword, item.id + item.customer.name + item.customer.email))
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }


    /// <summary>
    /// Lấy dữ liệu mua sản phẩm
    /// </summary>
    public static async Task<List<TransactionModel>> GetListPurchase(string keyword, string product,
      DateTimeOffset? dateS, DateTimeOffset? dateE)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var builder = Builders<TransactionModel>.Filter;

      var filtered = builder.Eq("type", 3);

      if (!string.IsNullOrEmpty(product))
        filtered = filtered & builder.Eq("product", product);
      if (dateS != null)
        filtered = filtered & builder.Gte("date", dateS.Value.Ticks);
      if (dateE != null)
        filtered = filtered & builder.Lt("date", dateE.Value.AddDays(1).Ticks);

      var sorted = Builders<TransactionModel>.Sort.Descending("date");

      var list = await collection.FindAsync(filtered).Result.ToListAsync();

      list = list.OrderByDescending(x => x.date).ToList();

      var results = new List<TransactionModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          if (Handled.Shared.SearchKeyword(keyword, item.id + item.customer.name + item.customer.email + item.content))
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }


    /// <summary>
    /// Lấy dữ liệu hệ thống nạp tiền
    /// </summary>
    public static async Task<List<TransactionModel>> GetListRecharge(string keyword, DateTimeOffset? dateS,
      DateTimeOffset? dateE)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var builder = Builders<TransactionModel>.Filter;

      var filtered = builder.Eq("type", 2) & builder.Gt("money", 0);

      if (dateS != null)
        filtered = filtered & builder.Gte("date", dateS.Value.Ticks);
      if (dateE != null)
        filtered = filtered & builder.Lt("date", dateE.Value.AddDays(1).Ticks);

      var sorted = Builders<TransactionModel>.Sort.Descending("date");

      var list = await collection.FindAsync(filtered).Result.ToListAsync();

      list = list.OrderByDescending(x => x.date).ToList();

      var results = new List<TransactionModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          if (Handled.Shared.SearchKeyword(keyword, item.id + item.customer.name + item.customer.email))
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }


    /// <summary>
    /// Lấy dữ liệu hệ thống rút tiền
    /// </summary>
    public static async Task<List<TransactionModel>> GetListCashout(string keyword, DateTimeOffset? dateS,
      DateTimeOffset? dateE)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var builder = Builders<TransactionModel>.Filter;

      var filtered = builder.Eq("type", 2) & builder.Lt("money", 0);

      if (dateS != null)
        filtered = filtered & builder.Gte("date", dateS.Value.Ticks);
      if (dateE != null)
        filtered = filtered & builder.Lt("date", dateE.Value.AddDays(1).Ticks);

      var sorted = Builders<TransactionModel>.Sort.Descending("date");

      var list = await collection.FindAsync(filtered).Result.ToListAsync();

      list = list.OrderByDescending(x => x.date).ToList();

      var results = new List<TransactionModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          if (Handled.Shared.SearchKeyword(keyword, item.id + item.customer.name + item.customer.email))
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }


    /// <summary>
    /// Lấy dữ liệu giao dịch của một khách hàng
    /// </summary>
    public static async Task<List<TransactionModel>> GetListOfCustomer(string customerId, int type, int status,
      DateTimeOffset? dateS, DateTimeOffset? dateE)
    {
      var collection = _db.GetCollection<TransactionModel>(_collection);

      var builder = Builders<TransactionModel>.Filter;

      var filtered = builder.Eq("customer.id", customerId);

      if (type != 0)
        filtered = filtered & builder.Eq("type", type);
      if (status != 0)
        filtered = filtered & builder.Eq("status", status);
      if (dateS != null)
        filtered = filtered & builder.Gte("date", dateS.Value.Ticks);
      if (dateE != null)
        filtered = filtered & builder.Lt("date", dateE.Value.AddDays(1).Ticks);

      var sorted = Builders<TransactionModel>.Sort.Descending("date");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      result = result.OrderByDescending(x => x.date).ToList();

      return result;
    }
  }
}
