using System;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class ConfigMongo
  {
    private string ConnectionString = "mongodb+srv://onetez_admin:7ABRv9HooN7qZI34@onetez-demo-fsfsg.gcp.mongodb.net/test?retryWrites=true&w=majority";

    public static IMongoDatabase DbConnect(string name)
    {
      //var client = new MongoClient(ConnectionString);
      var client = new MongoClient();
      return client.GetDatabase(name);
    }

    public static string RandomId()
    {
      return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();
    }
  }
}
