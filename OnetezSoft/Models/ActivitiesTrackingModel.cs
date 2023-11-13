using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class ActivitiesTrackingModel
  {
    [BsonId]
    public string id { get; set; }

    public long update { get; set; }

    public string update_string
    {
      get
      {
        if (update == 0)
          return string.Empty;
        else
        {
          return new DateTime(update).ToString("yyyy-MM-dd HH:mm");
        }
      }
    }

    public List<UserTracking> users { get; set; } = new();

    public class UserTracking
    {
      public string user { get; set; }

      public string company { get; set; }

      public string url { get; set; }
    }
  }

  public class SystemTracking
  {
    [BsonId]
    public string id { get; set; }

    public long create { get; set; }

    public string cpuUsage { get; set; }

    public string ramUsage { get; set; }
  }

  public class CatchLogModel
  {
    [BsonId]
    public string id { get; set; }

    public long create { get; set; }

    public string create_string
    {
      get
      {
        if (create == 0)
          return string.Empty;
        else
        {
          return new DateTime(create).ToString("yyyy-MM-dd HH:mm");
        }
      }
    }

    public string message { get; set; }
  }
}
