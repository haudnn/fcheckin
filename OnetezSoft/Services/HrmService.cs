using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;
using System.Globalization;
using OnetezSoft.Pages.Hrm;
using static OnetezSoft.Models.HrmFormModel;

namespace OnetezSoft.Services;

public class HrmService
{


  /// <summary>
  /// Kiểm tra vị trí hiện tại có đang trong công ty hay không?
  /// </summary>
  public static bool CompareLocation(HrmLocationModel location, double latitude, double longitude, out string logs)
  {
    double lat_cty = Handled.Shared.ConvertToDouble(location.latitude);
    double lon_cty = Handled.Shared.ConvertToDouble(location.longitude);
    long distance = Handled.Shared.GetDistanceFromLatLon(lat_cty, lon_cty, latitude, longitude);

    logs = $"[{DateTime.Now.ToString("HH:mm:ss")}] Tọa độ: {latitude}, {longitude}. Khoảng cách: {distance} mét";
    logs = $"[{DateTime.Now.ToString("HH:mm:ss")}] Khoảng cách: "
        + (distance > 1000 ? (distance / 1000) + " km" : distance + " mét");

    if (distance <= location.radius)
      return true;
    else
      return false;
  }

  /// <summary>
	/// Lấy khoảng cách từ 2 tọa độ
	/// </summary>
	public static long GetDistance(HrmLocationModel location, double latitude, double longitude)
  {
    double lat_cty = Handled.Shared.ConvertToDouble(location.latitude);
    double lon_cty = Handled.Shared.ConvertToDouble(location.longitude);
    long distance = Handled.Shared.GetDistanceFromLatLon(lat_cty, lon_cty, latitude, longitude);

    return distance;

  }

  #region Các hàm bảng công

  /// <summary>
  /// Hàm đồng bộ dữ liệu khi tạo, chỉnh sửa bảng công
  /// </summary>
  public static async Task UpdateTimeSheetUser(string companyId, string userId, HrmTimesheetModel sheet, bool checkTimeTotal, bool checkDelete)
  {
    HrmTimesheetUserModel timesheetUser = await DbHrmTimesheetUser.GetByTimesheetId(companyId, userId, sheet.id);
    var checkUpdate = false;

    if (timesheetUser != null)
    {
      checkUpdate = true;
      timesheetUser.from = sheet.from;
      timesheetUser.to = sheet.to;
      timesheetUser.updated = DateTime.Now.Ticks;
      timesheetUser.time_total_sheet = Math.Round(sheet.time_total, 2);

      if (checkDelete)
        timesheetUser.is_delete = true;
      else
        timesheetUser.is_delete = false;

      if (checkTimeTotal)
        timesheetUser.is_edit_time = false;
    }
    else
    {
      if (!checkDelete)
      {
        // tạo mới dữ liệu
        timesheetUser = new()
        {
          user = userId,
          timesheet_id = sheet.id,
          from = sheet.from,
          to = sheet.to,
          updated = DateTime.Now.Ticks,
          time_total_sheet = Math.Round(sheet.time_total, 2),
        };
      }
    }

    // lấy lại những dữ liệu cũ đã qua chỉnh sửa
    List<HrmTimesheetUserModel> timesheetUserData = await DbHrmTimesheetUser.GetAllByRangeUserId(companyId, sheet.from, sheet.to, userId);
    var timekeepingUpdate = new List<HrmTimesheetUserModel.TimeSheetDate>();

    if (timesheetUserData.Any())
    {
      var timesheetDates = timesheetUserData.SelectMany(i => i.timesheet_dates).GroupBy(i => i.date);

      // Thêm dữ liệu theo date range
      for (var i = sheet.from; i <= sheet.to; i = new DateTime(i).AddDays(1).Ticks)
      {
        var day = i;
        foreach (var item in timesheetDates)
        {
          if (item.Key == day)
            timekeepingUpdate.Add(item.ToList().FirstOrDefault());
        }
      }
    }

    var dateList = timekeepingUpdate.Select(i => i.date).ToList();
    // Thêm dữ liệu của những ngày chưa có dữ liệu
    for (var i = sheet.from; i <= sheet.to; i = new DateTime(i).AddDays(1).Ticks)
    {
      var day = i;
      if (!dateList.Contains(day))
      {
        timekeepingUpdate.Add(new()
        {
          date = day
        });
      }
    }

    if (!checkUpdate)
    {
      timesheetUser.timesheet_dates = timekeepingUpdate;
      await DbHrmTimesheetUser.Create(companyId, timesheetUser);
    }
    else
    {
      // lọc các trường hợp date range nhỏ lại
      timesheetUser.timesheet_dates = timekeepingUpdate.Where(i => i.date >= sheet.from && i.date <= sheet.to).ToList();
      await DbHrmTimesheetUser.Update(companyId, timesheetUser);
    }
  }

