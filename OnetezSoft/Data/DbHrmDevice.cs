using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbHrmDevice
  {
    private static string _collection = "hrm_devices";

    public static async Task<HrmDeviceModel> Create(string companyId, HrmDeviceModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      model.date_request = DateTime.Now.Ticks;
						model.status	= 1;
      

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDeviceModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<HrmDeviceModel> Update(string companyId, HrmDeviceModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDeviceModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDeviceModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<HrmDeviceModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmDeviceModel>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }


			public static async Task<List<HrmDeviceModel>> GetList(string companyId)
			{
					var _db = Mongo.DbConnect("fastdo_" + companyId);

					var collection = _db.GetCollection<HrmDeviceModel>(_collection);

					var results = await collection.Find(new BsonDocument()).ToListAsync();

					return results.OrderByDescending(x => x.date_request).ToList();
			}

			///  Kiểm tra user đang có thiết bị nào đang chờ duyệt không
			public static async Task<bool> CheckPending(string companyId, string userId)
			{
					var _db = Mongo.DbConnect("fastdo_" + companyId);

					var collection = _db.GetCollection<HrmDeviceModel>(_collection);

					var result = await collection.Find(x => x.user_request == userId && x.status == 1).FirstOrDefaultAsync();
						 
     return result != null ? true : false;
			}

    #region Dữ liệu cố định

    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

						list.Add(new StaticModel
      {
        id = 4,
        name = "Tất cả",
        color = "is-danger",
								isDefault = true
      });
      list.Add(new StaticModel
      {
        id = 1,
        name = "Đang chờ",
        color = "is-warning",
								icon = "remove"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Đã duyệt",
        color = "is-success",
								icon = "done"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Từ chối",
        color = "is-danger",
								icon = "close"
      });



      return list;
    }

				public static StaticModel Status(int id)
    {
      var query = from s in Status()
                  where s.id == id
                  select s;
      if (query.Count() > 0)
        return query.FirstOrDefault();
      else
        return new StaticModel();
    }
    #endregion
  }
}
