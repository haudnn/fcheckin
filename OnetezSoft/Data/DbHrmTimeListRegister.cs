using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data;

public class DbHrmTimeListRegister
{
   private static string _collection = "hrm_timelist_register";

   /// <summary>
   /// Hàm tạo mới ca làm cho nhân viên
   /// </summary>
   /// <param name="companyId">ID công ty</param>
   /// <param name="userId">Nhân viên áp dụng</param>
   /// <param name="day">Ngày áp dụng</param>
   /// <param name="shiftsId">Danh sách ca làm áp dụng</param>
   public static async Task<HrmTimeListRegisterModel> Create(string companyId, HrmTimeListRegisterModel model)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      await collection.InsertOneAsync(model);
      return model;
   }

   /// <summary>
   /// Cập nhật phân ca
   /// </summary>
   /// <param name="companyId">ID công ty</param>
   /// <param name="userId">Nhân viên áp dụng</param>
   /// <param name="data">Dữ liệu phân ca mới</param>
   public static async Task<bool> Update(string companyId, HrmTimeListRegisterModel.ShiftRegister data, string userId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      var builderFilter = Builders<HrmTimeListRegisterModel>.Filter;
      var builder = Builders<HrmTimeListRegisterModel>.Update;

      var filter = builderFilter.Eq(x => x.id, userId) & builderFilter.Where(x => x.shifts_register.Any(y => y.day == data.day));
      var update = builder.Set("shifts_register.$", data);

      await collection.UpdateOneAsync(filter, update);

      return true;
   }

   /// <summary>
   /// Hàm thêm mới dự	liệu phân ca vào dữ liệu đã có
   /// </summary>
   /// <param name="companyId">ID công ty</param>
   /// <param name="userId">Nhân viên áp dụng</param>
   /// <param name="data">Dữ liệu phân ca mới</param>
   public static async Task<bool> UpdateAdd(string companyId, HrmTimeListRegisterModel.ShiftRegister data, string userId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      var builderFilter = Builders<HrmTimeListRegisterModel>.Filter;
      var builder = Builders<HrmTimeListRegisterModel>.Update;

      var filter = builderFilter.Eq(x => x.id, userId);
      var update = builder.AddToSet(x => x.shifts_register, data);

      await collection.UpdateOneAsync(filter, update);
      return true;
   }

   public static async Task<bool> DeleteDayOff(string companyId, string id, string dayOff, DateTime date, bool isSoftDelete)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);
      var day = date.Ticks;

      var filterBuilder = Builders<HrmTimeListRegisterModel>.Filter;
      var updateBuilder = Builders<HrmTimeListRegisterModel>.Update;

      var filter = filterBuilder.Eq(x => x.id, id) & filterBuilder.Where(x => x.shifts_register.Any(y => y.day == day));
      var update = updateBuilder.Set(x => x.shifts_register[-1].dayoff_id, null as string);
      await collection.UpdateOneAsync(filter, update);

      return true;
   }

   public static async Task<HrmTimeListRegisterModel> Get(string companyId, string id)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
   }

   public static async Task<HrmTimeListRegisterModel.ShiftRegister> GetByDay(string companyId, string id, long day)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      var result = await collection.FindAsync(x => x.id == id && x.shifts_register.Any(y => y.day == day)).Result.ToListAsync();

      var list = result.Select(x => x.shifts_register.FirstOrDefault(y => y.day == day)).ToList();

      return list.FirstOrDefault();
   }


   public static async Task<List<HrmTimeListRegisterModel>> GetList(string companyId)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      return await collection.FindAsync(new BsonDocument()).Result.ToListAsync();
   }


   /// <summary>
   /// Lấy danh sách ca làm theo tháng của nhân sự
   /// </summary>
   /// <param name="companyId">id của công ty</param>
   /// <param name="month">Tháng hiện tại</param>
   public static async Task<List<HrmTimeListRegisterModel>> GetListByMonth(string companyId, DateTime month)
   {
      month = Convert.ToDateTime(month.ToString("yyyy-MM-01"));

      var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
      var endMonth = month.AddDays(daysInMonth - 1);

      long start = month.Ticks;
      long end = endMonth.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);
      var builder = Builders<HrmTimeListRegisterModel>.Filter;

      var filtered = builder.ElemMatch(
      x => x.shifts_register,
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
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      var data = await GetList(companyId);

      var updatedData = data.Select(doc =>
      {
         var updatedShifts = doc.shifts_register.Select(shift =>
               {
                  if (shift.shifts_id.Contains(workShiftId) && shift.day > DateTime.Now.Ticks)
                  {
                     shift.shifts_id.Remove(workShiftId);
                  }
                  return shift;
               }).ToList();

         doc.shifts_register = updatedShifts;
         return doc;
      }).ToList();

      foreach (var updatedDoc in updatedData)
      {
         var filter = Builders<HrmTimeListRegisterModel>.Filter.Eq(x => x.id, updatedDoc.id);
         var update = Builders<HrmTimeListRegisterModel>.Update.Set(x => x.shifts_register, updatedDoc.shifts_register);
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
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);
      var builder = Builders<HrmTimeListRegisterModel>.Update;
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
   public static async Task<bool> HandleData(string id, List<string> dataShifts, string companyId, DateTime date, string dayOffId = null, bool isOverrided = false, bool isConfirm = false)
   {
      var data = await Get(companyId, id);
      if (data != null)
      {
         var dayShift = data.shifts_register.FirstOrDefault(s => s.day == date.Ticks);
         if (dayShift != null)
         {
            if (Handled.Shared.IsEmpty(dayOffId))
               dayShift.shifts_id = dataShifts;
            if (isOverrided)
               dayShift.dayoff_id = dayOffId;
            else
               dayShift.dayoff_id = dayShift.dayoff_id ?? dayOffId;

            if (isConfirm)
               dayShift.is_confirm = isConfirm;
            await Update(companyId, dayShift, id);
         }
         else
         {
            var model = new HrmTimeListRegisterModel.ShiftRegister
            {
               day = date.Ticks,
               shifts_id = dataShifts,
               dayoff_id = dayOffId
            };

            if (isConfirm)
               model.is_confirm = isConfirm;
            await UpdateAdd(companyId, model, id);
         }
      }
      else
      {
         var shifts = new List<HrmTimeListRegisterModel.ShiftRegister>
         {
            new HrmTimeListRegisterModel.ShiftRegister
            {
               day = date.Ticks,
               shifts_id = dataShifts,
               dayoff_id = dayOffId,
               is_confirm = isConfirm
            }
         };
         var model = new HrmTimeListRegisterModel
         {
            id = id,
            shifts_register = shifts
         };
         await Create(companyId, model);
      }
      return true;
   }

   /// <summary>Danh sách ca làm theo ngày</summary>
   public static async Task<List<HrmTimeListRegisterModel.ShiftRegister>> GetByRangeDate(string companyId, string userId, long start, long end)
   {

      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      var filter = Builders<HrmTimeListRegisterModel>.Filter.And(
                        Builders<HrmTimeListRegisterModel>.Filter.Eq(x => x.id, userId),
                        Builders<HrmTimeListRegisterModel>.Filter.ElemMatch(x => x.shifts_register, y => y.day >= start && y.day <= end)
      );

      var result = await collection.FindAsync(filter).Result.ToListAsync();

      var shifts = result.SelectMany(x => x.shifts_register.Where(y => y.day >= start && y.day <= end)).ToList();

      return shifts;
   }

   /// <summary>Danh sách ca làm theo ngày và theo danh sách user</summary>
   public static async Task<List<HrmTimeListRegisterModel>> GetByRangeAndUsers(string companyId, List<string> userList, long start, long end)
   {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<HrmTimeListRegisterModel>(_collection);

      var builder = Builders<HrmTimeListRegisterModel>.Filter;

      var filter = builder.And(builder.In(x => x.id, userList), builder.ElemMatch(x => x.shifts_register, y => y.day >= start && y.day <= end));

      var result = await collection.FindAsync(filter).Result.ToListAsync();

      return result;
   }

}




