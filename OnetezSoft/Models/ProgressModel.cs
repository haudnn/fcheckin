using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class ProgressModel
  {
    public string id { get; set; }

    public string name { get; set; }

    public string avatar { get; set; }

    public string departments { get; set; }

    public string desc { get; set; }

    public double value { get; set; }
  }
}
