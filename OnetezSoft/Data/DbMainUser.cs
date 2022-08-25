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
  public class DbMainUser
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "users";

    public static async Task<UserModel> Create(UserModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      if (string.IsNullOrEmpty(model.avatar))
        model.avatar = $"https://avatars.dicebear.com/api/micah/{model.id}.svg?background=grey";

      model.email = model.email.Trim().ToLower();
      model.password = string.IsNullOrEmpty(model.password) ? 
        Handled.Shared.CreateMD5(model.id) : Handled.Shared.CreateMD5(model.password);
      model.create_date = DateTime.Now.Ticks;
      model.active = true;
      model.delete = false;

      var collection = _db.GetCollection<UserModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<bool> Delete(string id)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<UserModel> Update(UserModel model)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      // Cập nhật tất cả công ty
      if (model.companys != null)
      {
        foreach (var item in model.companys)
        {
          var user = await DbUser.Get(item.id, model.id);
          if(user != null)
          {
            user.is_admin = model.is_admin;
            user.is_customer = model.is_customer;
            user.password = model.password;
            user.session = model.session;
            user.companys = model.companys;
            user.online = model.online;
            user.avatar = model.avatar;
            user.balance = model.balance;
            //user.phone = model.phone;
            //user.first_name = model.first_name;
            //user.last_name = model.last_name;
            await DbUser.Update(item.id, user);
          }
        }
      }


      return model;
    }


    public static async Task<UserModel> Get(string id)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      return await collection.Find(x => x.id == id && !x.delete).FirstOrDefaultAsync();
    }


    public static async Task<UserModel> GetbyEmail(string email)
    {
      if(string.IsNullOrEmpty(email))
        return null;
      else
         email = email.Trim().ToLower();

      var collection = _db.GetCollection<UserModel>(_collection);

      return await collection.Find(x => !x.delete && x.email == email.Trim()).FirstOrDefaultAsync();
    }


    public static async Task<UserModel> GetbySession(string session)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      return await collection.Find(x => !x.delete && x.session == session).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Kiển tra email đã tồn tại chưa
    /// </summary>
    public static bool CheckEmail(string email)
    {
      if (string.IsNullOrEmpty(email))
        return false;
      else
        email = email.Trim().ToLower();

      var collection = _db.GetCollection<UserModel>(_collection);

      var result = collection.Find(x => !x.delete && x.email == email).FirstOrDefault();

      return result != null;
    }

    /// <summary>
    /// Hàm đăng nhập
    /// </summary>
    public static async Task<UserModel> Login(string username, string pass)
    {
      var user = await GetbyEmail(username);
      if (user != null)
      {
        var password = Handled.Shared.CreateMD5(pass);

        if (user.password == password)
          return user;
      }

      return null;
    }


    public static async Task<List<UserModel>> GetAll()
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var results = await collection.Find(x => x.delete == false).ToListAsync();

      return results;
    }


    public static async Task<List<UserModel>> GetListAdmin()
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var results = await collection.Find(x => !x.delete && x.is_admin).ToListAsync();

      return results.OrderBy(x => x.create_date).ToList();
    }


    public static List<UserModel> GetListCustomer()
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var results = collection.Find(x => !x.delete && x.active && x.is_customer).ToList();

      return results.OrderBy(x => x.create_date).ToList();
    }


    public static List<UserModel> GetListCustomer(string keyword, int status, int paging, int size, out int total,
      bool sortDesc, string sortBy)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var builder = Builders<UserModel>.Filter;

      var filtered = builder.Eq("delete", false) & builder.Eq("is_customer", true);

      if (status == 1) // Hoạt động
        filtered = filtered & builder.Eq("active", true);
      else if (status == 2) // Khóa
        filtered = filtered & builder.Eq("active", false);

      var sorted = Builders<UserModel>.Sort.Descending("first_name");

      var list = collection.Find(filtered).Sort(sorted).ToList();

      var results = new List<UserModel>();
      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          if(Handled.Shared.SearchKeyword(keyword, item.email + item.first_name + item.last_name))
            results.Add(item);
        }
      }
      else
        results = list;

      // Sắp xếp
      if (sortBy == "first_name")
        results = sortDesc ? results.OrderByDescending(x => x.first_name).ToList()
          : results.OrderBy(x => x.first_name).ToList();
      else if (sortBy == "online")
        results = sortDesc ? results.OrderByDescending(x => x.online).ToList()
          : results.OrderBy(x => x.online).ToList();

      total = results.Count;
      if (size > 0)
        return results.Skip(size * (paging - 1)).Take(size).ToList();
      else
        return results;
    }


    public static List<UserModel> GetList(string keyword, string company, int status, int paging, int size, out int total)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var builder = Builders<UserModel>.Filter;

      var filtered = builder.Eq("delete", false);

      if (status == 1) // Hoạt động
        filtered = filtered & builder.Eq("active", true);
      else if (status == 2) // Khóa
        filtered = filtered & builder.Eq("active", false);

      var sorted = Builders<UserModel>.Sort.Descending("create_date");

      var list = collection.Find(filtered).Sort(sorted).ToList();

      var results = new List<UserModel>();

      if (!string.IsNullOrEmpty(keyword) || !string.IsNullOrEmpty(company))
      {
        foreach (var item in list)
        {
          bool check = true;

          if(!string.IsNullOrEmpty(company) && !item.companys.Select(x => x.id).Contains(company))
            check = false;

          if(!Handled.Shared.SearchKeyword(keyword, item.email + item.first_name + item.last_name))
            check = false;

          if (check)
            results.Add(item);
        }
      }
      else
        results = list;

      total = results.Count;

      if (size > 0)
        return results.Skip(size * (paging - 1)).Take(size).ToList();
      else
        return results;
    }
  }
}
