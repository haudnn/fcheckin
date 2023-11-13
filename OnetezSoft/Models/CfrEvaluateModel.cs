using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class CfrEvaluateModel
  {

    [BsonId]
    public string id { get; set; }
    public string name { get; set; }
    public int star { get; set; }

    // => Bỏ
    public int type { get; set; }
    // => Bỏ
    public bool special { get; set; }
  }
}