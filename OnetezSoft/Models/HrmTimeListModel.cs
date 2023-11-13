using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmTimeListModel
{
	/// <summary>ID = UserID</summary>
	[BsonId]
	public string id { get; set; }

	/// <summary>Ca làm được áp dụng </summary>
	public List<Shift> shifts { get; set; } = new();

	public class Shift
	{
		/// <summary>Ngày áp dụng</summary>
		public long day { get; set; }

		/// <summary>id ngày nghỉ</summary>
		public string dayoff_id { get; set; }

		/// <summary>Danh sách chứa id ca làm được áp dụng</summary>
		public List<string> shifts_id { get; set; } = new();
	}

	public class ShiftsData
	{
		// Ngày áp dụng
		public long start { get; set; }
		// Ngày kết thúc
		public long end { get; set; }
		// Danh sách ca làm áp dụng: Ngày
		public List<string> days { get; set; }
		// Danh sách ca làm áp dụng: Tuần
		public Dictionary<int, List<string>> week { get; set; }
	}
	
	public class ExportData
	{
		/// <summary>Tên bảng</summary>
		public string name { get; set; }
		/// <summary>Ngày bắt đầu</summary>
		public DateTimeOffset? start { get; set; }
		/// <summary>Ngày kết thúc</summary>
		public DateTimeOffset? end { get; set; }
	}
}