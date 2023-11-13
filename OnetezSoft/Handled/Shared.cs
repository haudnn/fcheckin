using HtmlAgilityPack;
using Newtonsoft.Json;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace OnetezSoft.Handled
{
  public static class Shared
  {
    #region Các hàm liên quan đến String

    public static bool IsEmpty(string value)
    {
      if (string.IsNullOrEmpty(value))
        return true;
      if (string.IsNullOrEmpty(value.Trim()))
        return true;
      return false;
    }

    /// <summary>
    /// String to MD5
    /// </summary>
    public static string CreateMD5(string input)
    {
      // Use input string to calculate MD5 hash
      using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
      {
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Convert the byte array to hexadecimal string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
          sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
      }
    }


    /// <summary>
    /// Format kiểu dữ liệu phần trăm
    /// </summary>
    public static string FormatPercent(double value)
    {
      if (value == 0)
        return "0";
      else if (value >= 1000 || value <= -1000)
        return value.ToString("0,0");
      else if (value % 1 == 0)
        return value.ToString();
      else if (value % 1 != 0)
        return value.ToString("0.0");

      return value.ToString();
    }


    /// <summary>
    /// Đổi cách hiển thị tiền tệ
    /// </summary>
    public static string ConvertCurrency(double money)
    {
      if (money == 0)
        return "0";
      if (money >= 1000 || money <= -1000)
        return money.ToString("0,0");
      else if (money % 1 == 0)
        return money.ToString();
      else if (money != 0)
        return money.ToString();
      return "0";
    }

    /// <summary>
    /// Đổi cách hiển thị tiền tệ bao gồm số thập phân
    /// </summary>
    public static string ConvertCurrencyDouble(double money)
    {
      if (money == 0)
        return "0";
      if (money >= 1000 || money <= -1000)
      {
        var numberArray = money.ToString().Split(".");
        if (numberArray.Length == 2)
          return Convert.ToDouble(numberArray[0]).ToString("0,0") + "." + numberArray[1];
        else if (numberArray.Length == 1)
          return money.ToString("0,0");

        return money.ToString("0,0");
      }
      else if (money % 1 == 0)
        return money.ToString();
      else if (money != 0)
        return money.ToString();
      return "0";
    }


    /// <summary>
    /// Đổi cách hiển thị số thành rút gọn
    /// </summary>
    public static string ConvertNumber(double number)
    {
      string str = string.Empty;
      if (number != 0)
      {
        bool isNegative = number < 0;
        number = isNegative ? -number : number;

        if (number >= 1000000000)
        {
          str = String.Format(" {0:0,0.0}", number / 1000000000);
          str = str.Replace(" 0", "").Replace(".0", "");
          str += " tỷ";
        }
        else if (number >= 1000000)
        {
          str = String.Format(" {0:0,0.0}", number / 1000000);
          str = str.Replace(" 0", "").Replace(".0", "");
          str += " triệu";
        }
        else if (number >= 1000)
        {
          str = String.Format(" {0:0,0}", number / 1000);
          str = str.Replace(" 0", "");
          str += " nghìn";
        }
        else if (number % 1 == 0)
        {
          str = number.ToString();
        }
        else
        {
          str = String.Format("{0:0.0}", number);
        }

        str = isNegative ? "-" + str : str;
      }
      else
      {
        str = "0";
      }

      return str.Trim();
    }


    /// <summary>
    /// Chuyển List<string> thành string
    /// </summary>
    public static string ListToString(List<string> list)
    {
      if (list != null)
        return String.Join(", ", list.ToArray());
      else
        return string.Empty;
    }


    /// <summary>
    /// Kiểm tra nội dung có chưa từ khóa không ?
    /// </summary>
    public static bool SearchKeyword(string keyword, string content)
    {
      if (!string.IsNullOrEmpty(keyword))
      {
        content = content.ToLower();
        var keyChar = keyword.ToLower().Trim().Split(' ');
        for (int i = 0; i < keyChar.Length; i++)
        {
          if (!content.Contains(keyChar[i]))
            return false;
        }

        return true;
      }
      else
        return true;
    }


    /// <summary>
    /// Tối ưu link youtube
    /// </summary>
    /// <param name="link"></param>
    public static string VideoLink(string link)
    {
      if (string.IsNullOrEmpty(link))
        return string.Empty;

      if (link.Contains("/watch?v="))
        link = link.Replace("/watch?v=", "/embed/");
      if (link.Contains("&"))
        link = link.Substring(0, link.IndexOf("&"));
      if (link.Contains("drive.google.com"))
        link = link.Replace("/view", "/preview");

      return link;
    }


    /// <summary>
    /// Nhận diện link trong text
    /// </summary>
    public static string GetLinks(string text)
    {
      if (string.IsNullOrEmpty(text))
        return string.Empty;

      string content = text;
      content = content.Replace("<input", "&lt;input");

      List<string> links = new List<string>();
      Regex urlRx = new Regex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*",
        RegexOptions.IgnoreCase);

      MatchCollection matches = urlRx.Matches(text);
      foreach (Match match in matches)
      {
        links.Add(match.Value);
      }

      links = links.Distinct().ToList();

      foreach (var item in links)
        content = content.Replace(item, "<a href=\"" + item + "\" target=\"_blank\">" + item + "</a>");

      content = content.Replace("\n", "<br>");

      return content;
    }


    /// <summary>
    /// Chuyển TEXT thành HTML
    /// </summary>
    public static string TextToHtml(string text)
    {
      return GetLinks(text);
    }


    /// <summary>
    /// Chuyển HTML thành TEXT
    /// </summary>
    public static string HtmlToText(string html)
    {
      if (string.IsNullOrEmpty(html))
        return string.Empty;

      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(html);

      return htmlDoc.DocumentNode.InnerText;
    }


    /// <summary>
    /// Dữ liệu màu sắc
    /// </summary>
    public static List<string> ColorList()
    {
      return new List<string> {
        "#DC787E", "#F59E6C", "#986CF5", "#6CB4F5", "#F5D76C",
        "#485fc7", "#3e8ed0", "#48c78e", "#ffe08a", "#f14668",
        "#FF6633", "#FFB399", "#FF33FF", "#FFFF99", "#00B3E6",
        "#E6B333", "#3366E6", "#999966", "#99FF99", "#B34D4D",
        "#80B300", "#809900", "#E6B3B3", "#6680B3", "#66991A",
        "#FF99E6", "#CCFF1A", "#FF1A66", "#E6331A", "#33FFCC",
        "#66994D", "#B366CC", "#4D8000", "#B33300", "#CC80CC",
        "#66664D", "#991AFF", "#E666FF", "#4DB3FF", "#1AB399",
        "#E666B3", "#33991A", "#CC9999", "#B3B31A", "#00E680",
        "#4D8066", "#809980", "#E6FF80", "#1AFF33", "#999933",
        "#FF3380", "#CCCC00", "#66E64D", "#4D80CC", "#9900B3",
        "#E64D66", "#4DB380", "#FF4D4D", "#99E6E6", "#6666FF" };
    }


    /// <summary>
    /// Dữ liệu màu sắc
    /// </summary>
    public static string ColorRandom(int index)
    {
      var list = ColorList();

      if (index > 0 && index >= list.Count)
        index = index % list.Count;

      index = RandomInt(0, list.Count - 1);
      return list[index];
    }


    #endregion


    #region Các hàm liên quan đến số


    private static readonly Random random = new Random();

    /// <summary>
    /// Tạo một số ngẫu nhiên
    /// </summary>
    public static int RandomInt(int min, int max)
    {
      return random.Next(min, max);
    }

    /// <summary>
    /// Chuyển string thành số thập phân
    /// </summary>
    public static double ConvertToDouble(string text)
    {
      if ((0.1).ToString().Contains("."))
        return Convert.ToDouble(text.Replace(",", "."));
      else
        return Convert.ToDouble(text.Replace(".", ","));
    }

    /// <summary>
    /// Tính % tiến độ
    /// </summary>
    public static double Progress(double result, double target)
    {
      if (result > 0 && target > 0)
      {
        double progress = result * 100 / target;
        return progress > 100 ? 100 : progress;
      }
      else
        return 0;
    }

    /// <summary>
    /// Màu sắc của thanh phần trăm
    /// </summary>
    public static string ProgressColor(double progress)
    {
      string color = "is-dark";
      if (progress == 0)
        color = "is-dark";
      else if (progress < 25)
        color = "is-danger";
      else if (progress < 50)
        color = "is-warning";
      else if (progress < 75)
        color = "is-link";
      else if (progress >= 75)
        color = "is-success";

      return color;
    }

    /// <summary>
    /// Tính số sao đánh giá
    /// </summary>
    public static double Review(double point, int count)
    {
      if (count > 0)
        return point / count;
      else
        return 0;
    }

    /// <summary>
    /// Hàm tính phân trang
    /// </summary>
    public static int Paging(int total, int size)
    {
      if (total <= size)
        return 0;
      int col = total / size;
      float tp = total / (float)size;
      if (total % size != 0 && tp > (col))
        col = total / size + 1;
      else
        col = total / size;
      return col;
    }

    public static int ConvertToInt(string value)
    {
      if (string.IsNullOrEmpty(value))
        return 0;
      value = value.Replace(",", "").Replace(".", "").Trim();
      if (Int32.TryParse(value, out int parse))
        return parse;
      else
        return 0;
    }


    #endregion


    #region Các hàm liên quan tới thời gian


    /// <summary>
    /// Chuyển mốc thời gian trong ngày về đầu ngày
    /// </summary>
    public static DateTime DateToDay(DateTime date)
    {
      return Convert.ToDateTime(date.ToString("yyyy-MM-dd"));
    }


    /// <summary>
    /// Chuyển mốc thời gian trong ngày về đầu tháng
    /// </summary>
    public static DateTime DateToMonth(DateTime date)
    {
      return Convert.ToDateTime(date.ToString("yyyy-MM-01"));
    }

    public static string FormatMonth(string month)
    {
      DateTime date = DateTime.ParseExact(month, "M/d", null);

      return "Tháng " + date.ToString("M/yyyy");
    }

    /// <summary>
    /// Tính số phút chênh lệnh giũa 2 mốc thời gian
    /// </summary>
    /// <returns>Lớn 0: date2 > datet1</returns>
    public static long CompareTime(DateTime date1, DateTime date2)
    {
      TimeSpan spanMe = date2.Subtract(date1);
      return Convert.ToInt64(spanMe.TotalMinutes);
    }


    /// <summary>
    /// Đổi cách hiển thị thời gian
    /// </summary>
    public static string ConvertDate(DateTime? date)
    {
      DateTime DateTimeNow = DateTime.Now;
      string postTime = string.Empty;
      if (date != null)
      {
        if (DateTime.Compare(date.Value, DateTimeNow) <= 0)
        {
          TimeSpan spanMe = DateTimeNow.Subtract(date.Value);
          if (spanMe.Days < 1)
          {
            if (spanMe.Hours < 1)
            {
              if (spanMe.Minutes < 1)
              {
                if (spanMe.Seconds < 5)
                  postTime = "vừa xong";
                else
                  postTime = spanMe.Seconds + " giây trước";
              }
              else
                postTime = spanMe.Minutes + " phút trước";
            }
            else
              postTime = spanMe.Hours + " giờ trước";
          }
          else if (spanMe.Days < 30)
          {
            postTime = spanMe.Days + " ngày trước";
          }
          else if (spanMe.Days < 365)
          {
            postTime = (System.Convert.ToInt32(spanMe.Days / 30)) + " tháng trước";
          }
          else if (spanMe.Days > 365)
          {
            postTime = (System.Convert.ToInt32(spanMe.Days / 365)) + " năm trước";
          }
        }
        else
        {
          TimeSpan spanMe = date.Value.Subtract(DateTimeNow);
          if (spanMe.Days < 1)
          {
            if (spanMe.Hours < 1)
            {
              if (spanMe.Minutes < 1)
              {
                if (spanMe.Seconds < 5)
                  postTime = "bây giờ";
                else
                  postTime = spanMe.Seconds + " giây nữa";
              }
              else
                postTime = spanMe.Minutes + " phút nữa";
            }
            else
              postTime = spanMe.Hours + " giờ nữa";
          }
          else if (spanMe.Days < 30)
          {
            postTime = spanMe.Days + " ngày nữa";
          }
          else if (spanMe.Days < 365)
          {
            postTime = (System.Convert.ToInt32(spanMe.Days / 30)) + " tháng nữa";
          }
          else if (spanMe.Days > 365)
          {
            postTime = (System.Convert.ToInt32(spanMe.Days / 365)) + " năm nữa";
          }
        }
      }

      return postTime;
    }

    /// <summary>
    /// Chuyển thời gian thành thứ trong tuần bằng tiếng Việt
    /// </summary>
    public static string ConvertWeekdays(DateTime value, bool isFull = false)
    {
      var date = value.ToString("ddd");

      if (date.Contains("Mon"))
        return date.Replace("Mon", isFull ? "Thứ 2" : "T2");
      else if (date.Contains("Tue"))
        return date.Replace("Tue", isFull ? "Thứ 3" : "T3");
      else if (date.Contains("Wed"))
        return date.Replace("Wed", isFull ? "Thứ 4" : "T4");
      else if (date.Contains("Thu"))
        return date.Replace("Thu", isFull ? "Thứ 5" : "T5");
      else if (date.Contains("Fri"))
        return date.Replace("Fri", isFull ? "Thứ 6" : "T6");
      else if (date.Contains("Sat"))
        return date.Replace("Sat", isFull ? "Thứ 7" : "T7");
      else
        return date.Replace("Sun", "CN");
    }
    /// <summary>
    /// Chuyển long thành thứ trong tuần bằng tiếng Việt
    /// </summary>

    public static string ConvertLongToDate(long tickValue)
    {
      DateTime dateTime = new DateTime(tickValue);
      string dayOfWeek = dateTime.DayOfWeek.ToString();

      if (dayOfWeek == "Sunday")
      {
        dayOfWeek = "Chủ nhật";
      }
      else
      {
        switch (dayOfWeek)
        {
          case "Monday":
            dayOfWeek = "Thứ 2";
            break;
          case "Tuesday":
            dayOfWeek = "Thứ 3";
            break;
          case "Wednesday":
            dayOfWeek = "Thứ 4";
            break;
          case "Thursday":
            dayOfWeek = "Thứ 5";
            break;
          case "Friday":
            dayOfWeek = "Thứ 6";
            break;
          case "Saturday":
            dayOfWeek = "Thứ 7";
            break;
        }
      }

      string formattedDate = dateTime.ToString("dd/MM/yyyy");
      return dayOfWeek + " - " + formattedDate;
    }

    /// <summary>
    /// Đổi cách hiển thị thời gian, có tuần
    /// </summary>
    public static string ConvertDateWeek(long tick)
    {
      var value = new DateTime(tick);
      return string.Format("{0} - {1:dd/MM/yyyy}", ConvertWeekdays(value), value);
    }

    public static string GetTimeName(int type)
    {
      if (type == 1)
        return "Tuần này";
      else if (type == 2)
        return "Tháng này";
      else if (type == 3)
        return "Quý này";
      else if (type == 7)
        return "Quý trước";
      else if (type == 4)
        return "30 ngày gần đây";
      else if (type == 5)
        return "3 tháng gần đây";
      else if (type == 6)
        return "6 tháng gần đây";
      else if (type == 11)
        return "Tuần trước";
      else if (type == 22)
        return "Tháng trước";

      return "";
    }

    /// <summary>
    /// Lấy khoản thời gian
    /// 1. Tuần này
    /// 2. Tháng này
    /// 3. Quý này
    /// 7. Quý trước
    /// 4. 30 ngày gần đây
    /// 5. 3 tháng gần đây
    /// 6. 6 tháng gần đây
    /// 11. Tuần trước
    /// 22. Tháng trước
    /// </summary>
    public static void GetTimeSpan(int type, out DateTime start, out DateTime end)
    {
      var date = DateTime.Today;
      start = date;
      end = date;

      if (type == 1)
      {
        if (date.DayOfWeek == DayOfWeek.Tuesday)
          start = date.AddDays(-1);
        else if (date.DayOfWeek == DayOfWeek.Wednesday)
          start = date.AddDays(-2);
        else if (date.DayOfWeek == DayOfWeek.Thursday)
          start = date.AddDays(-3);
        else if (date.DayOfWeek == DayOfWeek.Friday)
          start = date.AddDays(-4);
        else if (date.DayOfWeek == DayOfWeek.Saturday)
          start = date.AddDays(-5);
        else if (date.DayOfWeek == DayOfWeek.Sunday)
          start = date.AddDays(-6);
        end = start.AddDays(6);
      }
      else if (type == 11)
      {
        GetTimeSpan(1, out start, out end);
        start = start.AddDays(-7);
        end = end.AddDays(-7);
      }
      else if (type == 2)
      {
        start = Convert.ToDateTime(string.Format("{0:yyyy-MM-01}", DateTime.Today));
        end = start.AddMonths(1).AddDays(-1);
      }
      else if (type == 22)
      {
        start = Convert.ToDateTime(string.Format("{0:yyyy-MM-01}", DateTime.Today)).AddMonths(-1);
        end = start.AddMonths(1).AddDays(-1);
      }
      else if (type == 3)
      {
        if (date.Month <= 3)
          start = Convert.ToDateTime(string.Format("{0:yyyy-01-01}", date));
        else if (date.Month <= 6)
          start = Convert.ToDateTime(string.Format("{0:yyyy-04-01}", date));
        else if (date.Month <= 9)
          start = Convert.ToDateTime(string.Format("{0:yyyy-07-01}", date));
        else
          start = Convert.ToDateTime(string.Format("{0:yyyy-10-01}", date));
        end = start.AddMonths(3).AddDays(-1);
      }
      else if (type == 4)
      {
        start = date.AddDays(-30);
        end = date;
      }
      else if (type == 5)
      {
        start = Convert.ToDateTime(string.Format("{0:yyyy-MM-01}", DateTime.Today)).AddMonths(-2);
        end = start.AddMonths(3).AddDays(-1);
      }
      else if (type == 6)
      {
        start = Convert.ToDateTime(string.Format("{0:yyyy-MM-01}", DateTime.Today)).AddMonths(-5);
        end = start.AddMonths(6).AddDays(-1);
      }
      else if (type == 7)
      {
        if (date.Month <= 3)
          start = Convert.ToDateTime(string.Format("{0:yyyy-10-01}", date.AddYears(-1)));
        else if (date.Month <= 6)
          start = Convert.ToDateTime(string.Format("{0:yyyy-01-01}", date));
        else if (date.Month <= 9)
          start = Convert.ToDateTime(string.Format("{0:yyyy-04-01}", date));
        else
          start = Convert.ToDateTime(string.Format("{0:yyyy-07-01}", date));
        end = start.AddMonths(3).AddDays(-1);
      }
    }

    /// <summary>
    /// Lấy khoản thời gian
    /// 1. Theo tuần
    /// 2. Theo tháng
    /// 3. Theo quý
    /// 8. Theo năm
    /// </summary>
    public static void GetTimeSpanForm(int type, DateTime date, out DateTime start, out DateTime end)
    {
      start = date;
      end = date;

      if (type == 1)
      {
        if (date.DayOfWeek == DayOfWeek.Tuesday)
          start = date.AddDays(-1);
        else if (date.DayOfWeek == DayOfWeek.Wednesday)
          start = date.AddDays(-2);
        else if (date.DayOfWeek == DayOfWeek.Thursday)
          start = date.AddDays(-3);
        else if (date.DayOfWeek == DayOfWeek.Friday)
          start = date.AddDays(-4);
        else if (date.DayOfWeek == DayOfWeek.Saturday)
          start = date.AddDays(-5);
        else if (date.DayOfWeek == DayOfWeek.Sunday)
          start = date.AddDays(-6);
        end = start.AddDays(6);
      }
      else if (type == 2)
      {
        start = Convert.ToDateTime(string.Format("{0:yyyy-MM-01}", start.Date));
        end = start.AddMonths(1).AddDays(-1);
      }
      else if (type == 3)
      {
        if (date.Month <= 3)
          start = Convert.ToDateTime(string.Format("{0:yyyy-01-01}", date));
        else if (date.Month <= 6)
          start = Convert.ToDateTime(string.Format("{0:yyyy-04-01}", date));
        else if (date.Month <= 9)
          start = Convert.ToDateTime(string.Format("{0:yyyy-07-01}", date));
        else
          start = Convert.ToDateTime(string.Format("{0:yyyy-10-01}", date));
        end = start.AddMonths(3).AddDays(-1);
      }
      else if (type == 8)
      {
        start = new DateTime(start.Year, 1, 1);
        end = new DateTime(start.Year, 12, 31);
      }
    }


    /// <summary>
    /// Số ngày trong tháng
    /// </summary>
    public static List<List<long>> CalenderMonth(DateTime date)
    {
      var start = Convert.ToDateTime(string.Format("{0:yyyy-MM-01}", date));
      var end = start.AddMonths(1).AddDays(-1);

      // Lấy số ngày trong tuần nhưng của tháng trước
      int before = 0;
      if (start.DayOfWeek == DayOfWeek.Tuesday)
        before = 1;
      else if (start.DayOfWeek == DayOfWeek.Wednesday)
        before = 2;
      else if (start.DayOfWeek == DayOfWeek.Thursday)
        before = 3;
      else if (start.DayOfWeek == DayOfWeek.Friday)
        before = 4;
      else if (start.DayOfWeek == DayOfWeek.Saturday)
        before = 5;
      else if (start.DayOfWeek == DayOfWeek.Sunday)
        before = 6;

      // Danh sách ngày
      var list = new List<long>();
      // Ngày của tháng trước
      for (DateTime d = start.AddDays(-before); d < start; d = d.AddDays(1))
        list.Add(-d.Ticks);
      // Ngày của tháng này
      for (DateTime d = start; d <= end; d = d.AddDays(1))
        list.Add(d.Ticks);

      var results = new List<List<long>>();

      int day = 0;
      while (day < list.Count)
      {
        var week = new List<long>();
        for (int i = 0; i < 7; i++)
        {
          week.Add(list[day]);
          day++;

          if (day == list.Count)
          {
            var after = end.AddDays(1);
            while (i + 1 < 7)
            {
              week.Add(-after.Ticks);
              after = after.AddDays(1);
              i++;
            }
            break;
          }
        }
        results.Add(week);
      }

      return results;
    }


    /// <summary>
    /// Mốc giờ
    /// </summary>
    public static List<StaticModel> TimeList(int min, int max)
    {
      var list = new List<StaticModel>();

      for (int i = min; i <= max; i++)
      {
        for (int m = 0; m < 60; m += 5)
        {
          list.Add(new StaticModel
          {
            id = i * 10 + m,
            name = string.Format("{0:00}:{1:00}", i, m),
          });
        }
      }

      return list;
    }

    /// <summary>
    /// Mốc giờ: mặc định từ 7 đến 23 giờ
    /// </summary>
    public static List<StaticModel> TimeList()
    {
      return TimeList(7, 23);
    }

    /// <summary>
    /// Buổi trong ngày
    /// </summary>
    public static string DateSession()
    {
      var time = "sáng";
      if (DateTime.Now.Hour > 18)
        time = "tối";
      else if (DateTime.Now.Hour > 12)
        time = "chiều";
      return time;
    }

    /// <summary>
    /// Buổi trong ngày + icon
    /// </summary>
    public static StaticModel DateSessionHaveIcon()
    {
      var time = new StaticModel { name = "sáng", icon = "wb_sunny" };

      if (DateTime.Now.Hour > 18)
      {
        time.name = "tối";
        time.icon = "nightlight_round";
      }

      else if (DateTime.Now.Hour > 12)
      {
        time.name = "chiều";
        time.icon = "wb_cloudy";
      }
      return time;
    }


    public static string MinutesToHours(double minutes)
    {
      var time = TimeSpan.FromMinutes(minutes);
      return $"{(int)time.TotalHours} giờ {(int)time.Minutes} phút";
    }

    public static string MinutesToHours2(double minutes, out int minutesF, out int hourF)
    {
      var time = TimeSpan.FromMinutes(minutes);
      minutesF = (int)time.Minutes;
      hourF = (int)time.TotalHours;
      return $"{hourF} giờ {minutesF} phút";
    }

    /// <summary>
    /// Hàm chuyển vị trí thành thứ
    /// </summary>
    /// <param name="posstion"></param>
    /// <returns></returns>
    public static string ConvertPosToWeekdays(int posstion)
    {
      string dayOfWeek = posstion switch
      {
        2 => "T2",
        3 => "T3",
        4 => "T4",
        5 => "T5",
        6 => "T6",
        7 => "T7",
        _ => "CN",
      };
      return dayOfWeek;
    }



    #endregion


    #region Các hàm khác


    public static bool DeviceMobile(string userAgent)
    {
      //Kiểm tra xem trình duyệt hiện tại là mobile hay ko?
      Regex mobile_b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      Regex mobile_v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      bool isMobile = (mobile_b.IsMatch(userAgent) || mobile_v.IsMatch(userAgent.Substring(0, 4))) ? true : false;

      return isMobile;
    }

    private static double Deg2rad(double deg)
    {
      return deg * (Math.PI / 180);
    }

    /// <summary>Lấy khoản cách giữa 2 điểm trả về mét</summary>
    public static long GetDistanceFromLatLon(double lat1, double lon1, double lat2, double lon2)
    {
      var R = 6378137;
      var dLat = Deg2rad(lat2 - lat1);
      var dLon = Deg2rad(lon2 - lon1);
      var a =
        Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
      var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      var d = R * c;

      return Convert.ToInt64(d);
    }

    #endregion


    #region Các hàm liên quan đến Object

    public static List<T> CloneList<T>(List<T> self)
    {
      var result = new List<T>();

      foreach (var item in self)
      {
        result.Add(Clone(item));
      }

      return result;
    }

    public static T Clone<T>(T self)
    {
      var serialized = JsonConvert.SerializeObject(self);
      return JsonConvert.DeserializeObject<T>(serialized);
    }

    public static void PrintObject(Object obj)
    {
      Type t = obj.GetType();
      Console.WriteLine("Type is: {0}", t.Name);
      PropertyInfo[] props = t.GetProperties();
      foreach (var prop in props)
      {
        if (prop.GetIndexParameters().Length == 0)
          Console.WriteLine("{0, 20} = {2, -40} ({1})", prop.Name, prop.PropertyType.Name, prop.GetValue(obj));
      }
    }

    #endregion
  }
}