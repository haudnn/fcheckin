using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

public class HrmUserShiftModel
{
  /// <summary>ID = UserID</summary>
  [BsonId]
  public string id { get; set; }

  /// <summary>Chế độ linh động</summary>
  public bool is_flexible { get; set; }

  /// <summary>Phân ca làm viêc trong tuần</summary>
  public List<Shift> shifts { get; set; } = new()
  {
    new() { day = 1 },
    new() { day = 2 },
    new() { day = 3 },
    new() { day = 4 },
    new() { day = 5 },
    new() { day = 6 },
    new() { day = 0 }
  };


  /// <summary>Phân ca làm việc</summary>
  public class Shift
  {
    /// <summary>Thứ trong tuần</summary>
    public int day { get; set; }

    /// <summary>ID ca sáng</summary>
    public string morning { get; set; }

    /// <summary>ID ca chiều</summary>
    public string afternoon { get; set; }
  }
}