using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data;
public class DbHrmForm
{
  private static string _collection = "hrm_form";

  public static async Task<HrmFormModel> Create(string companyId, HrmFormModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    if (string.IsNullOrEmpty(model.id))
      model.id = Mongo.RandomId();
    model.created = DateTime.Now.Ticks;
    model.users = model.confirm_users.Select(i => i.user).ToList();

    var collection = _db.GetCollection<HrmFormModel>(_collection);

    await collection.InsertOneAsync(model);

    return model;
  }

  public static async Task<HrmFormModel> Update(string companyId, HrmFormModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);

    var option = new ReplaceOptions { IsUpsert = false };

    model.users = model.confirm_users.Select(i => i.user).ToList();
    if (model.confirm_users.All(i => i.status == 2) && !model.is_confirm && model.confirm_date == 0)
    {
      model.is_confirm = true;
      model.confirm_date = DateTime.Now.Ticks;
    }

    var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

    return model;
  }

  public static async Task<bool> Delete(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);

    var result = await collection.DeleteOneAsync(x => x.id == id);

    if (result.DeletedCount > 0)
      return true;
    else
      return false;
  }

  public static async Task<HrmFormModel> Get(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);

    return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
  }

  public static async Task<List<HrmFormModel>> GetList(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);

    var results = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

    return results.OrderByDescending(x => x.created).ToList();
  }

  /// <summary>Danh sách đơn từ do mình tạo và đơn từ được mình phê duyệt</summary>
  public static async Task<List<HrmFormModel>> GetListByUser(string companyId, string userId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);
    var result = await collection.FindAsync(x => x.user == userId
                                            || x.users.Contains(userId))
                                 .Result.ToListAsync();
    return result.OrderByDescending(x => x.created).ToList();
  }

  /// <summary>Danh sách đơn từ cấp dưới của mình</summary>
  public static async Task<List<HrmFormModel>> GetListByUsers(string companyId, List<string> users, string userId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);
    var result = await collection.FindAsync(x => users.Contains(x.user) || x.user == userId || x.users.Contains(userId))
                                 .Result.ToListAsync();
    return result.OrderByDescending(x => x.created).ToList();
  }

  /// <summary>Danh sách đơn từ hệ thống trừ mình</summary>
  public static async Task<List<HrmFormModel>> GetListAdmin(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);
    var result = await collection.FindAsync(x => true)
                                 .Result.ToListAsync();
    return result.OrderByDescending(x => x.created).ToList();
  }

  /// <summary>Danh sách đơn từ do mình tạo và đơn từ đã được mình phê duyệt</summary>
  public static async Task<List<HrmFormModel>> GetListConfirmByUsers(string companyId, List<string> users)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);
    var result = await collection.FindAsync(x => users.Contains(x.user) && x.is_confirm)
                                 .Result.ToListAsync();
    return result.OrderByDescending(x => x.created).ToList();
  }

  /// <summary>Danh sách đơn từ chưa được nạp</summary>
  public static async Task<List<HrmFormModel>> GetListByNotLoad(string companyId, List<string> userList, long start, long end)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmFormModel>(_collection);
    var result = await collection.FindAsync(x => x.is_confirm && userList.Contains(x.user) && x.work_date_shifts.Any(i => i.loaded)
                                            && x.work_date_shifts.Any(x => (x.start >= start && x.start <= end) || (x.start <= start && x.end >= start)))
                                 .Result.ToListAsync();
    return result.OrderByDescending(x => x.created).ToList();
  }

  #region dữ liệu mặc định
  /// <summary>Trạng thái xác nhận đơn từ danh sách</summary>
  public static List<StaticModel> GetConfirmStatus()
  {
    var list = new List<StaticModel>();

    list.Add(new StaticModel
    {
      id = 1,
      name = "Chờ duyệt",
      icon = "more_horiz",
      color = "has-text-dark"
    });

    list.Add(new StaticModel
    {
      id = 2,
      name = "Phê duyệt",
      icon = "check",
      color = "has-text-success",
    });

    list.Add(new StaticModel
    {
      id = 3,
      name = "Từ chối",
      icon = "cancel",
      color = "has-text-danger",
    });

    list.Add(new StaticModel
    {
      id = 4,
      name = "Bị huỷ",
      icon = "block",
      color = "has-text-dark",
    });

    return list;
  }

  /// <summary>Trạng thái xác nhận đơn từ danh sách</summary>
  public static StaticModel GetConfirmStatusDetail(int id)
  {
    var result = GetConfirmStatus().Find(i => i.id == id);
    if (result != null)
      return GetConfirmStatus().FirstOrDefault(i => i.id == id);
    return new StaticModel() { name = "Tất cả trạng thái" };
  }

  #endregion
}
