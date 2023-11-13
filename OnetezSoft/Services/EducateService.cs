using System.Collections.Generic;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class EducateService
  {
    /// <summary>
    /// Danh sách khoá học của một người đã học
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="user"></param>
    /// <param name="status">0: tất cả, 1: đang học, 2: cấp chứng chỉ, 3: không đạt</param>
    public static async Task<List<EducateCourseModel>> GetListLearned(string companyId, string user, int status)
    {
      var data = await DbEducateLearned.GetList(companyId, user, status);

      var results = new List<EducateCourseModel>();
      foreach (var item in data)
      {
        var course = await DbEducateCourse.Get(companyId, item.course);
        if (course != null)
          results.Add(course);
      }

      return results;
    }


    /// <summary>
    /// Đổi giảng viên cho khoá học
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="courseId">ID khoá học</param>
    /// <param name="teacherOld">ID giàng viên cũ</param>
    /// <returns></returns>
    public static async Task ChangeTeacher(string companyId, string courseId)
    {
      var course = await DbEducateCourse.Get(companyId, courseId);

      if (course != null)
      {
        // Chuyển lịch sử học
        var learned = await DbEducateLearned.GetList(companyId, null, courseId, null, 0);
        foreach (var item in learned)
        {
          item.course_name = course.name;
          item.teacher = course.teacher;
          await DbEducateLearned.Update(companyId, item);
        }

        // Chuyển bài thi
        var exams = await DbEducateExam.GetList(companyId, null, courseId, null, null, null);
        foreach (var item in exams)
        {
          item.course_name = course.name;
          item.teacher = course.teacher;
          await DbEducateExam.Update(companyId, item);
        }
      }
    }
  }
}
