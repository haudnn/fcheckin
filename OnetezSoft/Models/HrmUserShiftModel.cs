using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmUserShiftModel
{
	/// <summary>ID = UserID</summary>
	[BsonId]
	public string id { get; set; }

	/// <summary>Chế độ hybrid</summary>
	public bool is_hybrid { get; set; }

	/// <summary>Gán ca cho nhân viên </summary>
	public List<string> shifts { get; set; } = new();
}