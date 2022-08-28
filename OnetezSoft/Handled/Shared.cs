using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OnetezSoft.Models;
using Newtonsoft.Json;

namespace OnetezSoft.Handled
{
  public class Shared
  {
    #region Các hàm liên quan đến String


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
    /// Đổi cách hiển thị tiền tệ
    /// </summary>
    public static string ConvertCurrency(double money)
    {
      if(money == 0)
        return "0";
      if (money >= 1000 || money <= -1000)
        return money.ToString("0,0");
      else if(money % 1 == 0)
        return money.ToString();
      else if(money != 0)
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
          str += "B";
        }
        else if (number >= 1000000)
        {
          str = String.Format(" {0:0,0.0}", number / 1000000);
          str = str.Replace(" 0", "").Replace(".0", "");
          str += "M";
        }
        else if (number >= 1000)
        {
          str = String.Format(" {0:0,0}", number / 1000);
          str = str.Replace(" 0", "");
          str += "K";
        }
        else if(number % 1 == 0)
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
      Regex urlRx = new Regex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);

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
      if (index > 0)
      {
        if (index >= list.Count)
          index = index % list.Count;
      }
      else
      {
        index = RandomInt(0, list.Count);
      }
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
      if(progress == 0)
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


    #endregion


    #region Các hàm liên quan tới thời gian


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
    /// Đổi cách hiển thị thời gian, có tuần
    /// </summary>
    public static string ConvertDateWeek(long tick)
    {
      var date = new DateTime(tick).ToString("ddd - dd/MM/yyyy");

      if (date.Contains("Mon"))
        return date.Replace("Mon", "T2");
      else if (date.Contains("Tue"))
        return date.Replace("Tue", "T3");
      else if (date.Contains("Wed"))
        return date.Replace("Wed", "T4");
      else if (date.Contains("Thu"))
        return date.Replace("Thu", "T5");
      else if (date.Contains("Fri"))
        return date.Replace("Fri", "T6");
      else if (date.Contains("Sat"))
        return date.Replace("Sat", "T7");
      else
        return date.Replace("Sun", "CN");
    }


    /// <summary>
    /// Lấy khoản thời gian
    /// 1. Tuần này
    /// 2. Tháng này
    /// 3. Quý này
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
      else if(type == 11)
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
    }

    /// <summary>
    /// Mốc giờ
    /// </summary>
    public static List<StaticModel> TimeList()
    {
      var list = new List<StaticModel>();

      for (int i = 7; i < 24; i++)
      {
        for (int m = 0; m < 60; m += 10)
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
    /// Buổi trang này
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


    #endregion


    #region Các hàm liên quan đến Object

    public static T Clone<T>(T self)
    {
      var serialized = JsonConvert.SerializeObject(self);
      return JsonConvert.DeserializeObject<T>(serialized);
    }

    #endregion
  }
}