  /// <summary>Cập nhật đơn từ sang bảng công</summary>
  public static async Task<(HrmFormModel, List<long>)> UpdateFormTimeSheetUser(string companyId, HrmFormModel hrmForm, List<HrmTimeListModel> timeList, List<HrmWorkShiftModel> workShiftList, List<HrmDayOffModel> dayOffs, bool checkUpdate)
  {
    List<long> lockDateList = new();
    List<long> dateCheckUpdate = new();
    // danh sách ngày cập nhật
    var startDate = hrmForm.work_date_shifts.Min(x => x.start);
    var endDate = hrmForm.work_date_shifts.Max(x => x.end);

    if (startDate <= DateTime.Today.AddDays(1).Ticks)
    {
      long end;

      if (endDate >= DateTime.Today.Ticks + new TimeSpan(23, 59, 59).Ticks)
        end = DateTime.Today.Ticks + new TimeSpan(23, 59, 59).Ticks;
      else
        end = endDate;

      // lấy danh sách phân ca
      var timeListItem = timeList.Find(i => i.id == hrmForm.user);
      var shiftList = new List<HrmTimeListModel.Shift>();

      if (timeListItem != null)
      {
        shiftList = timeListItem.shifts.Where(i => i.day >= new DateTime(startDate).Date.Ticks && i.day <= new DateTime(end).Date.Ticks).ToList();
      }

      List<HrmTimesheetUserModel> dataTimesheetUsers = await DbHrmTimesheetUser.GetAllByRangeUserId(companyId, startDate, end, hrmForm.user);

      Dictionary<long, List<string>> workShiftsUpdate = ConvertDateRangeToTimeList(hrmForm.user, startDate, end, shiftList, workShiftList, dayOffs);

      foreach (var item in workShiftsUpdate)
      {
        var timesheetUsers = dataTimesheetUsers.Where(i => i.timesheet_dates.Select(i => i.date).Contains(item.Key)).ToList();
        if (timesheetUsers.Any())
        {
          foreach (var timesheetUser in timesheetUsers)
          {
            var timesheetDate = timesheetUser.timesheet_dates.Find(i => i.date == item.Key);
            // bảng công bị khoá thi không nạp
            if (timesheetDate != null)
            {
              dateCheckUpdate.Add(timesheetDate.date);
              if (!timesheetDate.locked)
              {
                foreach (var shiftId in item.Value)
                {
                  // lock thì vẫn nạp dữ liệu những không tính công đơn từ
                  var form = new HrmTimesheetUserModel.TimeSheetForm()
                  {
                    form_id = hrmForm.id,
                    updated = DateTime.Now.Ticks,
                  };

                  if (timesheetDate.shifts_form.ContainsKey(shiftId))
                  {
                    if (timesheetDate.shifts_form[shiftId].form_id != hrmForm.id)
                      timesheetDate.shifts_form[shiftId] = form;
                  }
                  else
                    timesheetDate.shifts_form.Add(shiftId, form);
                }

                timesheetUser.timesheet_dates.RemoveAll(i => i.date == item.Key);
                timesheetUser.timesheet_dates.Add(timesheetDate);
              }
              else
              {
                lockDateList.Add(timesheetDate.date);
              }
            }

            if (checkUpdate)
            {
              await DbHrmTimesheetUser.Update(companyId, timesheetUser);
            }
          }
        }
      }

      if (checkUpdate && dateCheckUpdate.Any())
      {
        // cập nhật đã load bên đơn từ
        var workDateShifts = hrmForm.work_date_shifts.Where(i => true).ToList();
        if (new DateTime(startDate).Date == new DateTime(endDate).Date
            && new DateTime(startDate).Date == DateTime.Now.Date && dateCheckUpdate.Contains(DateTime.Today.Ticks))
        {
          //TH: trong ngày
          foreach (var item in hrmForm.work_date_shifts)
          {
            item.loaded = true;
            workDateShifts.RemoveAll(i => i.start == item.start && i.end == item.end);
            workDateShifts.Add(item);
          }
        }
        else
        {
          foreach (var item in hrmForm.work_date_shifts)
          {
            List<long> dateChecks = new List<long>();
            for (DateTime i = new DateTime(item.start).Date; i <= new DateTime(item.end).Date; i = i.AddDays(1))
            {
              dateChecks.Add(i.Ticks);
            }

            if (dateChecks.All(i => dateCheckUpdate.Contains(i)))
            {
              item.loaded = true;
              workDateShifts.RemoveAll(i => i.start == item.start && i.end == item.end);
              workDateShifts.Add(item);
            }
          }
        }
        hrmForm.work_date_shifts = workDateShifts;
      }
    }

    return (hrmForm, lockDateList);
  }



