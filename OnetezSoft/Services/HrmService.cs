using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services;

public class HrmService
{
  /// <summary>Số ngày tự cập nhật lại bảng công</summary>
  private static int DayAutoUpdateTimesheet = -1;

  /// <summary>Thời gian đi trễ về sớm cho phép (phút). Mặc định 60 phút</summary>
  public static int AllowedTimeDifference = 0;


  #region Các hàm xử lý chấm công

  /// <summary>
  /// Kiểm tra vị trí hiện tại có đang trong công ty hay không?
  /// </summary>
  public static bool CompareLocation(HrmLocationModel location, double latitude, double longitude, out string logs)
  {
    double lat_cty = Handled.Shared.ConvertToDouble(location.latitude);
    double lon_cty = Handled.Shared.ConvertToDouble(location.longitude);
    long distance = Handled.Shared.GetDistanceFromLatLon(lat_cty, lon_cty, latitude, longitude);
    logs = $"[{DateTime.Now.ToString("HH:mm:ss")}] Tọa độ: {latitude}, {longitude}. Khoảng cách: {distance} mét";
    Console.WriteLine(logs);
    logs = $"[{DateTime.Now.ToString("HH:mm:ss")}] Khoảng cách: "
      + (distance > 1000 ? (distance / 1000) + " km" : distance + " mét");

    if (distance <= location.radius)
      return true;
    else
      return false;
  }

  /// <summary>
  /// Hàm kiểm tra quyền chấm công trong công ty
  /// </summary>
  public static bool CheckRole(UserModel user, List<HrmOptionModel> companys, string companyId)
  {
    if (user.role == 1)
      return true;
    if (user.role == 2 && user.role_manage.timekeeping && !string.IsNullOrEmpty(companyId))
      if (companys.Any(x => x.id == companyId && x.managers.Contains(user.id)))
        return true;

    return false;
  }

  /// <summary>
  /// Lấy danh sách các tài khoản đang sử dụng phân ca
  /// </summary>
  public static async Task<List<HrmUserShiftModel>> GetWorkShiftsUse(string companyId, string workShiftId)
  {
    var results = new List<HrmUserShiftModel>();

    var dataList = await DbHrmUserShift.GetList(companyId);
    foreach (var item in dataList)
    {
      if (item.shifts.Any(x => x.morning == workShiftId || x.afternoon == workShiftId))
        results.Add(item);
    }

    return results;
  }

  /// <summary>
  /// Kiểm tra dữ liệu chấm công có hợp lệ không
  /// </summary>
  public static bool TimekeepingIsValid(HrmTimekeepingModel.TimeData item)
  {
    return item.is_valid && item.time_difference <= 0 && item.in_company;
  }

  #endregion


  #region Các hàm bảng công

