using OnetezSoft.Models;
using System.Collections.Generic;

namespace OnetezSoft.Services
{
  public class AchievementService
  {
    public static List<AchievementModel.Option> GetGroup()
    {
      var list = new List<AchievementModel.Option>();

      list.Add(new()
      {
        id = "todolist",
        name = "Thành tựu todolist",
        des = "Số ngày liên tiếp check-in và check-out đúng hạn trong 1 tháng",
        is_active = true,
      });
      list.Add(new()
      {
        id = "educate",
        name = "Thành tựu đào tạo",
        is_active = true,
        des = "Số chứng chỉ đạt được trong 1 tháng",
      });
      list.Add(new()
      {
        id = "cfrs",
        name = "Thành tựu ghi nhận",
        is_active = true,
        des = "Số lần được ghi nhận trong 1 tháng",
      });
      list.Add(new()
      {
        id = "kaizen",
        name = "Thành tựu kaizen",
        is_active = true,
        des = "Số Kaizen được đánh giá hữu ích trong 1 tháng (sẽ áp dụng, sẽ xem xét, đã ghi nhận)",
      });

      return list;
    }

    public static List<AchievementModel.Option> GetDefault()
    {
      var result = new List<AchievementModel.Option>();

      // Todolist
      result.Add(new()
      {
        id = "",
        name = "Không quên Todolist",
        is_active = true,
        des = "5 ngày liên tiếp không trễ todolist",
        star = 1,
        apply = 5,
        image = "/images/achi_todolist.png",
        parent = "todolist",
      });
      result.Add(new()
      {
        id = "",
        name = "Thích Todolist",
        is_active = true,
        des = "10 ngày liên tiếp không trễ todolist",
        star = 5,
        apply = 10,
        image = "/images/achi_todolist.png",
        parent = "todolist",
      });
      result.Add(new()
      {
        id = "",
        name = "Thích Todolist",
        is_active = true,
        des = "15 ngày liên tiếp không trễ todolist",
        star = 10,
        apply = 15,
        image = "/images/achi_todolist.png",
        parent = "todolist",
      });
      result.Add(new()
      {
        id = "",
        name = "Ghiền Todolist",
        is_active = true,
        des = "20 ngày liên tiếp không trễ todolist",
        star = 20,
        apply = 20,
        image = "/images/achi_todolist.png",
        parent = "todolist",
      });
      result.Add(new()
      {
        id = "",
        name = "Nghiện Todolist",
        is_active = true,
        des = "25 ngày liên tiếp không trễ todolist",
        star = 35,
        apply = 25,
        image = "/images/achi_todolist.png",
        parent = "todolist",
      });
      // CFRs
      result.Add(new()
      {
        id = "",
        name = "Nhân viên chăm chỉ",
        is_active = true,
        des = "Được ghi nhận 3 lần",
        star = 5,
        apply = 3,
        image = "/images/achi_cfrs.png",
        parent = "cfrs",
      });
      result.Add(new()
      {
        id = "",
        name = "Nhân viên giỏi",
        is_active = true,
        des = "Được ghi nhận 5 lần",
        star = 10,
        apply = 5,
        image = "/images/achi_cfrs.png",
        parent = "cfrs",
      });
      result.Add(new()
      {
        id = "",
        name = "Nhân viên tuyệt vời",
        is_active = true,
        des = "Được ghi nhận 7 lần",
        star = 20,
        apply = 7,
        image = "/images/achi_cfrs.png",
        parent = "cfrs",
      });
      result.Add(new()
      {
        id = "",
        name = "Nhân viên xuất sắc",
        is_active = true,
        des = "Được ghi nhận 10 lần",
        star = 35,
        apply = 10,
        image = "/images/achi_cfrs.png",
        parent = "cfrs",
      });
      // Kaizen
      result.Add(new()
      {
        id = "",
        name = "Gieo hạt",
        is_active = true,
        des = "Có 3 góp ý Kaizen có ích",
        star = 5,
        apply = 3,
        image = "/images/achi_kaizen.png",
        parent = "kaizen",
      });
      result.Add(new()
      {
        id = "",
        name = "Ươm mầm",
        is_active = true,
        des = "Có 5 góp ý Kaizen có ích",
        star = 10,
        apply = 5,
        image = "/images/achi_kaizen.png",
        parent = "kaizen",
      });
      result.Add(new()
      {
        id = "",
        name = "Đơm hoa",
        is_active = true,
        des = "Có 7 góp ý Kaizen có ích",
        star = 20,
        apply = 7,
        image = "/images/achi_kaizen.png",
        parent = "kaizen",
      });
      result.Add(new()
      {
        id = "",
        name = "Kết trái",
        is_active = true,
        des = "Có 10 góp ý Kaizen có ích",
        star = 35,
        apply = 10,
        image = "/images/achi_kaizen.png",
        parent = "kaizen",
      });
      // Đào tạo
      result.Add(new()
      {
        id = "",
        name = "Tốt nghiệp Mẫu giáo",
        is_active = true,
        des = "Nhận được 3 chứng chỉ Đào tạo",
        star = 5,
        apply = 3,
        image = "/images/achi_educate.png",
        parent = "educate",
      });
      result.Add(new()
      {
        id = "",
        name = "Tốt nghiệp Tiểu học",
        is_active = true,
        des = "Nhận được 5 chứng chỉ Đào tạo",
        star = 10,
        apply = 5,
        image = "/images/achi_educate.png",
        parent = "educate",
      });
      result.Add(new()
      {
        id = "",
        name = "Tốt nghiệp Trung học",
        is_active = true,
        des = "Nhận được 7 chứng chỉ Đào tạo",
        star = 20,
        apply = 7,
        image = "/images/achi_educate.png",
        parent = "educate",
      });
      result.Add(new()
      {
        id = "",
        name = "Tốt nghiệp Đại học",
        is_active = true,
        des = "Nhận được 10 chứng chỉ Đào tạo",
        star = 35,
        apply = 10,
        image = "/images/achi_educate.png",
        parent = "educate",
      });

      return result;
    }
  }
}
