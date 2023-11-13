using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class EducateLessonModel
  {
    [BsonId]
    public string id { get; set; }

    /// <summary>ID khóa học</summary>
    public string course { get; set; }
    
    /// <summary>Loại bài học</summary>
    public int type { get; set; }
    
    /// <summary>Tên bài</summary>
    public string name { get; set; }
    
    /// <summary>Hình ảnh</summary>
    public string image { get; set; }
    
    /// <summary>Thời lượng: phút</summary>
    public int time { get; set; }
    
    /// <summary>Thứ tự</summary>
    public int pos { get; set; }
    
    /// <summary>Nội dung</summary>
    public string content { get; set; }
    
    /// <summary>Video youtube</summary>
    public string video { get; set; }

    /// <summary>File đính kèm => bỏ</summary>
    public string file { get; set; }

    /// <summary>File đính kèm</summary>
    public List<string> files { get; set; } = new();
    
    /// <summary>Mức điểm đạt</summary>
    public int point { get; set; }

    /// <summary>Câu hỏi</summary>
    public List<Question> questions { get; set; } = new();


    public class Question
    {
      public string id { get; set; }

      /// <summary>Nội dung</summary>
      public string content { get; set; }

      /// <summary>Điểm</summary>
      public int point { get; set; }

      /// <summary>Đáp án</summary>
      public List<Answer> answers { get; set; } = new();

      /// <summary>File đính kèm</summary>
      public List<string> files { get; set; } = new();


      public class Answer
      {
        public string id { get; set; }

        public string content { get; set; }

        public bool correct { get; set; }
      }
    }
  }
}
