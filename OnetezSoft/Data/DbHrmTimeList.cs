using DocumentFormat.OpenXml.Bibliography;
using Google.Apis.Testing;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbHrmTimeList
{
  private static string _collection = "hrm_timelist";

  /// <summary>
  /// Hàm tạo mới ca làm cho nhân viên
  /// </summary>
  /// <param name="companyId">ID công ty</param>
  /// <param name="userId">Nhân viên áp dụng</param>
  /// <param name="day">Ngày áp dụng</param>
  /// <param name="shiftsId">Danh sách ca làm áp dụng</param>
  public static async Task<HrmTimeListModel> Create(string companyId, HrmTimeListModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    await collection.InsertOneAsync(model);
    return model;
  }

  /// <summary>
  /// Cập nhật phân ca
  /// </summary>
  /// <param name="companyId">ID công ty</param>
  /// <param name="userId">Nhân viên áp dụng</param>
  /// <param name="data">Dữ liệu phân ca mới</param>
  public static async Task<bool> Update(string companyId, HrmTimeListModel.Shift data, string userId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    var builderFilter = Builders<HrmTimeListModel>.Filter;
    var builder = Builders<HrmTimeListModel>.Update;

    var filter = builderFilter.Eq(x => x.id, userId) & builderFilter.Where(x => x.shifts.Any(y => y.day == data.day));
    var update = builder.Set("shifts.$", data);

    await collection.UpdateOneAsync(filter, update);

    return true;
  }

  /// <summary>
  /// Hàm thêm mới dự	liệu phân ca vào dữ liệu đã có
  /// </summary>
  /// <param name="companyId">ID công ty</param>
  /// <param name="userId">Nhân viên áp dụng</param>
  /// <param name="data">Dữ liệu phân ca mới</param>
  public static async Task<bool> UpdateAdd(string companyId, HrmTimeListModel.Shift data, string userId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    var builderFilter = Builders<HrmTimeListModel>.Filter;
    var builder = Builders<HrmTimeListModel>.Update;

    var filter = builderFilter.Eq(x => x.id, userId);
    var update = builder.AddToSet(x => x.shifts, data);

    await collection.UpdateOneAsync(filter, update);
    return true;
  }

  public static async Task<bool> DeleteDayOff(string companyId, string id, string dayOff, DateTime date, bool isSoftDelete)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);
    var day = date.Ticks;

    var filterBuilder = Builders<HrmTimeListModel>.Filter;
    var updateBuilder = Builders<HrmTimeListModel>.Update;

    var filter = filterBuilder.Eq(x => x.id, id) & filterBuilder.Where(x => x.shifts.Any(y => y.day == day));
    var update = updateBuilder.Set(x => x.shifts[-1].dayoff_id, null as string);
    await collection.UpdateOneAsync(filter, update);

    return true;
  }

  public static async Task<HrmTimeListModel> Get(string companyId, string id)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
  }

  public static async Task<HrmTimeListModel.Shift> GetByDay(string companyId, string id, long day)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    var result = await collection.FindAsync(x => x.id == id && x.shifts.Any(y => y.day == day)).Result.ToListAsync();

    var shift = result.Select(x => x.shifts.FirstOrDefault(y => y.day == day)).ToList();

    return shift.FirstOrDefault();
  }


  public static async Task<List<HrmTimeListModel>> GetList(string companyId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    return await collection.FindAsync(new BsonDocument()).Result.ToListAsync();
  }


  /// <summary>
  /// Lấy danh sách ca làm theo tháng của nhân sự
  /// </summary>
  /// <param name="companyId">id của công ty</param>
  /// <param name="month">Tháng hiện tại</param>
  public static async Task<List<HrmTimeListModel>> GetListByMonth(string companyId, DateTime month)
  {
    month = Convert.ToDateTime(month.ToString("yyyy-MM-01"));

    var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
    var endMonth = month.AddDays(daysInMonth - 1);

    long start = month.Ticks;
    long end = endMonth.Ticks;

    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimeListModel>(_collection);
    var builder = Builders<HrmTimeListModel>.Filter;

    var filtered = builder.ElemMatch(
    x => x.shifts,
    s => s.day >= start && s.day <= end
 );

    var result = await collection.FindAsync(filtered).Result.ToListAsync();
    return result;
  }

  /// <summary>
  /// Hàm xử lý xoá 1 ca làm của nhân viên
  /// </summary>
  /// <param name="companyId">ID công ty</param>
  /// <param name="user_id">ID của user cần xoá</param>
  /// <param name="date">Ngày chứa ca</param>
  /// <param name="shift_id">ID ca làm cần xoá</param>
  public static async Task<bool> DeleteShift(string companyId, string workShiftId)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    var data = await GetList(companyId);

    var updatedData = data.Select(doc =>
    {
      var updatedShifts = doc.shifts.Select(shift =>
              {
                if (shift.shifts_id.Contains(workShiftId) && shift.day > DateTime.Now.Ticks)
                {
                  shift.shifts_id.Remove(workShiftId);
                }
                return shift;
              }).ToList();

      doc.shifts = updatedShifts;
      return doc;
    }).ToList();

    foreach (var updatedDoc in updatedData)
    {
      var filter = Builders<HrmTimeListModel>.Filter.Eq(x => x.id, updatedDoc.id);
      var update = Builders<HrmTimeListModel>.Update.Set(x => x.shifts, updatedDoc.shifts);
      await collection.UpdateOneAsync(filter, update);
    }

    return true;
  }



  /// <summary>
  /// Hàm tạo ngày nghỉ cho nhân sự
  /// </summary>
  /// <param name="companyId">Thông tin công ty</param>
  /// <param name="model">Dữ liệu ngày nghỉ</param>
  /// <param name="usersId">Danh sách nhân sự áp dụng</param>
  public static async Task<bool> HandleDayOff(string companyId, HrmDayOffModel model)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var start = new DateTime(model.start);
    var end = new DateTime(model.end);
    // Lặp lại 1 lần
    if (model.loop == 1)
    {
      for (DateTime date = end; date >= start; date = date.AddDays(-1))
      {
        if (model.salary_users.Count > 0)
        {
          foreach (var user in model.salary_users)
          {
            await HandleData(user, new List<string>(), companyId, date, model.id, true);
          }
        }
        if (model.non_salary_users.Count > 0)
        {
          foreach (var user in model.non_salary_users)
          {
            await HandleData(user, new List<string>(), companyId, date, model.id, true);
          }
        }
      }
    }
    // Lặp lại hằng tuần
    else
    {
      var loopWeek = model.loop_week;
      for (DateTime date = end; date >= start; date = date.AddDays(-1))
      {
        var weekday = (int)date.DayOfWeek == 0 ? 8 : (int)date.DayOfWeek + 1;
        if ((loopWeek.mon && weekday == 2) ||
                    (loopWeek.tue && weekday == 3) ||
                    (loopWeek.wed && weekday == 4) ||
                    (loopWeek.thu && weekday == 5) ||
                    (loopWeek.fri && weekday == 6) ||
                    (loopWeek.sat && weekday == 7) ||
                    (loopWeek.sun && weekday == 8))
        {
          if (model.salary_users.Count > 0)
          {
            foreach (var user in model.salary_users)
            {
              var getDay = await GetByDay(companyId, user, date.Ticks);
              if (getDay != null && getDay.dayoff_id != null)
              {
                var dayOff = await DbHrmDayOff.Get(companyId, getDay.dayoff_id);
                var isHoliday = dayOff.loop == 1;
                if (isHoliday)
                  continue;
              }
              await HandleData(user, new List<string>(), companyId, date, model.id, true);
            }
          }
          if (model.non_salary_users.Count > 0)
          {
            foreach (var user in model.non_salary_users)
            {
              var getDay = await GetByDay(companyId, user, date.Ticks);
              if (getDay != null && getDay.dayoff_id != null)
              {
                var dayOff = await DbHrmDayOff.Get(companyId, getDay.dayoff_id);
                var isHoliday = dayOff.loop == 1;
                if (isHoliday)
                  continue;
              }
              await HandleData(user, new List<string>(), companyId, date, model.id, true);
            }
          }
        }
      }
    }
    return true;
  }

  /// <summary>
  /// Hàm xử lý cập nhật hoặc tạo dữ liệu mới
  /// </summary>
  /// <param name="usersId">List users áp dụng</param>
  /// <param name="dataShifts">Dữ liệu ca làm</param>
  /// <param name="companyId">Id công ty</param>
  /// <param name="date">Ngày áp dụng</param>
  /// <param name="dayOffId">Id ngày nghỉ</param>
  public static async Task<bool> HandleData(string id, List<string> dataShifts, string companyId, DateTime date, string dayOffId = null, bool isOverrided = false)
  {
    var data = await Get(companyId, id);
    if (data != null)
    {
      var dayShift = data.shifts.FirstOrDefault(s => s.day == date.Ticks);
      if (dayShift != null)
      {
        if (Handled.Shared.IsEmpty(dayOffId))
          dayShift.shifts_id = dataShifts;
        if (isOverrided)
          dayShift.dayoff_id = dayOffId;
        else
          dayShift.dayoff_id ??= dayOffId;
        await Update(companyId, dayShift, id);
      }
      else
      {
        var model = new HrmTimeListModel.Shift
        {
          day = date.Ticks,
          shifts_id = dataShifts,
          dayoff_id = dayOffId
        };
        await UpdateAdd(companyId, model, id);
      }
    }
    else
    {
      var shifts = new List<HrmTimeListModel.Shift>
            {
               new HrmTimeListModel.Shift
               {
                  day = date.Ticks,
                  shifts_id = dataShifts,
                  dayoff_id = dayOffId
               }
            };
      var model = new HrmTimeListModel
      {
        id = id,
        shifts = shifts
      };
      await Create(companyId, model);
    }
    return true;
  }

  /// <summary>Danh sách ca làm theo ngày</summary>
  public static async Task<List<HrmTimeListModel.Shift>> GetByRangeDate(string companyId, string userId, long start, long end)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    if (end == new DateTime(end).Date.Ticks)
    {
      var filter = Builders<HrmTimeListModel>.Filter.And(
                            Builders<HrmTimeListModel>.Filter.Eq(x => x.id, userId),
                            Builders<HrmTimeListModel>.Filter.ElemMatch(x => x.shifts, y => y.day >= start && y.day < end));

      var result = await collection.FindAsync(filter).Result.ToListAsync();

      var shifts = result.SelectMany(x => x.shifts.Where(y => y.day >= start && y.day < end)).ToList();

      return shifts;
    }
    else
    {
      var filter = Builders<HrmTimeListModel>.Filter.And(
                   Builders<HrmTimeListModel>.Filter.Eq(x => x.id, userId),
                   Builders<HrmTimeListModel>.Filter.ElemMatch(x => x.shifts, y => y.day >= start && y.day <= end));

      var result = await collection.FindAsync(filter).Result.ToListAsync();

      var shifts = result.SelectMany(x => x.shifts.Where(y => y.day >= start && y.day <= end)).ToList();

      return shifts;
    }
  }

  /// <summary>Danh sách ca làm theo ngày và theo danh sách user</summary>
  public static async Task<List<HrmTimeListModel>> GetByRangeAndUsers(string companyId, List<string> userList, long start, long end)
  {
    start = new DateTime(start).Date.Ticks;
    end = new DateTime(end).Date.Ticks;
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    var builder = Builders<HrmTimeListModel>.Filter;

    var filter = builder.And(builder.In(x => x.id, userList), builder.ElemMatch(x => x.shifts, y => y.day >= start && y.day <= end));

    var result = await collection.FindAsync(filter).Result.ToListAsync();

    return result;
  }
  public static async Task<List<HrmTimeListModel>> GetListByMonth(string companyId, long start, long end)
  {

    var _db = Mongo.DbConnect("fastdo_" + companyId);

    var collection = _db.GetCollection<HrmTimeListModel>(_collection);
    var builder = Builders<HrmTimeListModel>.Filter;

    var filtered = builder.ElemMatch(
    x => x.shifts,
    s => s.day >= start && s.day <= end
 );

    var result = await collection.FindAsync(filtered).Result.ToListAsync();
    return result;
  }



  /// <summary>
  /// Hàm dùng để reset ca làm nhanh
  /// </summary>
  /// <param name="userIds">Danh sách nhân viên áp dụng</param>
  /// <param name="start">Ngày bắt đầu</param>
  /// <param name="end">Ngày kết thúc</param>
  /// <returns></returns>
  public static async Task<bool> Reset(string companyId, List<string> userIds, long start, long end)
  {
    var _db = Mongo.DbConnect("fastdo_" + companyId);
    var collection = _db.GetCollection<HrmTimeListModel>(_collection);

    var timeListUsers = await collection.FindAsync(x => userIds.Contains(x.id)).Result.ToListAsync();

    foreach (var item in timeListUsers)
    {
      foreach (var shift in item.shifts)
      {
        if (shift.day >= start && shift.day <= end)
        {
          if (shift.shifts_id.Any() && shift.shifts_id != null)
          {
            shift.day = shift.day;
            shift.shifts_id = new List<string>();
            shift.dayoff_id = shift.dayoff_id;
          }
          else
            continue;
        }
      }
    }
    // update collection
    foreach (var item in timeListUsers)
    {
      var filter = Builders<HrmTimeListModel>.Filter.Eq(x => x.id, item.id);
      await collection.ReplaceOneAsync(filter, item);
    }
    return true;

  }
}
