using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbCompany
  {
    private static string _collection = "companys";

    public static async Task<CompanyModel> Create(string companyId, CompanyModel model)
    {
      var _db = Mongo.DbConnect("company_" + companyId);

      var collection = _db.GetCollection<CompanyModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<CompanyModel> Update(string companyId, CompanyModel model)
    {
      var _db = Mongo.DbConnect("company_" + companyId);

      var collection = _db.GetCollection<CompanyModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static CompanyModel Get(string companyId)
    {
      var _db = Mongo.DbConnect("company_" + companyId);

      var collection = _db.GetCollection<CompanyModel>(_collection);

      return collection.Find(new BsonDocument()).FirstOrDefault();
    }
  }
}
