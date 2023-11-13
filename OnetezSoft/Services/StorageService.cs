using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using OnetezSoft.Data;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnetezSoft.Services;

public class StorageService
{
  private static GoogleCredential credential = null;

  private static StorageClient storageClient;

  private static readonly string bucketName = "fastdo-storage.appspot.com";

  private static readonly string bucketLink = "https://storage.googleapis.com/" + bucketName + "/";

  private static readonly string pathSecret = AppDomain.CurrentDomain.BaseDirectory + "storage.json";

  private static readonly string TELEGRAM_CHAT_ID = "-958636002";

  private static void Credential()
  {
    try
    {
      using (var stream = new FileStream(pathSecret, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        credential = GoogleCredential.FromStream(stream);
      }
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Google Credential: " + ex.ToString());
    }
  }

  private static void StorageCredential()
  {
    try
    {
      Credential();
      storageClient ??= StorageClient.Create(credential);
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Storage Credential: " + ex.ToString());
    }
  }

  public static async Task<string> UploadAsync(string fileLink, string folderName, string companyId = "", UserModel user = null, string path = "", Progress<IUploadProgress> progress = null)
  {
    try
    {
      StorageCredential();

      string filePath = Files.FilePath(fileLink);
      string fileName = Path.GetFileName(filePath);

      if (filePath.IsEmpty())
      {
        Console.WriteLine("file not exist");
        return fileLink;
      }

      FileStream fileStream = File.OpenRead(filePath);

      // https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
      string contentType = null;
      if (fileName.EndsWith(".png"))
        contentType = "image/png";
      else if (fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg"))
        contentType = "image/jpeg";
      else if (fileName.EndsWith(".gif"))
        contentType = "image/gif";
      else if (fileName.EndsWith(".txt"))
        contentType = "text/plain";
      else
      {
        string format = Files.FileFormat(fileName);
        contentType = "application/" + format.Replace(".", "");
      }

      if (!string.IsNullOrEmpty(folderName))
        fileName = folderName + "/" + fileName;

      var result = await storageClient.UploadObjectAsync(bucketName, fileName, contentType, fileStream, null, default, progress);
      Console.WriteLine($"Storage Uploaded: {fileName}.");

      if (!string.IsNullOrEmpty(companyId) && result != null)
      {
        // tạo dữ liệu lưu file bên local
        var fileCreate = new FileModel()
        {
          link = bucketLink + result.Name,
          name = Files.FileName(result.Name.Replace(companyId + "/", "")),
          format = result.ContentType,
          size = Convert.ToInt64(result.Size),
          date = result.TimeCreated.Value.Ticks,
          creator_id = user.id,
          path = path
        };
        await DbStorageLog.Create(companyId, fileCreate);
      }

      await fileStream.DisposeAsync();

      // Set thuộc tính public
      MakePublic(fileName);

      await Task.Run(() =>
      {
        Files.DeleteFile(fileLink);
      });

      return bucketLink + fileName;
    }
    catch (System.Exception ex)
    {
      TelegramBot($"Storage Uploaded {fileLink}|{ex.ToString()}");
      return null;
    }
  }

  public static async Task<string> UploadStream(string fileName, string folderName, System.IO.Stream stream, string companyId = "", UserModel user = null, string path = "", Progress<IUploadProgress> progress = null)
  {
    try
    {
      StorageCredential();

      // Chuẩn hóa tên file
      fileName = DateTime.Now.Ticks + "_" + StringHelper.RenameFile(fileName);
      // Upload vào folder
      if (!string.IsNullOrEmpty(folderName))
        fileName = folderName + "/" + fileName;

      // https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
      string contentType = null;
      if (fileName.EndsWith(".png"))
        contentType = "image/png";
      else if (fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg"))
        contentType = "image/jpeg";
      else if (fileName.EndsWith(".gif"))
        contentType = "image/gif";
      else if (fileName.EndsWith(".txt"))
        contentType = "text/plain";
      else
      {
        string format = Files.FileFormat(fileName);
        contentType = "application/" + format.Replace(".", "");
      }

      var result = await storageClient.UploadObjectAsync(bucketName, fileName, contentType, stream, null, default, progress);

      if (!string.IsNullOrEmpty(companyId) && result != null && user != null)
      {
        // tạo dữ liệu lưu file bên local
        var fileCreate = new FileModel()
        {
          link = bucketLink + result.Name,
          name = Files.FileName(result.Name.Replace(companyId + "/", "")),
          format = result.ContentType,
          size = Convert.ToInt64(result.Size),
          date = result.TimeCreated.Value.Ticks,
          creator_id = user.id,
          path = path,
        };
        await DbStorageLog.Create(companyId, fileCreate);
      }

      Console.WriteLine($"Storage Uploaded: {fileName}.");

      // Set thuộc tính public
      MakePublic(fileName);

      return bucketLink + fileName;
    }
    catch (System.Exception ex)
    {
      TelegramBot($"Storage Uploaded {fileName}|{ex.ToString()}");
      return null;
    }
  }

  public static bool DeleteFile(string fileLink)
  {
    try
    {
      if (string.IsNullOrEmpty(fileLink))
        return true;

      if (fileLink.StartsWith("/"))
        return Files.DeleteFile(fileLink);

      StorageCredential();
      string fileName = fileLink.Replace(bucketLink, "");
      storageClient.DeleteObject(bucketName, fileName);

      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public static async Task<List<FileModel>> GetListAsync(string folder)
  {
    StorageCredential();

    var results = new List<FileModel>();
    var data = storageClient.ListObjectsAsync(bucketName, folder);
    await foreach (var item in data)
    {
      results.Add(new()
      {
        link = bucketLink + item.Name,
        name = Files.FileName(item.Name.Replace(folder + "/", "")),
        format = item.ContentType,
        size = Convert.ToInt64(item.Size),
        date = item.TimeCreated.Value.Ticks
      });
    }
    return results;
  }

  public static async Task<bool> DeleleFileAndLog(string link, UserModel user, string companyId)
  {
    var result = DeleteFile(link);
    if (result)
    {
      await DbStorageLog.Delete(companyId, link, user.FullName, user.avatar);
    }

    return result;
  }

  /// <summary>
  /// Lấy dung lượng đã dùng, tính theo byte
  /// </summary>
  public static async Task<long> GetStorageUsed(string folder)
  {
    StorageCredential();

    long result = 0;
    var data = storageClient.ListObjectsAsync(bucketName, folder);
    await foreach (var item in data)
      result += Convert.ToInt64(item.Size);
    return result;
  }

  public static void Download(string fileLink)
  {
    StorageCredential();

    var fileName = new FileInfo(fileLink).Name;
    var filePath = Environment.CurrentDirectory + "/wwwroot/download/" + fileName;

    using var outputFile = File.OpenWrite(filePath);
    storageClient.DownloadObject(bucketName, fileName, outputFile);
  }

  private static string MakePublic(string fileName)
  {
    var storageObject = storageClient.GetObject(bucketName, fileName);
    storageObject.Acl ??= new List<ObjectAccessControl>();
    storageClient.UpdateObject(storageObject, new UpdateObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead });
    return storageObject.MediaLink;
  }

  public static void TelegramBot(string message)
  {
    message = message.Replace("|", "\n").Trim();
    new Task(async () =>
    {
      var data = new Dictionary<string, string>
      {
          {"chat_id", "-731595697"},
          {"text", $"[Fastdo] {DateTime.Now:dd/MM, HH:mm:ss}\n{message}"}
      };

      //Create SSL/TLS secure channel
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      var url = "https://api.telegram.org/bot5195155040:AAF1LMXE6afxhVjCLnJ1W3f0131X2soQCCU/sendMessage";
      var client = new HttpClient();
      var response = await client.PostAsync(url, new System.Net.Http.FormUrlEncodedContent(data));
      //var result = await response.Content.ReadAsStringAsync();
    }).Start();
  }

  public static void TelegramBotERR(string message)
  {
    message = message.Replace("|", "\n").TrimStart('\n');
    return;

    new Task(async () =>
    {
      var data = new Dictionary<string, string>
      {
          {"chat_id", TELEGRAM_CHAT_ID},
          {"text", $"[Fastdo] {DateTime.Now:dd/MM, HH:mm:ss}\n{message}"}
      };

      //Create SSL/TLS secure channel
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      var url = "https://api.telegram.org/bot5195155040:AAF1LMXE6afxhVjCLnJ1W3f0131X2soQCCU/sendMessage";
      var client = new HttpClient();
      var response = await client.PostAsync(url, new System.Net.Http.FormUrlEncodedContent(data));
      //var result = await response.Content.ReadAsStringAsync();
    }).Start();
  }

  // Gửi bot telegram lỗi
  public static void CatchLog(string link, string fncName, string message, string userID, string companyName = "ADMIN/CLIENT")
  {
    var formatMessage = $"Function: {fncName}|Company: {companyName}|UserID: {userID}|Link: {link}|Error: {message}";
    Console.WriteLine(formatMessage);
    //TelegramBotERR(formatMessage);
    var log = new CatchLogModel()
    {
      create = DateTime.Now.Ticks,
      message = formatMessage,
    };

    new Task(async () =>
    {
      IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
      var environtment = config.GetValue<string>("EnvironmentDevelopment");

      if(environtment == "production")
      {
        await DbMainActivitiesTracking.Create(log);
      }
    }).Start();
  }

  #region Dữ liệu cố định
  public static string GetIconFromExtension(string extension)
  {
    if (extension.IsEmpty())
      return "other.png";

    extension = extension.Trim();
    extension = extension.Replace(".", "");
    extension = extension.ToLower();

    var document = new List<string>() { "doc", "docx" };
    var excel = new List<string>() { "xlsx", "xlsm", "xlsb", "xltx", "xls" };
    var illustrator = new List<string>() { "ai" };
    var images = new List<string>() { "png", "jpg", "jpeg", "gif" };
    var audio = new List<string>() { "mp3", "m4a", "flac", "mp3", "wav", "wma", "aac" };
    var pdf = new List<string>() { "pdf" };
    var ppt = new List<string>() { "ppt", "pptx", "pptm" };
    var psd = new List<string>() { "psd" };
    var svg = new List<string>() { "svg" };
    var txt = new List<string>() { "txt" };
    var video = new List<string>() { "mp4", "mov", "wmv", "avi", "avchd", "flv", "f4v", "swf", "mkv", "webm", "html5", "mpg" };
    var zip = new List<string>() { "zip", "rar", "7z", "iso" };

    var list = new Dictionary<string, List<string>>();

    list.Add("doc.png", document);
    list.Add("excel.png", excel);
    list.Add("ai.png", illustrator);
    list.Add("img.png", images);
    list.Add("music.png", audio);
    list.Add("pdf.png", pdf);
    list.Add("ppt.png", ppt);
    list.Add("psd.png", psd);
    list.Add("svg.png", svg);
    list.Add("txt.png", txt);
    list.Add("video.png", video);
    list.Add("zip.png", zip);

    var exist = list.FirstOrDefault(x => x.Value.Contains(extension));

    return exist.Key.IsEmpty() ? "other.png" : exist.Key;
  }
  #endregion
}