  public static List<HrmWorkShiftModel> SuggestShift(HrmTimeListModel.Shift userShift, List<HrmWorkShiftModel> workshifts, int timeInEarly)
  {
    DateTime currentTime = DateTime.Now;
    var now = DateTime.Now.ToString("HH:mm:ss");
    var suggestedShifts = new List<HrmWorkShiftModel>();

    // Xử lý những ca làm đã qua thời gian checkout
    foreach (var shiftId in userShift.shifts_id)
    {
      var workshift = workshifts.FirstOrDefault(i => i.id == shiftId);
      if (workshift == null)
        continue;
      var date1 = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd, " + now));
      var date2 = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd, " + workshift.checkout));
      var result = Handled.Shared.CompareTime(date1, date2);
      if (result < 0)
      {
        if (workshift.is_overday)
          suggestedShifts.Add(workshift);
      }
      else
        suggestedShifts.Add(workshift);
    }

    // Sắp xếp lại ca làm theo thời gian gần nhất.
    suggestedShifts = suggestedShifts
            .OrderBy(shift => Math.Abs((DateTime.ParseExact(shift.checkin, "HH:mm", CultureInfo.InvariantCulture) - DateTime.Now).TotalMinutes)).ToList();

    // Néu có cấu hình thời gian checkin sớm thì remove những ca làm chưa đến thời gian checkin.
    if (timeInEarly > 0)
    {
      suggestedShifts.RemoveAll(shift =>
      {
        DateTime checkinTime = DateTime.ParseExact(shift.checkin, "HH:mm", CultureInfo.InvariantCulture);
        TimeSpan timeDifference = checkinTime - currentTime;
        return timeDifference.TotalMinutes > timeInEarly;
      });
    }
    return suggestedShifts;
  }

  /// <summary>
  /// Hàm xử lý kiêm tra có được xóa hoặc sửa ngày nghỉ không?
  /// </summary>

  public static bool CheckLockedDayOff(HrmDayOffModel dayOff)
  {
    bool isLocked = false;

    long timeNow = DateTime.Today.Ticks;
    long timeStart = dayOff.start;
    long timeEnd = dayOff.end;

    // Nếu chưa có nhân viên nào áp dụng thì cho phép xóa
    if (dayOff.salary_users.Count == 0 && dayOff.non_salary_users.Count == 0)
      return isLocked;

    // Nếu đã qua ngày kết thúc thì cho phép xóa
    if (timeNow > timeEnd)
      return isLocked;


    // Nếu đã qua thời gian bắt đầu và còn trong thời gian kết thúc
    if (timeNow >= timeStart && timeNow <= timeEnd)
      isLocked = true;

    // Nếu đã qua thời gian hiện tại và đã có nhân viên áp dụng
    if (timeNow >= timeStart && (dayOff.salary_users.Count > 0 || dayOff.non_salary_users.Count > 0))
      isLocked = true;

    return isLocked;
  }

