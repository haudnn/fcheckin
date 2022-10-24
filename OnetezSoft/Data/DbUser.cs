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
        return true;
      else
        return false;
    }


    public static async Task<UserModel> Update(string companyId, UserModel model)
    {
      if (model.title == 0)
        model.title = 3;
      if (model.role == 0)
        model.role = 3;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<UserModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var result = await collection.Find(x => x.id == id && !x.delete).FirstOrDefaultAsync();

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
        if(result.plans_pin == null)
          result.plans_pin = new();
        if(result.plans_hide == null)
          result.plans_hide = new();
      }

      return result;
    }


    public static async Task<UserModel> GetDelete(string companyId, string id, string email)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      if(!string.IsNullOrEmpty(id))
        return await collection.Find(x => x.delete && x.id == id).FirstOrDefaultAsync();
      else if (!string.IsNullOrEmpty(email))
        return await collection.Find(x => x.delete && x.email == email.Trim()).FirstOrDefaultAsync();
      return null;
    }


    public static List<UserModel> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var results = collection.Find(x => !x.delete && x.active).ToList();
      
      foreach (var item in results)
      {
        if(item.products == null)
          item.products = new();
      }

      return results;
    }


    public static async Task<List<UserModel>> GetList(string companyId, string keyword, string department, int status)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      var builder = Builders<UserModel>.Filter;

      var filtered = builder.Eq("delete", false);

      if (!string.IsNullOrEmpty(department))
        filtered = filtered & builder.Eq("departments_id", department);
      if (status == 1) // Hoạt động
        filtered = filtered & builder.Eq("active", true);
      else if (status == 2) // Khóa
        filtered = filtered & builder.Eq("active", false);

      var sorted = Builders<UserModel>.Sort.Ascending("role");

      var list = await collection.Find(filtered).Sort(sorted).ToListAsync();

      var results = new List<UserModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          bool check = Handled.Shared.SearchKeyword(keyword, item.email + item.first_name + item.last_name);

          if (check)
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }


    /// <summary>
    /// Lấy danh sách Admin và QLHT
    /// </summary>
    public static async Task<List<UserModel>> GetManager(string companyId, bool onlyAdmin)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<UserModel>(_collection);

      if (onlyAdmin)
        return await collection.Find(x => !x.delete && x.role == 1).ToListAsync();
      else
        return await collection.Find(x => !x.delete && x.role >= 1 && x.role <=2).ToListAsync();
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
