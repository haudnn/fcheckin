using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OnetezSoft.Data;
using SkiaSharp;
using Excel;
using ClosedXML.Excel;

namespace OnetezSoft.Handled
{
  public class Files
  {
    private static bool isMacOS = Environment.CurrentDirectory.Contains("/");

    /// <summary>
    /// Lưu file vào hosting
    /// </summary>
    public static async Task<string> SaveFileAsync(StreamContent inputStream, string fileName)
    {
      string folder = "upload\\" + string.Format("{0:yyMMdd}", DateTime.Now);
      string filePath = Environment.CurrentDirectory + "\\wwwroot\\" + folder;

      if (isMacOS)
        filePath = filePath.Replace("\\", "/");

      if (!Directory.Exists(filePath))
        Directory.CreateDirectory(filePath);

      string rename = Mongo.RandomId() + "_" + StringHelper.RenameFile(fileName);

      string fullPath = Path.Combine(filePath, rename);

      await using FileStream fs = new(fullPath, FileMode.Create);
      await inputStream.CopyToAsync(fs);
      fs.Dispose();
      inputStream.Dispose();

      string result = $"/{folder.Replace("\\", "/")}/{rename}";

      Console.WriteLine($"Upload file to: {result}");

      return result;
    }

    /// <summary>
    /// Lưu file vào hosting và resize
    /// </summary>
    /// <returns>Trả về link hình</returns> 
    public static async Task<string> UploadFile(StreamContent inputStream, string fileName, int size)
    {
      string link = await SaveFileAsync(inputStream, fileName);

      // Không cần rezize
      if(size == 0)
        return link;

      // Resize hình ảnh
      return ResizeImage(link, size);
    }


    /// <summary>
    /// Thay đổi Kích thước hình ảnh
    /// </summary>
    /// <returns>Trả về link hình</returns>
    public static string ResizeImage(string link, int size)
    {
      // Chất lượng file
      int quality = 100;
      // Nơi chưa file temp
      var inputPath = string.Empty;
      var tempPath = string.Empty;
      if (isMacOS)
      {
        inputPath = Environment.CurrentDirectory + "/wwwroot" + link;
        tempPath = inputPath.Replace("/wwwroot/upload", "/wwwroot/upload/temp");
      }
      else
      {
        inputPath = Environment.CurrentDirectory + "\\wwwroot" + link;
        tempPath = inputPath.Replace("\\wwwroot\\upload", "\\wwwroot\\upload\\temp");
      }
      // Lấy thông tin file
      var fileInfo = new FileInfo(tempPath);
      var fileFormat = fileInfo.Extension;
      var folderPath = fileInfo.DirectoryName;
      var rootPath = Environment.CurrentDirectory + (isMacOS ? "/wwwroot" : "\\wwwroot");
      
      // Tạo folder temp
      if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);

      // Move file gốc qua folder temp
      File.Move(inputPath, tempPath);
      Console.WriteLine("Move: " + inputPath);
      Console.WriteLine("To  : " + tempPath);

      try
      {
        using (var fileStream = File.OpenRead(tempPath))
        {
          using (var sKStream = new SKManagedStream(fileStream))
          {
            using (var original = SKBitmap.Decode(sKStream))
            {
              int width, height;
              if (original.Width <= size && original.Height <= size)
              {
                width = original.Width;
                height = original.Height;
              }
              else if (original.Width > original.Height)
              {
                width = size;
                height = original.Height * size / original.Width;
              }
              else
              {
                height = size;
                width = original.Width * size / original.Height;
              }

              using (var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
              {
                using (var image = SKImage.FromBitmap(resized))
                {
                  using (var output = File.OpenWrite(inputPath))
                  {
                    if (fileFormat == ".png")
                      image.Encode(SKEncodedImageFormat.Png, quality).SaveTo(output);
                    else if (fileFormat == ".gif")
                      image.Encode(SKEncodedImageFormat.Gif, quality).SaveTo(output);
                    else
                      image.Encode(SKEncodedImageFormat.Jpeg, quality).SaveTo(output);
                  }
                }
              }
            }
          }
        }

        // Xóa file temp
        File.Delete(tempPath);

        string result = inputPath.Replace(rootPath, "").Replace("\\", "/");

        Console.WriteLine($"Resize file to {size}px: {result}");
        
        return result;
      }
      catch (System.Exception ex)
      {
        // Resize lỗi, chuyền file về lại
        File.Move(tempPath, inputPath);
        Console.WriteLine("Move: " + tempPath);
        Console.WriteLine("To  : " + inputPath);
        Console.WriteLine($"Resize error: {ex.Message}");

        return inputPath.Replace(rootPath, "").Replace("\\", "/"); 
      }
    }


    /// <summary>
    /// Lấy định đạng file
    /// </summary>
    /// <returns>.jpg, .png...</returns>
    public static string FileFormat(string fileName)
    {
      return Path.GetExtension(fileName);
    }

    /// <summary>
    /// Lấy tên file
    /// </summary>
    /// <returns>filename.jpg...</returns>
    public static string FileName(string link)
    {
      if (!string.IsNullOrEmpty(link))
      {
        var fileName = new FileInfo(link).Name;
        if(fileName.Contains("_"))
          return fileName.Split("_")[1];
        else
          return fileName;   
      }
      return string.Empty;
    }

    /// <summary>
    /// Lấy tên file
    /// </summary>
    /// <returns>filename.jpg...</returns>
    public static string FileName(string link, int length)
    {
      if (!string.IsNullOrEmpty(link))
      {
        var info = new FileInfo(link);
        var fileName = info.Name;
        var format = info.Extension;
        if(fileName.Contains("_"))
        {
          var name = fileName.Split("_")[1].Replace(format, "");
          if(name.Length > length)
            name = name.Substring(0, length - 2) + "..";
          return name + format;
        }
        else
          return fileName;   
      }
      return string.Empty;
    }

