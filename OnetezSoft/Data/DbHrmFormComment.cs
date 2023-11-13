using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbHrmFormComment
  {
    private static string _collection = "hrm_form_comments";

    public static async Task<HrmFormModel.Comment> Create(string companyId, HrmFormModel.Comment model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmFormModel.Comment>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<HrmFormModel.Comment> Update(string companyId, HrmFormModel.Comment model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmFormModel.Comment>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmFormModel.Comment>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<HrmFormModel.Comment> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmFormModel.Comment>(_collection);

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách bình luận trong đơn từ
    /// </summary>
    public static async Task<List<HrmFormModel.Comment>> GetList(string companyId, string formId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmFormModel.Comment>(_collection);

      var results = await collection.FindAsync(x => x.form_id == formId).Result.ToListAsync();
      return results.OrderBy(x => x.date).ToList();
    }
  }
}
