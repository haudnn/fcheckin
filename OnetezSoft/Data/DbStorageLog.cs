using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbStorageLog
{
  private static string _collection = "storage_logs";

  public static async Task<FileModel> Create(string companyId, FileModel model)
  {
    model.id = Mongo.RandomId();
    if (model.date == 0)
      model.date = DateTime.Now.Ticks;

    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<FileModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }
  public static async Task<FileModel> Update(string companyId, FileModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<FileModel>(_collection);

    await collection.FindOneAndReplaceAsync(x => x.id == model.id, model);

    return model;
  }

  public static async Task<List<FileModel>> CreateMany(string companyId, List<FileModel> list)
  {
    foreach (var item in list)
    {
      item.id = Mongo.RandomId();
      if(item.date == 0)
        item.date = DateTime.Now.Ticks;

      item.is_delete = false;
      item.date_delete = 0;
    }

    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<FileModel>(_collection);

    await collection.InsertManyAsync(list);

    return list;
  }


  public static async Task<bool> Delete(string companyId, string link, string fullname, string avatar)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<FileModel>(_collection);

    var filter = Builders<FileModel>.Filter.Eq(x => x.link, link);
    var update = Builders<FileModel>.Update.Set(x => x.date_delete, DateTime.Now.Ticks)
                                           .Set(x => x.author_avatar, avatar)
                                           .Set(x => x.is_delete, true)
                                           .Set(x => x.author_name, fullname);

    var result = await collection.FindOneAndUpdateAsync(filter, update);
    if (result != null)
      return true;
    else
      return false;
  }

  /// <summary>
  /// Danh sách dữ liệu
  /// </summary>
  public static async Task<List<FileModel>> GetList(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<FileModel>(_collection);

    var results = await collection.FindAsync(x => true).Result.ToListAsync();

    return results.OrderByDescending(x => x.date).ToList();
  }
}
