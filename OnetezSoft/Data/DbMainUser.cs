using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
      model.password = string.IsNullOrEmpty(model.password) ? Handled.Shared.CreateMD5(model.id) : Handled.Shared.CreateMD5(model.password);
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


    public static async Task<UserModel> Update(UserModel model, GlobalService globalService, bool updateAll = true)
    {
      model.email = model.email.Trim().ToLower();

      if (model.companys == null)
        model.companys = new();

      var collection = _db.GetCollection<UserModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      if (updateAll)
      {
        new Task(async () =>
        {
          // Cập nhật tất cả công ty
          foreach (var item in model.companys)
          {
            var user = await DbUser.Get(item.id, model.id, globalService);
            if (user != null)
            {
              user.email = model.email;
              user.password = model.password;
              user.companys = model.companys;
              user.session = model.session;
              user.balance = model.balance;
              user.online = model.online;
              user.avatar = model.avatar;
              user.is_admin = model.is_admin;
              user.is_customer = model.is_customer;
              await DbUser.Update(item.id, user, globalService);
            }
          }
        }).Start();
      }
      else
      {
        if (globalService != null)
        {
          var userList = await globalService.GetAllUserList();
          var memberList = await globalService.GetAllMemberList();

          var updateUser = userList.FirstOrDefault(x => x.Value.Any(y => y.id == model.id));
          var updateMember = memberList.FirstOrDefault(x => x.Value.Any(y => y.id == model.id));

          updateUser.Value.RemoveAll(x => x.id == model.id);
          updateMember.Value.RemoveAll(x => x.id == model.id);
          updateUser.Value.Add(model);
          updateMember.Value.Add(UserService.ConvertToMember(model));
        }
      }

      return model;
    }

    public static async Task<UserModel> UpdateSession(UserModel model, GlobalService globalService)
    {
      model.email = model.email.Trim().ToLower();

      if (model.companys == null)
        model.companys = new();

      var collection = _db.GetCollection<UserModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      if (globalService != null)
      {
        var userList = await globalService.GetAllUserList();
        var memberList = await globalService.GetAllMemberList();

        var updateUser = userList.FirstOrDefault(x => x.Value.Any(y => y.id == model.id));
        var updateMember = memberList.FirstOrDefault(x => x.Value.Any(y => y.id == model.id));

        updateUser.Value.RemoveAll(x => x.id == model.id);
        updateMember.Value.RemoveAll(x => x.id == model.id);
        updateUser.Value.Add(model);
        updateMember.Value.Add(UserService.ConvertToMember(model));
      }

      return model;
    }

    public static async Task<UserModel> UpdateOnline(string id)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var filter = Builders<UserModel>.Filter.Eq(x => x.id, id);

      var update = Builders<UserModel>.Update.Set(x => x.online, DateTime.Now.Ticks);

      var result = await collection.FindOneAndUpdateAsync(filter, update);

      if (result != null)
        return result;
      else
        return null;
    }

    public static async Task<UserModel> Get(string id, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetAllUserList();
      var kpv = globalService == null ? new() : shareStorage.FirstOrDefault(x => x.Value.FirstOrDefault(x => x.id == id) != null);

      var result = new UserModel();

      if (kpv.Value != null && kpv.Value.Count > 0)
      {
        result = kpv.Value.FirstOrDefault(x => x.id == id);
      }
      if (result == null || result.id.IsEmpty())
      {
        var collection = _db.GetCollection<UserModel>(_collection);

        result = await collection.FindAsync(x => x.id == id && !x.delete).Result.FirstOrDefaultAsync();
      }

      return result;
    }

    public static async Task<UserModel> GetbySession(string session, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetAllUserList();

      // Lưu mật khẩu trong 1 tháng
      var sessionEncryp = Handled.Shared.CreateMD5(session + DateTime.Now.Year + DateTime.Now.Month);

      // Từ ngày 1/12/2023 đổi cách login
      var expireDate = new DateTime(2023, 12, 1, 0, 0, 0);
      if (DateTime.Now.Ticks > expireDate.Ticks)
      {
        sessionEncryp = Handled.Shared.CreateMD5(session);
      }

      var kpv = globalService == null ? new() : shareStorage.FirstOrDefault(x => x.Value.FirstOrDefault(x => x.session == sessionEncryp) != null);
      var result = new UserModel();

      if (kpv.Value != null && kpv.Value.Count > 0)
      {
        result = kpv.Value.FirstOrDefault(x => x.session == sessionEncryp);
      }
      if (result == null || result.id.IsEmpty())
      {
        var collection = _db.GetCollection<UserModel>(_collection);
        result = await collection.FindAsync(x => !x.delete && x.session == sessionEncryp).Result.FirstOrDefaultAsync();
      }

      if (result != null && result.last_login > 0)
      {
        if (new TimeSpan(DateTime.Today.Ticks - result.last_login).TotalDays > 60)
        {
          return null;
        }
      }

      return result;
    }

    public static async Task<UserModel> GetbyEmail(string email, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetAllUserList();

      if (string.IsNullOrEmpty(email))
        return null;
      else
        email = email.Trim().ToLower();

      var kpv = globalService == null ? new() : shareStorage.FirstOrDefault(x => x.Value.FirstOrDefault(x => x.email == email) != null);

      var result = new UserModel();

      if (kpv.Value != null && kpv.Value.Count > 0)
      {
        result = kpv.Value.FirstOrDefault(x => x.email == email);
      }
      if (result == null || result.id.IsEmpty())
      {
        var collection = _db.GetCollection<UserModel>(_collection);
        result = await collection.FindAsync(x => !x.delete && x.email.ToLower() == email).Result.FirstOrDefaultAsync();
      }

      return result;
    }

    /// <summary>
    /// Kiển tra email đã tồn tại chưa
    /// </summary>
    public static async Task<bool> CheckEmail(string email)
    {
      if (string.IsNullOrEmpty(email))
        return false;
      else
        email = email.Trim().ToLower();

      var collection = _db.GetCollection<UserModel>(_collection);

      var result = await collection.FindAsync(x => !x.delete && x.email.ToLower() == email).Result.FirstOrDefaultAsync();

      return result != null;
    }

    /// <summary>
    /// Hàm đăng nhập
    /// </summary>
    public static async Task<UserModel> Login(string username, string pass, GlobalService globalService)
    {
      var user = await GetbyEmail(username, globalService);

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

      var results = await collection.FindAsync(x => x.delete == false).Result.ToListAsync();

      return results;
    }

    public static async Task<List<UserModel>> GetAll(string companyId)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var results = await collection.FindAsync(x => x.delete == false
                  && x.companys.FirstOrDefault(y => y.id == companyId) != null).Result.ToListAsync();

      return results;
    }

    public static List<UserModel> GetAll(string keyword, int orderby, int paging, int size, out int total)
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var sorted = Builders<UserModel>.Sort.Descending("create_date");

      var list = collection.Find(new BsonDocument()).Sort(sorted).ToList();

      var results = new List<UserModel>();
      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          if (Handled.Shared.SearchKeyword(keyword, item.id + item.email + item.FullName))
            results.Add(item);
          else
          {
            var company = item.companys != null ? item.companys.Select(x => x.name).ToList() : new();
            if (Handled.Shared.SearchKeyword(keyword, string.Join(", ", company)))
              results.Add(item);
          }
        }
      }
      else
        results = list;

      if (orderby == 2)
        results = results.OrderByDescending(x => x.online).ToList();
      else if (orderby == 3)
        results = results.OrderBy(x => x.email).ToList();
      else if (orderby == 4)
        results = results.OrderBy(x => x.companys.Count).ToList();

      total = results.Count;
      if (size > 0)
        return results.Skip(size * (paging - 1)).Take(size).ToList();
      else
        return results;
    }


    public static async Task<List<UserModel>> GetListAdmin()
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var results = await collection.FindAsync(x => !x.delete && x.is_admin).Result.ToListAsync();

      return results.OrderBy(x => x.create_date).ToList();
    }


    public static async Task<List<UserModel>> GetListCustomer()
    {
      var collection = _db.GetCollection<UserModel>(_collection);

      var results = await collection.FindAsync(x => !x.delete && x.active && x.is_customer).Result.ToListAsync();

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
          if (Handled.Shared.SearchKeyword(keyword, item.email + item.first_name + item.last_name))
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

          if (!string.IsNullOrEmpty(company) && !item.companys.Select(x => x.id).Contains(company))
            check = false;

          if (!Handled.Shared.SearchKeyword(keyword, item.email + item.first_name + item.last_name))
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