  /// <summary>
  /// Hàm đồng bộ dữ liệu chấm công qua bảng công
  /// </summary>
  public static async Task<HrmTimesheetModel> UpdateTimeSheet(string companyId, string userId, DateTime month,
    List<DayOffModel> daysOff)
  {
    // Set tháng về ngày 1 cho chuẩn dữ liệu
    month = Convert.ToDateTime(month.ToString("yyyy-MM-01"));
    var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
    // Lấy dữ liệu bảng công tháng
    var sheetId = month.ToString("yyMM") + userId;
    var timesheetMonth = await DbHrmTimesheet.Get(companyId, sheetId);
    if (timesheetMonth == null)
    {
      Console.WriteLine("Create TimeSheet " + sheetId);
      // Tạo dữ liệu mẫu cho tháng
      timesheetMonth = new HrmTimesheetModel();
      timesheetMonth.id = sheetId;
      timesheetMonth.user = userId;
      timesheetMonth.month = month.Ticks;
      timesheetMonth.updated = DateTime.Now.Ticks;
      for (int i = 0; i < daysInMonth; i++)
      {
        timesheetMonth.days.Add(new()
        {
          day = month.AddDays(i).Ticks
        });
      }
      await DbHrmTimesheet.Create(companyId, timesheetMonth);
    }

    // Dữ liệu phân ca
    var shiftList = await DbHrmWorkShift.GetList(companyId);
    // Phân ca của nhân sự
    var userShift = await DbHrmUserShift.Get(companyId, userId);

    // Có phân ca mới xử lý
    if (userShift != null && shiftList.Count > 0)
    {
      // Lấy dữ liệu chấm công
      var timekeepingList = await DbHrmTimekeeping.GetList(companyId, userId,
        month.Ticks, month.AddMonths(daysInMonth - 1).Ticks);

      // Gán dữ liệu chấm công qua bảng công
      for (int i = 0; i < daysInMonth; i++)
      {
        // Lấy ngày chấm công
        var day = month.AddDays(i);
        // Lấy dữ liệu theo ngày trong bảng công
        var timesheetDay = timesheetMonth.days.FirstOrDefault(x => x.day == day.Ticks);

        // Dữ liệu bảng công cũ hơn số ngày cấu hình và có dữ liệu rồi
        // Thì thôi không cần tính lại và cập nhật lại dữ liệu nữa
        if (day < DateTime.Today.AddDays(DayAutoUpdateTimesheet)
          && (timesheetDay.morning != null || timesheetDay.afternoon != null))
          continue;

        // Dữ liệu đã chỉnh sửa thì cũng không cần tính lại
        if ((timesheetDay.morning != null && timesheetDay.morning.edited)
          || (timesheetDay.afternoon != null && timesheetDay.afternoon.edited))
          continue;

        // Lấy dữ liệu chấm công
        var timekeeping = timekeepingList.FirstOrDefault(x => x.date == day.Ticks);
        // Lấy số công cấu hình ca sáng
        double timeWorkMorning = GetTimeWorkInShift(shiftList, userShift, day, true);
        // Lấy số công cấu hình ca chiều
        double timeWorkAfternoon = GetTimeWorkInShift(shiftList, userShift, day, false);

        // Có dữ liệu chấm công
        if (timekeeping != null)
        {
          // Chấm công buổi sáng
          if (timekeeping.morning_checkout != null)
            timesheetDay.morning = GetTimeWorkInTimekeeping(timekeeping.morning_checkin, timekeeping.morning_checkout);
          else if (timeWorkMorning > 0)
            timesheetDay.morning = new() { time = timeWorkMorning, type = "V" };
          else
            timesheetDay.morning = null;

          // Chấm công buổi chiều
          if (timekeeping.afternoon_checkout != null)
            timesheetDay.afternoon = GetTimeWorkInTimekeeping(timekeeping.afternoon_checkin, timekeeping.afternoon_checkout);
          else if (timeWorkAfternoon > 0)
            timesheetDay.afternoon = new() { time = timeWorkAfternoon, type = "V" };
          else
            timesheetDay.afternoon = null;
        }
        // Quá ngày nhưng không chấm công
        else if (day < DateTime.Today)
        {
          // Kiểm tra ngày nghỉ
          var dayOff = DbDayOff.CheckOff(daysOff, day, out bool hasSalary);
          // Ngày nghỉ có tính công
          if (dayOff && hasSalary)
          {
            // Gán số công ca sáng
            if (timeWorkMorning > 0)
              timesheetDay.morning = new() { time = timeWorkMorning, type = "L" };
            else
              timesheetDay.morning = null;

            // Gán số công ca chiều
            if (timeWorkAfternoon > 0)
              timesheetDay.afternoon = new() { time = timeWorkAfternoon, type = "L" };
            else
              timesheetDay.afternoon = null;
          }
          // Không phải ngày nghỉ
          else if (!dayOff)
          {
            // Chế độ linh động
            if (userShift.is_flexible)
            {
              // Gán số công ca sáng
              if (timeWorkMorning > 0)
                timesheetDay.morning = new() { time = timeWorkMorning };
              else
                timesheetDay.morning = null;

              // Gán số công ca chiều
              if (timeWorkAfternoon > 0)
                timesheetDay.afternoon = new() { time = timeWorkAfternoon };
              else
                timesheetDay.afternoon = null;
            }
            // Chế độ làm bình thường nhưng không chấm công
            else
            {
              // Gán số công ca sáng
              if (timeWorkMorning > 0)
                timesheetDay.morning = new() { time = timeWorkMorning, type = "V" };
              else
                timesheetDay.morning = null;

              // Gán số công ca chiều
              if (timeWorkAfternoon > 0)
                timesheetDay.afternoon = new() { time = timeWorkAfternoon, type = "V" };
              else
                timesheetDay.afternoon = null;
            }
          }
        }
        // Chưa đến ngày chấm công
        else
        {
          timesheetDay.morning = null;
          timesheetDay.afternoon = null;
        }
      }

      timesheetMonth.updated = DateTime.Now.Ticks;
      await DbHrmTimesheet.Update(companyId, timesheetMonth);
    }

    return timesheetMonth;
  }


