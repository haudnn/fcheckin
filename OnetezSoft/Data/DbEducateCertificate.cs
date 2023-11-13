using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbEducateCertificate
  {
    private static string _collection = "educate_certificate";

    public static async Task<EducateCertificateModel> Create(string companyId, EducateCertificateModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCertificateModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<EducateCertificateModel> Update(string companyId, EducateCertificateModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCertificateModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCertificateModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<EducateCertificateModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCertificateModel>(_collection);

      return await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();
    }


    public static async Task<List<EducateCertificateModel>> GetList(string companyId, string keyword)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCertificateModel>(_collection);

      var list = await collection.FindAsync(new BsonDocument()).Result.ToListAsync();

      list = (from x in list orderby x.pin descending, x.date descending select x).ToList();

      var results = new List<EducateCertificateModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in list)
        {
          bool check = Handled.Shared.SearchKeyword(keyword, item.name);

          if (check)
            results.Add(item);
        }
      }
      else
        results = list;

      return results;
    }


    public static async Task<EducateCertificateModel> GetPin(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCertificateModel>(_collection);

      var results = await collection.FindAsync(x => x.pin).Result.ToListAsync();

      return results.OrderByDescending(x => x.date).FirstOrDefault();
    }


    public static async Task<EducateCertificateModel> GetDefault(string companyId)
    {
      var list = await GetList(companyId, null);
      if (list.Count > 0)
        return list[0];
      else
        return null;
    }
  }
}
