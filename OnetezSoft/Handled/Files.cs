﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OnetezSoft.Models;
using Excel;
using System.Text;
using OnetezSoft.Data;
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

      if (isMacOS) filePath = filePath.Replace("\\", "/");

      if (!Directory.Exists(filePath))
        Directory.CreateDirectory(filePath);

      string format = new FileInfo(fileName.ToLower()).Extension;

      string rename = Mongo.RandomId() + format;

      string path = Path.Combine(filePath, rename);

      await using FileStream fs = new(path, FileMode.Create);
      await inputStream.CopyToAsync(fs);
      fs.Dispose();
      inputStream.Dispose();

      //if (format == ".png" || format == ".gif" || format == ".jpg" || format == ".jpeg")
      //  ResizeImage(path);

      return $"/{folder.Replace("\\", "/")}/{rename}";
    }


    /// <summary>
    /// Lấy định đạng file
    /// </summary>
    /// <returns>.jpg, .png...</returns>
    public static string FileExtension(string fileName)
    {
      return Path.GetExtension(fileName);
    }


    /// <summary>
    /// Lấy định đạng file
    /// </summary>
    /// <returns>.jpg, .png...</returns>
    public static string FileName(string link)
    {
      var filePath = Environment.CurrentDirectory + "\\wwwroot" + link.Replace("/", "\\");

      if (isMacOS) filePath = filePath.Replace("\\", "/");

      if (File.Exists(filePath))
      {
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Name;
      }
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
    public static void DeleteFile(string fileLink)
    {
      if (!fileLink.StartsWith("/upload"))
        return;

      string filePath = Environment.CurrentDirectory + "\\wwwroot" + fileLink.Replace("/", "\\");

      if (isMacOS) filePath = filePath.Replace("\\", "/");

      new Task(() =>
      {
        try
        {
          if (File.Exists(filePath))
            File.Delete(filePath);

          string thumbPath = filePath.Replace("\\upload\\", "\\thumb\\");
          if (File.Exists(thumbPath))
            File.Delete(thumbPath);
        }
        catch (Exception)
        {

        }
      }).Start();
    }


    /// <summary>
    /// Hàm xóa nhiều file
    /// </summary>
    public static void DeleteMultiFile(List<ImageModel> images)
    {
      if (images == null)
        return;

      foreach (var item in images)
        DeleteFile(item.link);
    }


    /// <summary>
    /// Xóa folder
    /// </summary>
    public static void DeleteFoder(string folder)
    {
      Directory.Delete(folder, true);
    }


    /// <summary>
    /// Đọc file Excel
    /// </summary>
    public static List<List<string>> ReadExcel(string link, out string error)
    {
      try
      {
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
              error = "row error";
              return null;
            }
          }

          //error = worksheet.NumberOfColumns.ToString();
          error = string.Empty;

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
  }
}