  public static string CountDownDayOff(long endDate)
  {
    DateTime today = DateTime.Today;
    DateTime endDay = new DateTime(endDate);

    if (today == endDay)
    {
      endDay = new DateTime(endDate).Date.AddDays(1);
      TimeSpan remainingTime = endDay - DateTime.Now;
      return remainingTime.Hours + " giờ";
    }
    TimeSpan remainingDays = endDay - today;
    return remainingDays.Days + " ngày";
  }


  /// <summary>
  /// Hàm kiểm tra có checkout trước giờ checkin luôn hay không?
  /// </summary>
  /// <param name="checkin">Giờ checkin</param>
  /// <param name="checkout">Giờ checkout</param>
  /// <param name="realTime">Thời gian checkout</param>
  /// <param name="isOverDay">Kiểm tra qua ngày</param>
  /// <returns></returns>
  public static bool CheckEarly(string checkin, string checkout, string realTime, bool isOverDay)
  {
    if (isOverDay)
    {
      DateTime currentDate = DateTime.Now.Date;
      DateTime checkinConverted = Convert.ToDateTime(currentDate.ToString("yyyy-MM-dd, " + checkin));
      DateTime checkoutConverted = Convert.ToDateTime(currentDate.AddDays(1).ToString("yyyy-MM-dd, " + checkout));
      DateTime realTimeConverted = Convert.ToDateTime(currentDate.ToString("yyyy-MM-dd, " + realTime));

      long result = Handled.Shared.CompareTime(checkinConverted, checkoutConverted);
      long result2 = Handled.Shared.CompareTime(realTimeConverted, checkoutConverted);
      if (result2 > result)
        return true;
      else
        return false;
    }
    else
    {
      DateTime checkinCoverted = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd, " + checkin));
      DateTime checkoutCoverted = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd, " + checkout));
      DateTime realTimeCoverted = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd, " + realTime));

      long result = Handled.Shared.CompareTime(checkinCoverted, checkoutCoverted);
      long result2 = Handled.Shared.CompareTime(realTimeCoverted, checkoutCoverted);

      if (result2 > result)
        return true;
      else
        return false;
    }
  }

  /// <summary>
  /// Hàm lấy phút chekout sớm ca qua ngày
  /// </summary>
  /// <returns></returns>
  public static long CheckTimeDifference(long timeCheckout, long timeCheckin, string timeAllowed)
  {
    var checkinConverted = new DateTime(timeCheckin);
    var checkoutConverted = new DateTime(timeCheckout);
    long result;

    // Nếu checkout ở ngày checkin
    if (checkinConverted.Date == checkoutConverted.Date)
    {
      DateTime converted = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd, " + timeAllowed));
      result = Handled.Shared.CompareTime(checkoutConverted, converted);
    }
    // Nếu checkout sau ngày checkin
    else
    {
      TimeSpan difference = checkoutConverted - checkinConverted;
      if ((int)difference.TotalDays < 1)
      {
        var date2 = Convert.ToDateTime(checkinConverted.AddDays(1).ToString("yyyy-MM-dd, " + timeAllowed));
        result = Handled.Shared.CompareTime(checkoutConverted, date2);
      }
      else
      {
        result = 0;
      }
    }
    return result;
  }

  /// <summary>
  /// Các hạng mục chính
  /// </summary>
  public static List<HrmOptionModel> Options()
  {
    var list = new List<HrmOptionModel>();

    list.Add(new HrmOptionModel
    {
      id = "company",
      name = "Công ty",
      type = "company"
    });

    list.Add(new HrmOptionModel
    {
      id = "department",
      name = "Phòng ban",
      type = "department"
    });

    return list;
  }


