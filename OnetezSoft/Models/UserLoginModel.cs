using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class UserLoginModel
  {
    [BsonId]
    public string id { get; set; }

    public string user_id { get; set; }

    public long update { get; set; }

    public int fail_count { get; set; }

    public long expired { get; set; }

    public List<string> details { get; set; } = new();
  }
}
