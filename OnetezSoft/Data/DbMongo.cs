using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
   public class DbMongo
   {
      //private string ConnectionString = "mongodb+srv://onetez_admin:7ABRv9HooN7qZI34@onetez-demo-fsfsg.gcp.mongodb.net/test?retryWrites=true&w=majority";

      public static IMongoDatabase DbConnect(string name)
      {
         //var client = new MongoClient(ConnectionString);
         var client = new MongoClient();
         return client.GetDatabase(name);
      }
   }
}
