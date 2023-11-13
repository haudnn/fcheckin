using MongoDB.Driver;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbUser
  {
    private static string _collection = "users";

    public static async Task<UserModel> Create(string companyId, UserModel model)
    {
      if (model.title == 0)
        model.title = 3;
      if (model.role == 0)
        model.role = 3;
      if (model.companys == null)
        model.companys = new();
      if (model.role_manage == null)
        model.role_manage = new();
      if (model.departments_id == null)
        model.departments_id = new();
      if (model.teams_id == null)
        model.teams_id = new();
      if (model.products == null)
        model.products = new();

      model.email = model.email.Trim().ToLower();
      model.star_total = 0;
      model.star_receive = 0;
      model.star_distribute = 0;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
      {
        return true;
      }
      else
        return false;
    }


    public static async Task<UserModel> Update(string companyId, UserModel model, GlobalService globalService)
    {
      if (model.title == 0)
        model.title = 3;
      if (model.role == 0)
        model.role = 3;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      if (globalService != null)
      {
        var shareStorage = await globalService.GetShareStorage(companyId);

        shareStorage.UserList.RemoveAll(x => x.id == model.id);
        shareStorage.MemberList.RemoveAll(x => x.id == model.id);

        shareStorage.UserList.Add(model);
        shareStorage.MemberList.Add(UserService.ConvertToMember(model));
      }

      return model;
    }


    /// <summary>
    /// Lấy thông tin tài khoản, không lấy tài khoản đã xóa
    /// </summary>
    public static async Task<UserModel> Get(string companyId, string id, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetShareStorage(companyId);

      var result = globalService == null ? null : shareStorage.UserList.FirstOrDefault(x => x.id == id);

      if (result == null)
      {
        var _db = Mongo.DbConnect("fastdo_" + companyId);

        var collection = _db.GetCollection<UserModel>(_collection);

        result = await collection.FindAsync(x => x.id == id && !x.delete).Result.FirstOrDefaultAsync();

        if (result != null)
        {
          if (result.companys == null)
            result.companys = new();
          if (result.role_manage == null)
            result.role_manage = new();
          if (result.departments_id == null)
            result.departments_id = new();
          if (result.teams_id == null)
            result.teams_id = new();
          if (result.products == null)
            result.products = new();
          if (result.plans_pin == null)
            result.plans_pin = new();
          if (result.plans_hide == null)
            result.plans_hide = new();
        }
      }

      return result;
    }


    /// <summary>
    /// Lấy thông tin tài khoản, kể cả tài khoản đã xóa
    /// </summary>
    public static async Task<UserModel> GetDelete(string companyId, string userId, string email)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      if (!string.IsNullOrEmpty(userId))
        return await collection.FindAsync(x => x.id == userId).Result.FirstOrDefaultAsync();
      else if (!string.IsNullOrEmpty(email))
        return await collection.FindAsync(x => x.email == email.Trim()).Result.FirstOrDefaultAsync();
      return null;
    }


    public static async Task<List<UserModel>> GetAll(string companyId, GlobalService globalService)
    {
      var results = new List<UserModel>();
      if (globalService != null && await globalService.CheckIfExist(companyId) && globalService != null)
      {
        var shareStorage = await globalService.GetShareStorage(companyId);
        results = shareStorage.UserList.Where(x => !x.delete && x.active).ToList();
      }

      if (results == null || results.Count == 0)
      {
        var _db = Mongo.DbConnect("fastdo_" + companyId);

        var collection = _db.GetCollection<UserModel>(_collection);

        results = collection.Find(x => !x.delete && x.active).ToList();
      }

      return (from x in results orderby x.title, x.role select x).ToList();
    }


    public static async Task<List<UserModel>> GetAll(string companyId, string department, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetShareStorage(companyId);

      var results = globalService == null ? new() : shareStorage.UserList.Where(x => !x.delete && x.active && x.departments_id.Contains(department)).ToList();

      if (results == null || results.Count == 0)
      {
        var _db = Mongo.DbConnect("fastdo_" + companyId);

        var collection = _db.GetCollection<UserModel>(_collection);

        results = await collection.FindAsync(x => !x.delete && x.active
        && x.departments_id.Contains(department)).Result.ToListAsync();
      }

      return results;
    }

    /// <summary>Lấy danh sách thông tin tài khoản, kể cả tài khoản đã xóa</summary>
    public static async Task<List<UserModel>> GetAllWithinDelete(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var results = await collection.FindAsync(x => true).Result.ToListAsync();

      return (from x in results orderby x.title, x.role select x).ToList();
    }


    public static async Task<List<UserModel>> GetList(string companyId, string keyword, string department, int status, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetShareStorage(companyId);

      var results = globalService == null ? new() : shareStorage.UserList;

      if (results == null || results.Count == 0)
      {
        var _db = Mongo.DbConnect("fastdo_" + companyId);

        var collection = _db.GetCollection<UserModel>(_collection);

        results = await collection.FindAsync(x => !x.delete).Result.ToListAsync();
      }

      if (!string.IsNullOrEmpty(department))
        results = results.Where(x => x.departments_id.Contains(department)).ToList();
      if (status == 1) // Hoạt động
        results = results.Where(x => x.active).ToList();
      else if (status == 2) // Khóa
        results = results.Where(x => !x.active).ToList();

      results = results.OrderBy(x => x.role).ToList();

      var result = new List<UserModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in results)
        {
          bool check = Handled.Shared.SearchKeyword(keyword, item.email + item.first_name + item.last_name);

          if (check)
            results.Add(item);
        }
      }
      else
        result = results;

      return result;
    }


    /// <summary>
    /// Lấy danh sách Admin và QLHT
    /// </summary>
    public static async Task<List<UserModel>> GetManager(string companyId, bool onlyAdmin, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetShareStorage(companyId);

      var results = globalService == null ? new() : shareStorage.UserList.Where(x => !x.delete && (onlyAdmin ? x.role == 1 : (x.role >= 1 && x.role <= 2))).ToList();

      if (results == null || results.Count == 0)
      {
        var _db = Mongo.DbConnect("fastdo_" + companyId);

        var collection = _db.GetCollection<UserModel>(_collection);

        if (onlyAdmin)
          return await collection.FindAsync(x => !x.delete && x.role == 1).Result.ToListAsync();
        else
          return await collection.FindAsync(x => !x.delete && x.role >= 1 && x.role <= 2).Result.ToListAsync();
      }

      return results;
    }


    /// <summary>
    /// Lần lần online gần nhất của người dùng trong tổ chức
    /// </summary>
    public static async Task<long> GetOnline(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var builder = Builders<UserModel>.Filter;

      var filtered = builder.Eq("delete", false);

      var sorted = Builders<UserModel>.Sort.Descending("online");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      result = result.OrderByDescending(x => x.online).ToList();

      if (result.Count > 0)
        return result.FirstOrDefault().online;
      else
        return 0;
    }

    /// <summary>
    /// Lấy thông tin thiết bị của người dùng
    /// </summary>
    public static async Task<bool> GetDevice(string companyId, string userId, string deviceId, GlobalService globalService)
    {
      var shareStorage = globalService == null ? null : await globalService.GetShareStorage(companyId);

      var result = globalService == null ? null : shareStorage.UserList.FirstOrDefault(x => x.id == userId && x.device_id == deviceId);

      if (result == null)
      {
        var _db = Mongo.DbConnect("fastdo_" + companyId);

        var collection = _db.GetCollection<UserModel>(_collection);

        result = await collection.FindAsync(x => x.id == userId && x.device_id == deviceId).Result.FirstOrDefaultAsync();
      };


      return result != null ? true : false;
    }


    #region Dữ liệu cố định

    public static List<StaticModel> Role()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Admin",
        color = "is-danger",
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "QLHT",
        color = "is-warning",
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Người dùng",
        color = "",
      });

      return list;
    }

    // Phân loại: chi tiết
    public static StaticModel Role(int id)
    {
      var query = from s in Role()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      return new StaticModel();
    }

    #endregion
  }
}
