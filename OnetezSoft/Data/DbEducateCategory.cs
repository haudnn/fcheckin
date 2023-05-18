﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbEducateCategory
  {
    private static string _collection = "educate_category";

    public static async Task<EducateCategoryModel> Create(string companyId, EducateCategoryModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCategoryModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<EducateCategoryModel> Update(string companyId, EducateCategoryModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCategoryModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCategoryModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }

    public static async Task<EducateCategoryModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCategoryModel>(_collection);

      return await collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public static List<EducateCategoryModel> GetList(string companyId, string keyword)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<EducateCategoryModel>(_collection);

      var sorted = Builders<EducateCategoryModel>.Sort.Ascending("name");

      var list = collection.Find(new BsonDocument()).Sort(sorted).ToList();

      var results = new List<EducateCategoryModel>();

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
  }
}
