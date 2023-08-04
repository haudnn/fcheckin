using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;

namespace OnetezSoft.Data;

public class DbHrmTimekeeping
{
	private static string _collection = "hrm_timekeepings";

	public static async Task<HrmTimekeepingModel> Create(string companyId, HrmTimekeepingModel model)
	{

		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		await collection.InsertOneAsync(model);

		return model;
	}


	public static async Task<HrmTimekeepingModel> Update(string companyId, HrmTimekeepingModel model)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		var option = new ReplaceOptions { IsUpsert = false };

		var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

		return model;
	}


	public static async Task<bool> Delete(string companyId, string id)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		var result = await collection.DeleteOneAsync(x => x.id == id);

		if (result.DeletedCount > 0)
			return true;
		else
			return false;
	}


	public static async Task<HrmTimekeepingModel> Get(string companyId, string id)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
	}


	public static async Task<HrmTimekeepingModel> Get(string companyId, string userId, long date)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		return await collection.Find(x => x.user == userId && x.date == date).FirstOrDefaultAsync();
	}


	public static async Task<List<HrmTimekeepingModel>> GetList(string companyId, string userId, long dateS, long dateE)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		return await collection.Find(x => x.user == userId
				&& x.date >= dateS && x.date <= dateE
				).ToListAsync();
	}

	/// <summary>
	/// Tìm trong lịch sử chấm công ngày nào checkin nhưng chưa checkout
	/// </summary>
	/// <param name="companyId">ID công ty</param>
	/// <param name="userId">ID user</param>
	/// <returns>Lịch sử chấm công của ngày đó</returns>
	public static async Task<HrmTimekeepingModel> GetUnCheckout(string companyId, string userId)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);
		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		var builder = Builders<HrmTimekeepingModel>.Filter;

		var filter = builder.And(builder.Eq(x => x.user, userId), builder.ElemMatch(x => x.time_tracking,
				i => i.time_type == "Check-out" && i.time_active == null && (i.is_overday == true || i.is_ot == true)));

		var uncheckoutRecord = await collection.Find(filter).FirstOrDefaultAsync();

		var collectionCount = await collection.CountDocumentsAsync("{}");
		if (collectionCount == 0)
			return null;
		return uncheckoutRecord;
	}

	/// <summary>Lấy tất cả dữ liệu chấm công theo range và danh sách user</summary>
	public static async Task<List<HrmTimekeepingModel>> GetByRange(string companyId, long start, long end, List<string> users)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		return await collection.FindAsync(x => users.Contains(x.user) && x.date >= start && x.date <= end).Result.ToListAsync();
	}

	/// <summary>Kiểm tra ca làm vào ngày hôm đó có người checkin chưa</summary>
	public static async Task<bool> CheckTimekeeping(string companyId, long date, string shiftId, bool isOverDay)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		if (!isOverDay)
		{
			var result = await collection.Find(x => x.date == date && x.time_tracking.Any(i => i.time_id == shiftId && i.time_type == "Check-out" && i.time_active == null)).FirstOrDefaultAsync();
			if (result != null)
				return true;
			else
				return false;
		}
		else
		{
			var result = await collection.Find(x => x.time_tracking.Any(i => i.time_id == shiftId && i.time_type == "Check-out" && i.time_active == null && i.is_overday == true)).FirstOrDefaultAsync();
			if (result != null)
				return true;
			else
				return false;
		}
	}

	/// <summary>Thời gian ghi nhận thiết bị chấm công hợp lệ gần nhất</summary>
	
	public static async Task<long> GetNearest(string companyId, string userId)
	{
		var _db = Mongo.DbConnect("fastdo_" + companyId);

		var collection = _db.GetCollection<HrmTimekeepingModel>(_collection);

		var result = await collection.Find(x => x.user == userId && x.time_tracking.Any(i => i.time_active != null && i.is_valid_device)).ToListAsync();
		
		return result.Count > 0 ? result.LastOrDefault().time_tracking.LastOrDefault()?.time_active_tick ?? 0 : 0;

	}


}