    /// <summary>
    /// Lấy nới chứa file
    /// </summary>
    public static string FilePath(string link)
    {
      var filePath = Environment.CurrentDirectory + "\\wwwroot" + link.Replace("/", "\\");
      if (isMacOS)
        filePath = filePath.Replace("\\", "/");

      if (File.Exists(filePath))
        return filePath;

      return string.Empty;
    }


    /// <summary>
    /// Kiểm tra định dạng file tải lên
    /// </summary>
    /// <param name="fileName">Tên file</param>
    /// <param name="fileTypes">Định dạng file: .pdf,.png,.jpg,.jpeg,.mp4,.doc,.docx,.xls,.xlsx</param>
    /// <returns></returns>
    public static bool CheckExtension(string fileName, string fileTypes)
    {
      if (string.IsNullOrEmpty(fileTypes))
        fileTypes = ".pdf,.png,.jpg,.jpeg,.mp4,.doc,.docx,.xls,.xlsx";
      string[] types = fileTypes.Split(',');
      foreach (var type in types)
      {
        if (fileName.ToLower().EndsWith(type))
          return true;
      }

      return false;
    }


    /// <summary>
    /// Hàm xóa file
    /// </summary>
    public static bool DeleteFile(string link)
    {
      if (string.IsNullOrEmpty(link))
        return false;

      try
      {
        var filePath = Environment.CurrentDirectory + "\\wwwroot" + link.Replace("/", "\\");

        if (isMacOS)
          filePath = filePath.Replace("\\", "/");

        if (File.Exists(filePath))
          File.Delete(filePath);

        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Can't delete file: {link} \n{ex.Message}");
        return false;
      }
    }

    /// <summary>
    /// Hàm xóa nhiều files
    /// </summary>
    public static void DeleteFiles(List<string> links)
    {
      foreach (var link in links)
      {
        DeleteFile(link);
      }
    }


    /// <summary>
    /// Đọc file Excel
    /// </summary>
    public static List<List<string>> ReadExcel(string link, out string error)
    {
      try
      {
        error = string.Empty;
        var results = new List<List<string>>();

        // Đọc file Excel
        string filePath = Environment.CurrentDirectory + "\\wwwroot" + link.Replace("/", "\\");

        if (isMacOS) filePath = filePath.Replace("\\", "/");

        var excelData = Workbook.Worksheets(filePath);
        if (excelData != null && excelData.Count() > 0)
        {
          var worksheet = excelData.FirstOrDefault();

          for (int r = 1; r < worksheet.Rows.Length; r++)
          {
            try
            {
              var row = worksheet.Rows[r];
              var model = new List<string>();
              for (int i = 0; i < worksheet.NumberOfColumns; i++)
              {
                var text = row.Cells[i] == null ? "" : row.Cells[i].Text.Trim();

                model.Add(text);
              }
              results.Add(model);
            }
            catch (Exception)
            {
              error = $"{r}, ";
            }
          }

          if (!string.IsNullOrEmpty(error))
            error = "Error rows: " + error;

          return results;
        }
        else
          error = "No data";
      }
      catch (Exception ex)
      {
        error = ex.Message;
      }

      return null;
    }


    /// <summary>
    /// Xuất file Excel
    /// </summary>
    public static string ExportExcel(List<List<string>> list)
    {
      // Folder lưu file
      string file = string.Format("{0:yyy-MM-dd-HH-mm}.xlsx", DateTime.Now);
      string folder = "upload\\export";
      string path = Environment.CurrentDirectory + "\\wwwroot\\" + folder;

      if (isMacOS)
        path = path.Replace("\\", "/");

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      string filePath = Path.Combine(path, file);

      try
      {
        using (var workbook = new XLWorkbook())
        {
          IXLWorksheet worksheet = workbook.Worksheets.Add("Export");
          for (int r = 0; r < list.Count; r++)
          {
            var rows = list[r];
            for (int c = 0; c < rows.Count; c++)
            {
              var col = rows[c] != null ? rows[c] : "";
              worksheet.Cell(r + 1, c + 1).Value = col;
            }
          }
          workbook.SaveAs(filePath);
        }

        return $"/{folder.Replace("\\", "/")}/{file}";
      }
      catch (System.Exception ex)
      {
        return ex.Message;
      }
    }

    /// <summary>
    /// Xuất file Excel
    /// </summary>
    public static string ExportExcel(List<List<string>> list, string nameFile)
    {
      // Folder lưu file
      string file = nameFile + ".xlsx";
      string folder = "upload\\export";
      string path = Environment.CurrentDirectory + "\\wwwroot\\" + folder;

      if (isMacOS)
        path = path.Replace("\\", "/");

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      string filePath = Path.Combine(path, file);

      try
      {
        using (var workbook = new XLWorkbook())
        {
          IXLWorksheet worksheet = workbook.Worksheets.Add("Export");
          for (int r = 0; r < list.Count; r++)
          {
            var rows = list[r];
            for (int c = 0; c < rows.Count; c++)
            {
              var col = rows[c] != null ? rows[c] : "";
              worksheet.Cell(r + 1, c + 1).Value = col;
            }
          }
          workbook.SaveAs(filePath);
        }

        return $"/{folder.Replace("\\", "/")}/{file}";
      }
      catch (System.Exception ex)
      {
        return ex.Message;
      }
    }
  }
}