  /// <summary>
  /// Gán số công hợp lệ trong 1 ca vào bảng công
  /// </summary>
  public static HrmTimesheetModel.TimeData GetTimeWorkInTimekeeping(HrmTimekeepingModel.TimeData checkin,
    HrmTimekeepingModel.TimeData checkout)
  {
    var result = new HrmTimesheetModel.TimeData();
    // Số công theo cấu hình
    result.time = checkin.time_work;
    // Cảnh báo dữ liệu bất thường
    if (TimekeepingIsValid(checkin) && TimekeepingIsValid(checkout))
      result.warning = false;
    else
      result.warning = true;

    // Có dữ liệu checkout
    if (!string.IsNullOrEmpty(checkout.time_active))
    {
      if (checkin.is_valid && checkout.is_valid)
        result.type = null; // Công tính lương
      else
        result.type = "0"; // Công không hợp lệ
    }
    // Checkin nhưng không checkout
    else if (!string.IsNullOrEmpty(checkin.time_active))
      result.type = "0"; // Không không hợp lệ
    else
      result.type = "V";

    return result;
  }


  /// <summary>
  /// Lấy cấu hình ca làm việc của 1 ca trong ngày
  /// </summary>
  public static async Task<HrmWorkShiftModel> GetWorkShiftInDay(string companyId, string userId,
    DateTime day, bool isMorning)
  {
    // Dữ liệu phân ca
    var shiftList = await DbHrmWorkShift.GetList(companyId);
    if (shiftList.Count > 0)
    {
      // Phân ca của nhân sự
      var userShift = await DbHrmUserShift.Get(companyId, userId);
      if (userShift != null)
        return GetWorkShiftInDay(shiftList, userShift, day, isMorning);
    }

    return null;
  }


  /// <summary>
  /// Lấy cấu hình ca làm việc của 1 ca trong ngày
  /// </summary>
  public static HrmWorkShiftModel GetWorkShiftInDay(List<HrmWorkShiftModel> shiftList, HrmUserShiftModel userShift,
    DateTime day, bool isMorning)
  {
    // Dữ liệu phân ca
    if (shiftList.Count > 0)
    {
      // Phân ca của nhân sự
      if (userShift != null)
      {
        // Ca làm theo ngày
        var dayShift = userShift.shifts.FirstOrDefault(x => x.day == (int)day.DayOfWeek);
        if (dayShift != null)
        {
          // Lấy cấu hình ca làm
          if (isMorning)
            return shiftList.FirstOrDefault(x => x.id == dayShift.morning);
          else
            return shiftList.FirstOrDefault(x => x.id == dayShift.afternoon);
        }
      }
    }

    return null;
  }


  /// <summary>
  /// Lấy số công trong 1 ca theo cấu hình
  /// </summary>
  public static double GetTimeWorkInShift(List<HrmWorkShiftModel> shiftList, HrmUserShiftModel userShift,
    DateTime day, bool isMorning)
  {
    // Lấy cấu hình ca làm
    HrmWorkShiftModel workShift = GetWorkShiftInDay(shiftList, userShift, day, isMorning);
    // Có cấu hình ca làm
    if (workShift != null)
      return workShift.value;
    else
      return 0;
  }


  #endregion


  #region Dữ liệu cố định

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
  /// Chi tiết hạng mục chính
  /// </summary> 
  public static HrmOptionModel Options(string type)
  {
    var model = Options().SingleOrDefault(x => x.type == type);
    if (model != null)
      return model;
    else
      return new HrmOptionModel();
  }

  /// <summary>
  /// Kiểu tính công
  /// </summary>
  public static List<StaticModel> TimeType()
  {
    var list = new List<StaticModel>();

    list.Add(new() { icon = "D", name = "Công tác", color = "#AEC6FF" });
    list.Add(new() { icon = "O", name = "Làm việc từ xa", color = "#AEC6FF" });
    list.Add(new() { icon = "L", name = "Lễ/Tết", color = "#AEC6FF" });
    list.Add(new() { icon = "P", name = "Phép năm dùng", color = "#AEC6FF" });
    list.Add(new() { icon = "C", name = "Cưới/Tang", color = "#AEC6FF" });
    list.Add(new() { icon = "V", name = "Nghỉ phép không lương", color = "#FFB4A9" });
    list.Add(new() { icon = "K", name = "Nghỉ không xin phép", color = "#FFB4A9" });
    list.Add(new() { icon = "T", name = "Thai sản", color = "#FFB4A9" });
    list.Add(new() { icon = "OT", name = "Làm ngày lễ", color = "#CEF9CC" });
    list.Add(new() { icon = "0", name = "Công không hợp lệ", color = "#f9f9f9" });

    return list;
  }

  /// <summary>
  /// Chi tiết kiểu tính công
  /// </summary> 
  public static StaticModel TimeType(string type)
  {
    var model = TimeType().SingleOrDefault(x => x.icon == type);
    if (model != null)
      return model;
    else
      return new StaticModel() { name = "Công tính lương", color = "#fff" };
  }

  #endregion 
}
