using MongoDB.Driver;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbTodoItem
  {
    private static string _collection = "todoitems";

    public static async Task<TodolistModel.Todo> Create(string companyId, TodolistModel.Todo model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();

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

      var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

      return result;
    }


    public static async Task<TodolistModel.Todo> GetByUserid(string companyId, string id, string userID)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var result = await collection.FindAsync(x => x.id == id && x.user == userID).Result.FirstOrDefaultAsync();

      return result;
    }


    public static async Task<List<TodolistModel.Todo>> GetList(string companyId, string todolistId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var result = await collection.FindAsync(x => todolistId.IsEmpty() || x.todolist == todolistId).Result.ToListAsync();

      return result.OrderBy(x => x.start).ToList();
    }


    /// <summary>
    /// Danh sách Todolist liên kết của công việc trong kế hoạch
    /// </summary>
    public static async Task<List<TodolistModel.Todo>> GetList(string companyId, string taskId, string userId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = await collection
              .Find(x => x.user == userId && x.plan_task == taskId)
              .SortByDescending(x => x.date)
              .ThenBy(x => x.start)
              .ToListAsync();

      return results;
    }

    /// <summary>
    /// Danh sách công việc đã giao
    /// </summary>
    public static async Task<List<TodolistModel.Todo>> GetAssignedList(string companyId, string assignUser, long start, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = await collection.FindAsync(x => x.assign_user == assignUser && x.date >= start && x.date <= end).Result.ToListAsync();

      return results;
    }
    /// <summary>
    /// Đếm danh sách công việc đã giao
    /// </summary>
    public static async Task<int> GetAssignedListCount(string companyId, string assignUser, long start, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = await collection.FindAsync(x => x.assign_user == assignUser && x.date >= start && x.date <= end && x.assign_status == 1).Result.ToListAsync();

      return results.Count();
    }


    /// <summary>
    /// Danh sách công việc được giao
    /// </summary>
    public static async Task<List<TodolistModel.Todo>> GetMyAssignedList(string companyId, string userId, long start, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);


      var results = await collection.Find(x => x.user == userId && !string.IsNullOrEmpty(x.assign_user) && x.date >= start && x.date <= end).ToListAsync();

      return results.OrderByDescending(x => x.date).ToList();
    }

    /// <summary>
    /// Đếm danh sách công việc được giao mà chưa xác nhận
    /// </summary>
    public static async Task<int> GetMyAssignedListCount(string companyId, string userId, long start, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);

      var results = await collection.FindAsync(x => x.user == userId && !string.IsNullOrEmpty(x.assign_user) && x.date >= start && x.date <= end && x.assign_status == 1).Result.ToListAsync();

      return results.OrderByDescending(x => x.date).ToList().Count;
    }

    /// <summary>
    /// Xoá tất cả công việc lặp trong quá khứ
    /// </summary>
    public static async Task DeleteTodoPrev(string companyId, string userId, string id, long end)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<TodolistModel.Todo>(_collection);
      long today = DateTime.Today.Ticks;

      var results = await collection.FindAsync(x => x.user == userId && x.parentId == id && x.date >= today && x.date <= end).Result.ToListAsync();


      var option = new ReplaceOptions { IsUpsert = false };

      foreach (var item in results)
      {
        item.dateEnd_cycle = 0;
        item.cycle = 2;
        item.option = 1;
        item.is_loop = false;
        item.weeks = new();
        await collection.ReplaceOneAsync(x => x.id == item.id, item, option);
      }
    }

  }
}