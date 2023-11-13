using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbMainNotify
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "notifys";


    public static async Task<NotifyModel> Create(NotifyModel model)
    {
      model.id = Guid.NewGuid().ToString();
      model.date = DateTime.Now.Ticks;

      var collection = _db.GetCollection<NotifyModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<NotifyModel> Update(NotifyModel model)
    {
      var collection = _db.GetCollection<NotifyModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    /// <summary>
    /// Lấy thông báo của một người
    /// </summary>
    public static async Task<List<NotifyModel>> GetList(string user)
    {
      var collection = _db.GetCollection<NotifyModel>(_collection);

      var builder = Builders<NotifyModel>.Filter;

      var filtered = builder.Gt("date", DateTime.Today.AddMonths(-3).Ticks)
        & builder.Eq("user", user);

      var sorted = Builders<NotifyModel>.Sort.Descending("date");

      var result = await collection.FindAsync(filtered).Result.ToListAsync();

      return (from x in result orderby x.date descending select x).ToList();
    }


    /// <summary>
    /// Mẫu thông báo
    /// </summary>
    public static async Task<NotifyModel> Create(int type, string key, string target, string create)
    {
      var creator = await DbMainUser.Get(create, null);
      var creatorName = "<b>" + (creator != null ? creator.FullName : create) + "</b>";
      var name = string.Empty;
      var link = string.Empty;

      #region Các loại thông tạo

      if (type == 101)
      {
        var data = await DbMainTransaction.Get(key);
        name = $"Yêu cầu nạp {string.Format("{0:0,0}", data.money)} VNĐ của bạn đã được phê duyệt.";
        link = "/client/transactions";
      }
      else if (type == 102)
      {
        var data = await DbMainTransaction.Get(key);
        name = $"Yêu cầu nạp {string.Format("{0:0,0}", data.money)} VNĐ của bạn đã bị từ chối.";
        link = "/client/transactions";
      }
      else if (type == 103)
      {
        var data = await DbMainTransaction.Get(key);
        name = $"Hệ thống đã nạp {string.Format("{0:0,0}", data.money)} VNĐ vào tài khoản của bạn.";
        link = "/client/transactions";
      }
      else if (type == 104)
      {
        var data = await DbMainTransaction.Get(key);
        name = $"Hệ thống đã rút {string.Format("{0:0,0}", -data.money)} VNĐ khỏi tài khoản của bạn.";
        link = "/client/transactions";
      }

      #endregion

      if (!string.IsNullOrEmpty(name))
      {
        var model = new NotifyModel();
        model.name = name;
        model.link = link;
        model.user = target;
        model.type = type;
        model.key = key;

        return await Create(model);
      }
      else
        return null;
    }
  }
}