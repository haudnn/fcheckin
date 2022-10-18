using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class BackupData
  {
    public static void Create<T>(string db_name, string table_name, T record)
    {
      var db = Mongo.DbConnect(db_name);

      var collection = db.GetCollection<T>(table_name);
      
      collection.InsertOne(record);
    }
    
    public static void Update<T>(string db_name, string table_name, string id, T record)
    {
      var db = Mongo.DbConnect(db_name);

      var collection = db.GetCollection<T>(table_name);
      
      var result = collection.ReplaceOne(new BsonDocument("_id", id), record, new UpdateOptions { IsUpsert = true});
    }

    public static void Delete<T>(string db_name, string table_name, string id)
    {
      var db = Mongo.DbConnect(db_name);

      var collection = db.GetCollection<T>(table_name);

      collection.DeleteOne(new BsonDocument("_id", id));
    }
    
    public static async Task<List<T>> GetList<T>(string db_name, string table_name)
    {
      var db = Mongo.DbConnect(db_name);

      var collection = db.GetCollection<T>(table_name);

      return await collection.Find(new BsonDocument()).ToListAsync();
    }

    public static string GetValue<T>(T item, string key)
    {
      var type = item.GetType();
      var prop = type.GetProperty(key);
      var value = prop.GetValue(item);
      return value.ToString();
    }
  }
}