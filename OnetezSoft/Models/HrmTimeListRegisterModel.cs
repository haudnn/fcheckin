using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System;
using OnetezSoft.Data;

namespace OnetezSoft.Models;

[BsonIgnoreExtraElements]
public class HrmTimeListRegisterModel : HrmTimeListModel
{
	/// <summary>Ca làm được áp dụng </summary>
	public List<ShiftRegister> shifts_register { get; set; } = new();

	public class ShiftRegister
	{
		/// <summary>Ngày áp dụng</summary>
		public long day { get; set; }

		/// <summary>Xác nhận đã phê duyệt chưa</summary>
		public bool is_confirm { get; set; }

		/// <summary>id ngày nghỉ</summary>
		public string dayoff_id { get; set; }

		/// <summary>Danh sách chứa id ca làm được áp dụng</summary>
		public List<string> shifts_id { get; set; } = new();
	}
}