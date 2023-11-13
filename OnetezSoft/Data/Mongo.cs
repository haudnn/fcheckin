using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using MongoDB.Driver;
using OnetezSoft.Handled;
using System;
using System.Text;

namespace OnetezSoft.Data
{
  public class Mongo
  {
    private static MongoCredential credential = MongoCredential.CreateCredential("admin", "fastdo", "#=62=V8l5%%jHN4");
    private static MongoClientSettings settings = MongoClientSettings.FromConnectionString("mongodb://103.153.215.53:27017/");

    public static IMongoDatabase DbConnect(string name)
    {
      IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
      var environtment = config.GetValue<string>("EnvironmentDevelopment");

      if (environtment == "development")
      {
        var mongoClient = new MongoClient();

        return mongoClient.GetDatabase(name);
      }
      else if (environtment == "production")
      {
        settings.Credential = credential;

        var mongoClient = new MongoClient(settings);

        return mongoClient.GetDatabase(name);
      }
      else if (environtment == "beta")
      {
        MongoCredential betaCredential = MongoCredential.CreateCredential("admin", "myUserAdmin", "=#Vqa6u##88%11N");
        MongoClientSettings betaSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017/");

        betaSettings.Credential = betaCredential;

        var mongoClient = new MongoClient(betaSettings);

        return mongoClient.GetDatabase(name);
      }

      return null;
    }

    private static readonly string Characters = "0123456789abcdefghijklmnopqrstuvwxyz";
    public static string RandomId()
    {
      DateTime date = DateTime.Now;

      var result = new StringBuilder();
      result.Append(DateTime.Now.ToString("yy"));
      result.Append(Characters[date.Month]);
      result.Append(Characters[date.Day]);
      result.Append(Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6));

      return result.ToString().ToUpper();
    }
  }
}