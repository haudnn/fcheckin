using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class StaticModel
   {
      public int id { get; set; }

      public string id_string { get; set; }

      public string name { get; set; }

      public string icon { get; set; }

      public string color { get; set; }

      public bool isDefault { get; set; }
   }
}
