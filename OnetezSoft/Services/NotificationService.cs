using System.Collections.Generic;

namespace OnetezSoft.Services
{
  public class NotificationService
  {
    public class NotiType
    {
      public string name { get; set; }

      public int type_from { get; set; }

      public int type_to { get; set; }

      public bool is_default { get; set; }
    }

    public static string GetUpdateLink(int type, string link)
    {
      var result = link;
      // Todolist
      if (type == 201)
        result = "/todolist/receive";
      if (type == 202 || type == 203 || type == 212 || type == 214 || type == 215)
        result = "/todolist/send";

      return result;
    }

    public static List<NotiType> GetListType()
    {
      var list = new List<NotiType>();

      list.Add(new()
      {
        name = "Thông báo Tin tức",
        type_from = 9,
        type_to = 9,
      });

      list.Add(new()
      {
        name = "Thông báo Cấu hình",
        type_from = 10,
        type_to = 23,
      });

      list.Add(new()
      {
        name = "Thông báo Góp ý hệ thống",
        type_from = 30,
        type_to = 30,
      });

      list.Add(new()
      {
        name = "Thông báo Kaizen",
        type_from = 100,
        type_to = 199,
      });

      list.Add(new()
      {
        name = "Thông báo Todolist",
        type_from = 200,
        type_to = 299,
      });

      list.Add(new()
      {
        name = "Thông báo Cửa hàng",
        type_from = 300,
        type_to = 399,
      });

      list.Add(new()
      {
        name = "Thông báo hệ thống",
        type_from = 400,
        type_to = 450,
      });

      list.Add(new()
      {
        name = "Thông báo OKRs",
        type_from = 503,
        type_to = 599,
      });

      list.Add(new()
      {
        name = "Thông báo Đào tạo",
        type_from = 600,
        type_to = 699,
      });

      list.Add(new()
      {
        name = "Thông báo Kế hoạch",
        type_from = 700,
        type_to = 799,
      });

      list.Add(new()
      {
        name = "Thông báo Chấm công",
        type_from = 800,
        type_to = 899,
      });

      list.Add(new()
      {
        name = "Thông báo Ghi nhận",
        type_from = 900,
        type_to = 999,
      });

      list.Add(new()
      {
        name = "Thông báo KPIs",
        type_from = 1000,
        type_to = 1099,
      });

      return list;
    }

