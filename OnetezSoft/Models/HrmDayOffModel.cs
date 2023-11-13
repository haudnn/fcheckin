using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class HrmDayOffModel
	{
		[BsonId]
		public string id { get; set; }

		/// <summary>Tên ngày nghỉ</summary>
		public string name { get; set; }

		/// <summary>Ngày bắt đầu</summary>
		public long start { get; set; }

		/// <summary>Ngày kết thúc</summary>
		public long end { get; set; }

		/// <summary>Ngày tạo</summary>
		public long create { get; set; }

		/// <summary>Lặp: 1. Một lần | 2. Hàng tuần</summary>
		public int loop { get; set; }

		/// <summary>Lặp ngày trong tuần</summary>
		public Week loop_week { get; set; }
		/// <summary>Danh sách nhân sự có tính lương</summary>
		public List<string> salary_users { get; set; } = new();

	/// <summary>Danh sách nhân sự không tính lương</summary>
	public List<string> non_salary_users { get; set; } = new();

		public class Week
		{
			public bool mon { get; set; }
			public bool tue { get; set; }
			public bool wed { get; set; }
			public bool thu { get; set; }
			public bool fri { get; set; }
			public bool sat { get; set; }
			public bool sun { get; set; }
		}
		/// <summary>Xoá mềm dữ liệu</summary>
		public bool isDeleted { get; set; }
	}
}
