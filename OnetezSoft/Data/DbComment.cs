using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbComment
  {
    public static async Task<CommentModel> Create(string companyId, string databaseName, CommentModel model)
    {
      model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CommentModel>(databaseName + "_comments");

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<CommentModel> Update(string companyId, string databaseName, CommentModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CommentModel>(databaseName + "_comments");

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string databaseName, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CommentModel>(databaseName + "_comments");

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<CommentModel> Get(string companyId, string databaseName, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CommentModel>(databaseName + "_comments");

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }

    /// <summary>
    /// Danh sách bình luận trong đơn từ
    /// </summary>
    public static async Task<List<CommentModel>> GetList(string companyId, string databaseName, string modelId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<CommentModel>(databaseName + "_comments");

      var results = await collection.FindAsync(x => x.model_id == modelId).Result.ToListAsync();
      return results.OrderBy(x => x.date).ToList();
    }
  }
}