    public static List<NotiType> GetAll()
    {
      var list = new List<NotiType>();

      // Tin tức
      list.Add(new()
      {
        name = "Tin tức",
        type_from = 9,
        type_to = 9,
      });

      // Cấu hình
      list.Add(new()
      {
        name = "Tôi được thêm vào phòng ban",
        type_from = 10,
        type_to = 10,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Tôi được thêm quyền trưởng phòng",
        type_from = 11,
        type_to = 11,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Tôi được thêm quyền phó phòng",
        type_from = 12,
        type_to = 12,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Thời gian todolist",
        type_from = 20,
        type_to = 20,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Phòng ban mới được tạo",
        type_from = 21,
        type_to = 21,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Phòng ban được chỉnh sửa",
        type_from = 22,
        type_to = 22,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Phòng ban bị xóa",
        type_from = 23,
        type_to = 23,
        is_default = true,
      });

      // Góp ý hệ thống
      list.Add(new()
      {
        name = "Góp ý của tôi được phản hồi",
        type_from = 30,
        type_to = 30,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Kaizen mới được tạo",
        type_from = 100,
        type_to = 100,
      });

      list.Add(new()
      {
        name = "Kaizen của tôi bị gỡ",
        type_from = 101,
        type_to = 101,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Kaizen của tôi được phản hồi",
        type_from = 102,
        type_to = 102,
      });

      list.Add(new()
      {
        name = "Kaizen của tôi bị xóa",
        type_from = 103,
        type_to = 103,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Kaizen của tôi được đánh giá",
        type_from = 104,
        type_to = 104,
      });

      list.Add(new()
      {
        name = "Kaizen được đánh giá",
        type_from = 105,
        type_to = 105,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Kaizen bị xóa",
        type_from = 106,
        type_to = 106,
        is_default = true,
      });

      // Todolist
      list.Add(new()
      {
        name = "Todolist của tôi được xác nhận",
        type_from = 200,
        type_to = 200,
      });

      list.Add(new()
      {
        name = "Có công việc giao việc cho tôi",
        type_from = 201,
        type_to = 201,
      });

      list.Add(new()
      {
        name = "Việc đã giao được xác nhận",
        type_from = 202,
        type_to = 202,
      });

      list.Add(new()
      {
        name = "Việc đã giao bị từ chối",
        type_from = 203,
        type_to = 203,
      });

      list.Add(new()
      {
        name = "Việc đã giao cập nhật Pending",
        type_from = 212,
        type_to = 212,
      });

      list.Add(new()
      {
        name = "Việc đã giao cập nhật Done",
        type_from = 214,
        type_to = 214,
      });

      list.Add(new()
      {
        name = "Việc đã giao cập nhật Cancel",
        type_from = 215,
        type_to = 215,
      });

      list.Add(new()
      {
        name = "Yêu cầu cần phê duyệt đổi quà",
        type_from = 300,
        type_to = 300,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Yêu cầu đổi quà của tôi được duyệt",
        type_from = 301,
        type_to = 301,
      });

      list.Add(new()
      {
        name = "Yêu cầu đổi quà của tôi bị từ chối",
        type_from = 302,
        type_to = 302,
      });

      list.Add(new()
      {
        name = "Nhận được quà tặng",
        type_from = 303,
        type_to = 303,
      });

      // Hệ thống
      list.Add(new()
      {
        name = "Thông báo hệ thống",
        type_from = 400,
        type_to = 400,
        is_default = true,
      });

      // OKRs
      list.Add(new()
      {
        name = "Yêu cầu xác nhận Checkin nháp",
        type_from = 503,
        type_to = 503,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Checkin nháp được xác nhận",
        type_from = 504,
        type_to = 504,
      });

      list.Add(new()
      {
        name = "Hoàn thành buổi checkin OKRs",
        type_from = 505,
        type_to = 505,
      });

      list.Add(new()
      {
        name = "Phản hồi mục tiêu của người khác",
        type_from = 506,
        type_to = 506,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Phản hồi mục tiêu của tôi",
        type_from = 507,
        type_to = 507,
      });

      list.Add(new()
      {
        name = "Đánh giá OKRs cần tôi đánh giá",
        type_from = 508,
        type_to = 508,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Đánh giá của tôi được mở lại",
        type_from = 509,
        type_to = 509,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Đánh giá của tôi được xác nhận",
        type_from = 510,
        type_to = 510,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Có đánh giá gửi đến tôi",
        type_from = 511,
        type_to = 511,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Được thêm vào hành động OKRs",
        type_from = 513,
        type_to = 513,
      });

      list.Add(new()
      {
        name = "Được cấp chứng chỉ",
        type_from = 600,
        type_to = 600,
      });

      list.Add(new()
      {
        name = "Học viên nộp bài thi khóa học",
        type_from = 601,
        type_to = 601,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Tôi được cấp quyền quản lý khóa học",
        type_from = 602,
        type_to = 602,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Bài thi của tôi được chấm",
        type_from = 603,
        type_to = 603,
      });

      list.Add(new()
      {
        name = "Ca làm được tạo",
        type_from = 800,
        type_to = 800,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Ca làm được chỉnh sửa",
        type_from = 801,
        type_to = 801,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Ca làm bị xóa",
        type_from = 802,
        type_to = 802,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Địa điểm chấm công được tạo",
        type_from = 803,
        type_to = 803,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Địa điểm chấm công được chỉnh sửa",
        type_from = 804,
        type_to = 804,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Địa điểm chấm công bị xóa",
        type_from = 805,
        type_to = 805,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Quy định chấm công được cập nhật",
        type_from = 806,
        type_to = 806,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Bảng công được tạo",
        type_from = 807,
        type_to = 807,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Bảng công được chỉnh sửa",
        type_from = 808,
        type_to = 808,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Bảng công bị xóa",
        type_from = 809,
        type_to = 809,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Ngày nghỉ được tạo",
        type_from = 810,
        type_to = 810,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Ngày nghỉ được chỉnh sửa",
        type_from = 811,
        type_to = 811,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Ngày nghỉ bị xóa",
        type_from = 812,
        type_to = 812,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Đơn từ được đăng ký",
        type_from = 813,
        type_to = 813,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Đơn từ của tôi được cập nhật",
        type_from = 814,
        type_to = 814,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Đơn từ của tôi được duyệt quá hạn",
        type_from = 815,
        type_to = 815,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Thiết bị mới được đăng ký",
        type_from = 816,
        type_to = 816,
      });

      list.Add(new()
      {
        name = "Thiết bị của tôi được phê duyệt",
        type_from = 817,
        type_to = 817,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Thiết bị của tôi bị từ chối",
        type_from = 818,
        type_to = 818,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Bảng đăng ký ca làm được duyệt",
        type_from = 819,
        type_to = 819,
      });

      list.Add(new()
      {
        name = "Yêu cầu phân địa điểm chấm công",
        type_from = 820,
        type_to = 820,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Đơn từ có bình luận mới",
        type_from = 821,
        type_to = 821,
      });

      list.Add(new()
      {
        name = "Đơn từ có bình luận nhắc tên",
        type_from = 822,
        type_to = 822,
      });

      list.Add(new()
      {
        name = "Kế hoạch được tạo",
        type_from = 700,
        type_to = 700,
      });

      list.Add(new()
      {
        name = "Kế hoạch được cập nhật tiêu đề",
        type_from = 701,
        type_to = 701,
      });

      list.Add(new()
      {
        name = "Kế hoạch có nhóm công việc mới",
        type_from = 702,
        type_to = 702,
      });

      list.Add(new()
      {
        name = "Kế hoạch có nhóm công việc được cập nhật",
        type_from = 703,
        type_to = 703,
      });

      list.Add(new()
      {
        name = "Kế hoạch có nhóm công việc bị xóa",
        type_from = 704,
        type_to = 704,
      });

      list.Add(new()
      {
        name = "Kế hoạch bị xóa",
        type_from = 705,
        type_to = 705,
      });

      list.Add(new()
      {
        name = "Tôi được thêm vào kế hoạch",
        type_from = 706,
        type_to = 706,
      });

      list.Add(new()
      {
        name = "Tôi bị xóa khỏi kế hoạch",
        type_from = 707,
        type_to = 707,
      });

      list.Add(new()
      {
        name = "Công việc được tạo",
        type_from = 708,
        type_to = 708,
      });

      list.Add(new()
      {
        name = "Công việc được cập nhật tiêu đề",
        type_from = 709,
        type_to = 709,
      });

      list.Add(new()
      {
        name = "Công việc được cập nhật trạng thái Done",
        type_from = 710,
        type_to = 710,
      });

      list.Add(new()
      {
        name = "Công việc được cập nhật trạng thái Review",
        type_from = 711,
        type_to = 711,
      });

      list.Add(new()
      {
        name = "Kế hoạch có bình luận mới",
        type_from = 712,
        type_to = 712,
      });

      list.Add(new()
      {
        name = "Tôi được thêm vào một công việc",
        type_from = 713,
        type_to = 713,
      });

      list.Add(new()
      {
        name = "Tôi bị xóa khỏi một công việc",
        type_from = 714,
        type_to = 714,
      });

      list.Add(new()
      {
        name = "Tôi được thêm vào một công việc phụ",
        type_from = 715,
        type_to = 715,
      });

      list.Add(new()
      {
        name = "Tôi bị xóa khỏi một công việc phụ",
        type_from = 716,
        type_to = 716,
      });

      list.Add(new()
      {
        name = "Công việc bị xóa",
        type_from = 717,
        type_to = 717,
      });

      list.Add(new()
      {
        name = "Có thành viên rời khỏi kế hoạch",
        type_from = 718,
        type_to = 718,
      });

      list.Add(new()
      {
        name = "Kế hoạch được chuyển trạng thái",
        type_from = 720,
        type_to = 720,
      });

      list.Add(new()
      {
        name = "Kế hoạch có bình luận mới được nhắc tên",
        type_from = 721,
        type_to = 721,
      });



      // CFRs
      list.Add(new()
      {
        name = "Cấp sao và trừ sao",
        type_from = 900,
        type_to = 900,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Được ghi nhận",
        type_from = 901,
        type_to = 901,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Được tặng sao",
        type_from = 902,
        type_to = 902,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Ghi nhận và tặng sao được thả tim",
        type_from = 903,
        type_to = 903,
        is_default = true,
      });

      list.Add(new()
      {
        name = "Thông báo tạo chu kỳ",
        type_from = 1000,
        type_to = 1000,
      });

      list.Add(new()
      {
        name = "Thông báo chỉnh sửa chu kỳ",
        type_from = 1001,
        type_to = 1001,
      });

      list.Add(new()
      {
        name = "Thông báo xoá chu kỳ",
        type_from = 1002,
        type_to = 1002,
      });

      list.Add(new()
      {
        name = "Thông báo tạo chỉ số",
        type_from = 1003,
        type_to = 1003,
      });

      list.Add(new()
      {
        name = "Thông báo sửa chỉ số",
        type_from = 1004,
        type_to = 1004,
      });

      list.Add(new()
      {
        name = "Thông báo xoá chỉ số",
        type_from = 1005,
        type_to = 1005,
      });

      list.Add(new()
      {
        name = "Thông báo tạo kpis cho người quản lý và người xem",
        type_from = 1006,
        type_to = 1006,
      });

      list.Add(new()
      {
        name = "Thông báo tạo kpis cho người giám sát",
        type_from = 1007,
        type_to = 1007,
      });

      list.Add(new()
      {
        name = "Thông báo tạo kpis cho người thực hiện",
        type_from = 1008,
        type_to = 1008,
      });

      list.Add(new()
      {
        name = "Thông báo chỉnh sửa kpis cho người quản lý và người xem",
        type_from = 1009,
        type_to = 1009,
      });

      list.Add(new()
      {
        name = "Thông báo chỉnh sửa kpis cho người giám sát",
        type_from = 1010,
        type_to = 1010,
      });

      list.Add(new()
      {
        name = "Thông báo chỉnh sửa kpis cho người thực hiện",
        type_from = 1011,
        type_to = 1011,
      });

      list.Add(new()
      {
        name = "Thông báo xoá kpis cho người quản lý và người xem",
        type_from = 1012,
        type_to = 1009,
      });

      list.Add(new()
      {
        name = "Thông báo xoá kpis cho người giám sát",
        type_from = 1013,
        type_to = 1010,
      });

      list.Add(new()
      {
        name = "Thông báo xoá kpis cho người thực hiện",
        type_from = 1014,
        type_to = 1014,
      });

      list.Add(new()
      {
        name = "Thông báo gửi check-in",
        type_from = 1015,
        type_to = 1015,
      });

      list.Add(new()
      {
        name = "Thông báo cập nhật check-in",
        type_from = 1016,
        type_to = 1016,
      });

      list.Add(new()
      {
        name = "Thông báo xác nhận check-in",
        type_from = 1017,
        type_to = 1017,
      });

      // cây KPIs trong chu kỳ
      list.Add(new()
      {
        name = "Thông báo tạo cây KPIs cho quản lý chu kỳ",
        type_from = 1018,
        type_to = 1018,
      });
      list.Add(new()
      {
        name = "Thông báo tạo cây KPIs cho quản lý cây KPIs",
        type_from = 1019,
        type_to = 1019,
      });
      list.Add(new()
      {
        name = "Thông báo tạo cây KPIs cho người xem cây KPIs",
        type_from = 1020,
        type_to = 1020,
      });
      list.Add(new()
      {
        name = "Thông báo chỉnh sửa cây KPIs",
        type_from = 1021,
        type_to = 1021,
      });
      list.Add(new()
      {
        name = "Thông báo xoá cây KPIs",
        type_from = 1022,
        type_to = 1022,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận KPIs nhập liệu của mình",
        type_from = 1023,
        type_to = 1023,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận KPIs tự động của mình",
        type_from = 1024,
        type_to = 1024,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận quản lý phía trên",
        type_from = 1025,
        type_to = 1025,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận quản lý chu kỳ, quản lý cây kpis, người xem",
        type_from = 1026,
        type_to = 1026,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận nhắc tên KPIs nhập liệu của mình",
        type_from = 1027,
        type_to = 1027,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận nhắc tên KPIs tự động của mình",
        type_from = 1028,
        type_to = 1028,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận nhắc tên quản lý phía trên",
        type_from = 1029,
        type_to = 1029,
      });
      list.Add(new()
      {
        name = "Thông báo bình luận nhắc tên quản lý chu kỳ, quản lý cây kpis, người xem",
        type_from = 1030,
        type_to = 1030,
      });
      return list;
    }
  }
}
