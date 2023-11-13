using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OnetezSoft.Handled
{
  public static class StringHelper
  {
    private static readonly string[] VietnameseSigns = new string[]
     {
      "aAeEoOuUiIdDyY",
      "áàạảãâấầậẩẫăắằặẳẵ",
      "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
      "éèẹẻẽêếềệểễ",
      "ÉÈẸẺẼÊẾỀỆỂỄ",
      "óòọỏõôốồộổỗơớờợởỡ",
      "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
      "úùụủũưứừựửữ",
      "ÚÙỤỦŨƯỨỪỰỬỮ",
      "íìịỉĩ",
      "ÍÌỊỈĨ",
      "đ",
      "Đ",
      "ýỳỵỷỹ",
      "ÝỲỴỶỸ"
     };


    /// <summary>
    /// Loại bỏ ký tự đặt biệt, không xóa dấu cách
    /// </summary>
    private static string RemoveSpecial(string text)
    {
      //Loại bỏ ký tự đặc biệt
      //text = text.Replace("-", ""); Không bỏ ký tự -
      text = text.Replace("?", "");
      text = text.Replace("'", "");
      text = text.Replace("/", "");
      text = text.Replace("\"", "");
      text = text.Replace("’", "");
      text = text.Replace(",", "");
      text = text.Replace(":", "");
      text = text.Replace(";", "");
      text = text.Replace("!", "");
      text = text.Replace(".", "");
      text = text.Replace("~", "");
      text = text.Replace("`", "");
      text = text.Replace("#", "");
      text = text.Replace("@", "");
      text = text.Replace("$", "");
      text = text.Replace("®", "");
      text = text.Replace("%", "");
      text = text.Replace("^", "");
      text = text.Replace("&", "");
      text = text.Replace("*", "");
      text = text.Replace("(", "");
      text = text.Replace(")", "");
      text = text.Replace("<", "");
      text = text.Replace(">", "");
      text = text.Replace("[", "");
      text = text.Replace("]", "");
      text = text.Replace("{", "");
      text = text.Replace("}", "");
      text = text.Replace("=", "");
      text = text.Replace("–", "");
      text = text.Replace("+", "");
      text = text.Replace("|", "");
      return text.Trim();
    }


    /// <summary>
    /// Chuyển chuỗi tiếng Việt có dấu thành chuỗi không dấu, bỏ ký tự đặt biệt
    /// </summary>
    public static string VnNoSigns(string text)
    {
      if (!string.IsNullOrEmpty(text))
      {
        // Chuyển thành chuỗi không dấu
        for (int i = 1; i < VietnameseSigns.Length; i++)
        {
          for (int j = 0; j < VietnameseSigns[i].Length; j++)
            text = text.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
        }

        // Loại bỏ ký tự đặc biệt
        text = RemoveSpecial(text);

        // Chuyển UTF8 thành ASCII
        text = ConvertASCII(text);

        // Xóa 2 dấu cách liền kề
        while (text.Contains("  "))
          text = text.Replace("  ", " ");

        return text.Trim();
      }
      else
        return string.Empty;
    }


    /// <summary>
    /// Đổi tên file thành tiếng việt không dấu
    /// </summary>
    public static string RenameFile(string text)
    {
      if (!string.IsNullOrEmpty(text))
      {
        // Chuyển thành chuỗi không dấu
        for (int i = 1; i < VietnameseSigns.Length; i++)
        {
          for (int j = 0; j < VietnameseSigns[i].Length; j++)
            text = text.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
        }

        // Chuyển UTF8 thành ASCII
        text = ConvertASCII(text);

        // Xóa dấu cách
        text = text.Replace(" ", "-");

        // Xóa 2 dấu - liền kề
        while (text.Contains("--"))
          text = text.Replace("--", "-");

        return text.Trim().ToLower();
      }
      else
        return string.Empty;
    }

    /// <summary>
    /// Chuyển UTF8 thành ASCII
    /// </summary>
    private static string ConvertASCII(string text)
    {
      byte[] strBytes = Encoding.UTF8.GetBytes(text);

      byte[] asciiBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, strBytes);

      string asciiStr = Encoding.ASCII.GetString(asciiBytes);

      if (asciiStr.Contains("?"))
        return asciiStr.Replace("?", "");
      else
        return text;
    }


    /// <summary>
    /// Kiểm tra dữ liệu rỗng
    /// </summary>
    public static bool IsEmpty(this string source)
    {
      if (string.IsNullOrEmpty(source))
        return string.IsNullOrEmpty(source);
      else
        return string.IsNullOrEmpty(source.Trim());
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
    /// String to MD5
    /// </summary>
    public static string CreateMD5(string input)
    {
      // Use input string to calculate MD5 hash
      using (MD5 md5 = MD5.Create())
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
    /// Chuyển TEXT thành HTML
    /// </summary>
    public static string TextToHtml(string text)
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
    /// Chuyển Text thành viết hoa chữ cái đầu trong từ
    /// </summary>
    public static string TextToCase(string text)
    {
      if (string.IsNullOrEmpty(text))
        return string.Empty;

      // Xóa 2 dấu cách liền kề
      while (text.Contains("  "))
        text = text.Replace("  ", " ");

      return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
    }
  }

}