  /// <summary>
  /// Hàm kiểm tra có khoảng gap giữa các ca làm không?
  /// </summary>
  public static bool CheckGapShifts(List<HrmWorkShiftModel> shifts)
  {
    for (int i = 0; i < shifts.Count - 1; i++)
    {
      var shiftA = shifts[i];
      var shiftB = shifts[i + 1];
      TimeSpan checkoutA = TimeSpan.Parse(shiftA.checkout);
      TimeSpan checkinB = TimeSpan.Parse(shiftB.checkin);
      if (checkinB < checkoutA)
        return true;
    }
    return false;
  }

  public static List<string> SortedShifts(List<HrmWorkShiftModel> shifts)
  {
    return shifts.OrderBy(x => TimeSpan.Parse(x.checkin)).Select(x => x.id).ToList();
  }

  public static List<HrmWorkShiftModel> SortedShiftsTimeSheet(List<HrmWorkShiftModel> shifts)
  {
    return shifts.OrderBy(x => TimeSpan.Parse(x.checkin)).ToList();
  }


  public static List<StaticModel> FilterTime()
  {
    var list = new List<StaticModel>();

    list.Add(new StaticModel
    {
      id = 1,
      name = "Tuần này",
    });

    list.Add(new StaticModel
    {
      id = 11,
      name = "Tuần trước",
    });

    list.Add(new StaticModel
    {
      id = 2,
      name = "Tháng này",
      isDefault = true
    });

    list.Add(new StaticModel
    {
      id = 22,
      name = "Tháng trước",
    });

    return list;
  }

  public static List<StaticModel> FilterStatus()
  {
    var list = new List<StaticModel>();

    list.Add(new StaticModel
    {
      id = 0,
      name = "Tất cả",
      isDefault = true
    });

    list.Add(new StaticModel
    {
      id = 1,
      name = "Ca tính lương",
    });

    list.Add(new StaticModel
    {
      id = 7,
      name = "Ca không tính lương",
    });

    list.Add(new StaticModel
    {
      id = 2,
      name = "Ca check-in trễ",
    });

    list.Add(new StaticModel
    {
      id = 3,
      name = "Ca check-out sớm",
    });

    list.Add(new StaticModel
    {
      id = 4,
      name = "Ca qua ngày",
    });

    list.Add(new StaticModel
    {
      id = 5,
      name = "Ca ngoài vị trí",
    });

    list.Add(new StaticModel
    {
      id = 6,
      name = "Ca làm ngoài giờ",
    });
    return list;
  }

  #endregion


  /// <summary>Cập nhật từ khoảng thời gian bên đơn từ của user lấy ra danh sách phân ca, Key: ngày, Value: danh sách phân ca trong ngày</summary>
  public static Dictionary<long, List<string>> ConvertDateRangeToTimeList(string user, long start, long end, List<HrmTimeListModel.Shift> shiftList, List<HrmWorkShiftModel> workShiftList, List<HrmDayOffModel> hrmDayOffs, bool checkDateOff = true)
  {
    Dictionary<long, List<string>> shiftByDates = new();
    if (shiftList.Any())
    {
      foreach (var item in shiftList)
      {
        if (string.IsNullOrEmpty(item.dayoff_id))
        {
          var shiftsId = item.shifts_id.Where(i => true).ToList();
          if (item.shifts_id.Any())
          {
            foreach (var shiftId in item.shifts_id)
            {
              var workShift = workShiftList.Find(i => i.id == shiftId);
              if (workShift != null)
              {
                var timeInString = new DateTime(item.day).ToString("dd'/'MM'/'yyyy") + " " + workShift.checkin;
                var timeInCheck = DateTime.ParseExact(timeInString, "dd'/'MM'/'yyyy HH:mm", CultureInfo.InvariantCulture).Ticks;
                if (start > timeInCheck || timeInCheck > end)
                  shiftsId.RemoveAll(i => i == workShift.id);
              }
            }
            shiftByDates.Add(item.day, shiftsId);
          }
        }
        else
        {
          //TH ngày nghỉ
          if (checkDateOff)
          {
            var itemOff = hrmDayOffs.Find(i => i.id == item.dayoff_id);
            if (itemOff != null && itemOff.salary_users.Contains(user))
              shiftByDates.Add(item.day, item.shifts_id);
          }
        }
      }
    }
    return shiftByDates;
  }
}