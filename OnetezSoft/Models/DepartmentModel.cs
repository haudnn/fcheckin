using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class DepartmentModel
  {
    [BsonId]
    public string id { get; set; }

    public bool delete { get; set; }

    public string name { get; set; }

    public string parent { get; set; }

    /// <summary>Chức danh quản lý</summary>
    public string manager { get; set; }

    /// <summary>Chức danh phó quản lý</summary>
    public string deputy { get; set; }

    public int pos { get; set; }

    public List<string> members_id { get; set; }

    public List<MembersList> members_list { get; set; } = new();


    public class MembersList
    {
      /// <summary>ID nhân viên</summary>
      public string id { get; set; }

      /// <summary>1: quản lý; 2: phó quản lý; 3: nhân viên</summary>
      public int role { get; set; }
    }

    public class SelectList
    {
      public string id { get; set; }
      public string name { get; set; }
      public int level { get; set; }
    }
  }
}