using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OnetezSoft.Handled;
using OnetezSoft.Models;

namespace OnetezSoft.Services;

public class StorageService
{
  private static GoogleCredential credential = null;
  private static StorageClient storageClient;
  
  private static readonly string bucketName = "fastdo-storage.appspot.com";
  private static readonly string bucketLink = "https://storage.googleapis.com/" + bucketName + "/";
  private static readonly string pathSecret = AppDomain.CurrentDomain.BaseDirectory + "storage.json";

  private static void Credential()
  {
    try
    {
      using (var stream = new FileStream(pathSecret, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        credential = GoogleCredential.FromStream(stream);
      }
      //Console.WriteLine("Google Credential: Created");
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Google Credential: " + ex.Message);
    }
  }

  private static void StorageCredential()
  {
    try
    {
      Credential();
      if (storageClient == null)
        storageClient = StorageClient.Create(credential);
      //Console.WriteLine("Storage Credential: Created");
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Storage Credential: " + ex.Message);
    }
  }

  public static async Task<string> UploadAsync(string fileLink, string folderName)
  { 
    Console.WriteLine($"Storage Uploading: {fileLink}");
    try
    {
      StorageCredential();

      string filePath = Files.FilePath(fileLink);
      string fileName = Path.GetFileName(filePath);
      
      Console.WriteLine($"filePath: {filePath}");
      Console.WriteLine($"fileName: {fileName}");

      using (var fileStream = File.OpenRead(filePath))
      {
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

        if(!string.IsNullOrEmpty(folderName))
          fileName = folderName + "/" + fileName;

        var result = await storageClient.UploadObjectAsync(bucketName, fileName, contentType, fileStream, null);
        Console.WriteLine($"Storage Uploaded: {fileName}.");
      }

      // Set thuộc tính public
      MakePublic(fileName);

      return bucketLink + fileName;
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Storage Uploaded: " + ex.Message);
      TelegramBot($"Storage Uploaded {fileLink}|{ex.Message}");
      return null;
    }
  }

  public static async Task<string> UploadStream(string fileName, string folderName, System.IO.Stream stream)
  {
    Console.WriteLine($"Storage Uploading: {fileName}");
    try
    {
      StorageCredential();

      // Chuẩn hóa tên file
      fileName = DateTime.Now.Ticks + "_" + StringHelper.RenameFile(fileName);
      // Upload vào folder
      if(!string.IsNullOrEmpty(folderName))
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

      var result = await storageClient.UploadObjectAsync(bucketName, fileName, contentType, stream, null);
      Console.WriteLine($"Storage Uploaded: {fileName}.");

      // Set thuộc tính public
      MakePublic(fileName);

      return bucketLink + fileName;
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Storage Uploaded: " + ex.Message);
      TelegramBot($"Storage Uploaded {fileName}|{ex.Message}");
      return null;
    }
  }

  public static bool DeleteFile(string fileLink)
  {
    Console.WriteLine("Storage Deleteing: " + fileLink);
    try
    {
      if(string.IsNullOrEmpty(fileLink))
        return true;

      if(fileLink.StartsWith("/"))
        return Files.DeleteFile(fileLink);

      StorageCredential();
      string fileName = fileLink.Replace(bucketLink, "");
      storageClient.DeleteObject(bucketName, fileName);
      Console.WriteLine("Storage Deleted: " + fileName);

      return true;
    }
    catch (System.Exception ex)
    {
      Console.WriteLine("Storage Deleted: " + ex.Message);
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
        name = Files.FileName(item.Name.Replace(folder +"/", "")),
        format = item.ContentType,
        size = Convert.ToInt64(item.Size),
        date = item.TimeCreated.Value.Ticks
      });
    }
    return results;
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

    Console.WriteLine($"Downloaded {fileName} to {filePath}.");
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
          {"text", $"[Fastdo] {DateTime.Now.ToString("dd/MM, HH:mm:ss")}\n{message}"}
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
}
