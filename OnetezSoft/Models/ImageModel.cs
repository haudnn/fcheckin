using MongoDB.Bson.Serialization.Attributes;
namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class ImageModel
  {
    public string id { get; set; }

    public string link { get; set; }
  }
}
