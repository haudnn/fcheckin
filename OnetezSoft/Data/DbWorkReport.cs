using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using OnetezSoft.Services;

namespace OnetezSoft.Data
{
  public class DbWorkReport
  {
    private static string _collection = "work_reports";

    public static async Task<WorkPlanModel.Report> Update(string companyId, string id)
    {
      var tasks = await DbWorkTask.GetListInPlan(companyId, id);
      var report = WorkService.ReportTasks(tasks);
      report.id = id;
      
      var _db = Mongo.DbConnect("fastdo_" + companyId);
      var collection = _db.GetCollection<WorkPlanModel.Report>(_collection);
      var check = collection.Find(x => x.id == id).FirstOrDefault();
      if(check == null)
      {
        await collection.InsertOneAsync(report);
      }
      else
      {
        var option = new ReplaceOptions { IsUpsert = false };
        await collection.ReplaceOneAsync(x => x.id == id, report, option);
      }

      return report;
    }

    
    public static WorkPlanModel.Report Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<WorkPlanModel.Report>(_collection);

      var result = collection.Find(x => x.id == id).FirstOrDefault();

      if(result != null)
        return result;
      else
        return new WorkPlanModel.Report() { id = id };
    }
  }
}