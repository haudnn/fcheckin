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
  public class DbMainFeedback
  {
    private static IMongoDatabase _db = Mongo.DbConnect("fastdo");
    private static string _collection = "feedbacks";

    public static FeedbackModel Create(FeedbackModel model)
    {
      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      model.date = DateTime.Now.Ticks;
      model.status = 1;
      model.new_client = true;
      model.comments = new();
      model.notes = new();
      model.content = model.content.Replace("<", "&lt;").Replace(">", "&gt;");

      var collection = _db.GetCollection<FeedbackModel>(_collection);

      collection.InsertOne(model);

      return model;
    }


    public static bool Delete(string id)
    {
      var collection = _db.GetCollection<FeedbackModel>(_collection);

      var result = collection.DeleteOne(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static FeedbackModel Update(FeedbackModel model)
    {
      var collection = _db.GetCollection<FeedbackModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      collection.ReplaceOne(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static FeedbackModel Get(string id)
    {
      var collection = _db.GetCollection<FeedbackModel>(_collection);

      var result = collection.Find(x => x.id == id).FirstOrDefault();

      return result;
    }


    /// <summary>
    /// Danh sách góp ý của 1 người dùng
    /// </summary>
    /// <param name="userId"></param>
    public static async Task<List<FeedbackModel>> GetList(string userId)
    {
      var collection = _db.GetCollection<FeedbackModel>(_collection);

      var sorted = Builders<FeedbackModel>.Sort.Descending("date");

      return await collection.Find(x => x.user_id == userId).Sort(sorted).ToListAsync();
    }


    /// <summary>
    /// Danh sách góp ý dùng cho Admin
    /// </summary>
    /// <param name="keyword">Từ khoá</param>
    /// <param name="status">Trạng thái</param>
    /// <param name="company">Công ty</param>
    /// <param name="start">Từ ngày</param>
    /// <param name="end">Đến ngày</param>
    public static List<FeedbackModel> GetList(string keyword, int status, string company, 
      DateTime start, DateTime end)
    {
      var collection = _db.GetCollection<FeedbackModel>(_collection);

      var builder = Builders<FeedbackModel>.Filter;

      var filtered = builder.Gte("date", start.Ticks)
        & builder.Lt("date", end.AddDays(1).Ticks);

      if (status != 0) // Hoạt động
        filtered = filtered & builder.Eq("status", status);
      if (!string.IsNullOrEmpty(company))
        filtered = filtered & builder.Eq("company_id", company);

      var sorted = Builders<FeedbackModel>.Sort.Descending("date");

      var database = collection.Find(filtered).Sort(sorted).ToList();

      var results = new List<FeedbackModel>();

      if (!string.IsNullOrEmpty(keyword))
      {
        foreach (var item in database)
        {
          if (Handled.Shared.SearchKeyword(keyword, item.user_name + item.company_name + item.name))
            results.Add(item);
        }

        return results;
      }
      else
        return database;
    }


    /// <summary>
    /// Thêm phản hồi vào Góp ý
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="kaizenId"></param>
    /// <param name="userId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static FeedbackModel Comment(string feedbackId, string content, List<string> files,
      UserModel user, string company_name, bool isAdmin)
    {
      var feedback = Get(feedbackId);
      if (feedback != null)
      {
        var comment = new FeedbackModel.Comment()
        {
          id = Guid.NewGuid().ToString(),
          date = DateTime.Now.Ticks,
          content = content,
          files = files,
          user_id = user.id,
          user_name = user.FullName,
          user_avatar = user.avatar,
          user_admin = isAdmin,
          company_name = company_name
        };
        feedback.comments.Add(comment);

        if (isAdmin)
          feedback.new_admin = true;
        else
          feedback.new_client = true;

        return Update(feedback);
      }
      else
        return null;
    }


    /// <summary>
    /// Thêm ghi chú vào Góp ý
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="kaizenId"></param>
    /// <param name="userId"></param>
    /// <param name="content"></param>
    public static FeedbackModel Note(string feedbackId, string content, List<string> files, UserModel user)
    {
      var feedback = Get(feedbackId);
      if (feedback != null)
      {
        var note = new FeedbackModel.Note()
        {
          id = Guid.NewGuid().ToString(),
          date = DateTime.Now.Ticks,
          content = content,
          files = files,
          user_id = user.id,
          user_name = user.FullName,
          user_avatar = user.avatar
        };
        feedback.notes.Add(note);

        return Update(feedback);
      }
      else
        return null;
    }


    /// <summary>
    /// Lấy số lượng góp mới có cập nhật mới từ khách hàng
    /// </summary>
    public static async Task<long> GetCountNews()
    {
      var collection = _db.GetCollection<FeedbackModel>(_collection);

      var results = await collection.Find(x => x.new_client).ToListAsync();

      return results.Count;
    }


    #region Dữ liệu cố định

    // Trạng thái: danh sách
    public static List<StaticModel> Status()
    {
      var list = new List<StaticModel>();

      list.Add(new StaticModel
      {
        id = 1,
        name = "Chờ tiếp nhận",
        color = "has-text-warning"
      });

      list.Add(new StaticModel
      {
        id = 2,
        name = "Đã tiếp nhận",
        color = "has-text-success"
      });

      list.Add(new StaticModel
      {
        id = 3,
        name = "Đang xử lý",
        color = "has-text-link"
      });

      list.Add(new StaticModel
      {
        id = 4,
        name = "Đóng",
        color = "has-text-grey"
      });

      return list;
    }

    // Trạng thái: chi tiết
    public static StaticModel Status(int id)
    {
      var result = Status().SingleOrDefault(x => x.id == id);
      if (result != null)
        return result;
      else
        return new StaticModel();
    }

    #endregion
  }
}
