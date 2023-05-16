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
  public class DbBlog
  {
    private static string _collection = "blogs";

    public static async Task<BlogModel> Create(string companyId, BlogModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;

      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<BlogModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<BlogModel> Update(string companyId, BlogModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<BlogModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<BlogModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<BlogModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<BlogModel>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }


    public static async Task<List<BlogModel>> GetList(string companyId, string department)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<BlogModel>(_collection);

      var results = new List<BlogModel>();

      if (string.IsNullOrEmpty(department))
          results = await collection.Find(x => true).ToListAsync();
      else
          results = await collection.Find(x => x.department == department).ToListAsync();

      return (from x in results orderby x.pin descending, x.created descending, x.date descending select x).ToList();
    }

      public static async Task<List<BlogModel>> GetListShow(string companyId, string department)
        {
            var _db = Mongo.DbConnect("company_" + companyId);

            var collection = _db.GetCollection<BlogModel>(_collection);

            var builder = Builders<BlogModel>.Filter;

            var filtered = builder.Eq("is_show", true);

            if (!string.IsNullOrEmpty(department))
                filtered = filtered & builder.Eq("department", department);

            var results = await collection.Find(filtered).ToListAsync();

            return (from x in results orderby x.date descending select x).ToList();
        }
  }
}
