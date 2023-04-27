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
  public class DbTodoItem
  {
    private static string _collection = "todoitems";

    public static async Task<TodolistModel.Todo> Create(string companyId, TodolistModel.Todo model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<TodolistModel.Todo> Update(string companyId, TodolistModel.Todo model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id == model.id, model, option);

      return model;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static async Task<TodolistModel.Todo> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var result = await collection.Find(x => x.id == id).FirstOrDefaultAsync();

      return result;
    }


    public static List<TodolistModel.Todo> GetList(string companyId, string todolistId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      return collection.Find(x => x.todolist == todolistId).ToList().OrderBy(x => x.start).ToList();
    }


    /// <summary>
    /// Danh sách Todolist liên kết của công việc trong kế hoạch
    /// </summary>
    public static List<TodolistModel.Todo> GetList(string companyId, string taskId, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = collection.Find(x => x.user == userId && x.plan_task == taskId).ToList();

      return (from x in results orderby x.date descending, x.start select x).ToList();
    }

    /// <summary>
    /// Danh sách công việc đã giao
    /// </summary>
    public static async Task<List<TodolistModel.Todo>> GetAssignedList(string companyId, string assignUser,
      long start, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = await collection.Find(x => x.assign_user == assignUser
        && x.date >= start && x.date <= end).ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc được giao
    /// </summary>
    public static async Task<List<TodolistModel.Todo>> GetMyAssignedList(string companyId, string userId,
      long start, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = await collection.Find(x => x.user == userId && !string.IsNullOrEmpty(x.assign_user)
        && x.date >= start && x.date <= end).ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }
  }
}