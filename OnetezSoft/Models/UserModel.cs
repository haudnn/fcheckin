using MongoDB.Bson.Serialization.Attributes;
using OnetezSoft.Services;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static OnetezSoft.Models.UserModel;

namespace OnetezSoft.Models
{
  [BsonIgnoreExtraElements]
  public class UserModel
  {
    [BsonId]
    public string id { get; set; }

    public bool delete { get; set; }

    public bool active { get; set; }

    public string email { get; set; }

    public string phone { get; set; }

    public string social { get; set; }

    public string note { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string avatar { get; set; }

    public string password { get; set; }

    public string session { get; set; }

    public long online { get; set; }

    public long last_login { get; set; }

    public long create_date { get; set; }

    /// <summary>Admin hệ thống</summary>
    public bool is_admin { get; set; }

    /// <summary>Chủ tổ chức</summary>
    public bool is_customer { get; set; }

    /// <summary>Không thống kê</summary>
    public bool no_report { get; set; }

    /// <summary>Tự checkin OKRs</summary>
    public bool okr_checkin { get; set; }

    /// <summary>Nhận thông báo đổi quà</summary>
    public bool noti_store { get; set; } = true;

    /// <summary>Quyền trong công ty: 1. Admin, 2. QLHT, 3. Nhân viên</summary>
    public int role { get; set; }

    /// <summary>Chức danh: 1. quản lý, 2. phó quản lý, 3. nhân viên</summary>
    public int title { get; set; }
    /// <summary>Tên chức danh</summary>
    public string title_name { get; set; }

    /// <summary>Tên nghề nghiệp</summary>
    public string job_title { get; set; }

    /// <summary>Danh sách công ty trực thuộc</summary>
    public List<Company> companys { get; set; } = new();

    /// <summary>Quyền theo chức năng</summary>
    public RoleManage role_manage { get; set; }

    /// <summary>Danh sách ID phòng ban trực thuộc</summary>
    public List<string> departments_id { get; set; } = new();

    /// <summary>Danh sách ID phòng ban ghim trong đội nhóm</summary>
    public List<string> teams_id { get; set; } = new();

    /// <summary>Danh sách tên phòng ban trực thuộc</summary>
    public string departments_name { get; set; }

    /// <summary>Phòng ban mặc định cho các bộ lọc</summary>
    public string department_default { get; set; }

    /// <summary>Trực thuộc công ty</summary>
    public string company_id { get; set; }

    /// <summary>Trực thuộc phòng ban</summary>
    public string department_id { get; set; }

    /// <summary>Tổng số sao đang có</summary>
    public int star_total { get; set; }

    /// <summary>Tổng số sao được cấp</summary>
    public int star_distribute { get; set; }

    /// <summary>Tổng số sao nhận được </summary>
    public int star_receive { get; set; }

    /// <summary>Tổng sao cho đi</summary>
    public int star_give { get; set; }

    /// <summary>Trang mặc định khi vào phần mềm</summary>
    public string page_default { get; set; }

    ///// <summary>Không thống kê: OKRs</summary>
    //public bool no_report_okr { get; set; }

    /// <summary>Không thống kê: CFRs</summary>
    public bool no_report_cfr { get; set; }

    /// <summary>Không thống kê: Todolist</summary>
    public bool no_report_todolist { get; set; }

    ///// <summary>Không thống kê: Kaizen</summary>
    //public bool no_report_kaizen { get; set; }

    /// <summary>Không thống kê: Thành tựu</summary>
    public bool no_report_achievement { get; set; }

    /// <summary>Sản phẩm được sử dụng</summary>
    public List<string> products { get; set; } = new();

    /// <summary>Sản phẩm ưu thích</summary>
    public List<string> products_favorite { get; set; } = new();

    /// <summary>Số dư tài khoản</summary>
    public int balance { get; set; }

    /// <summary>Mã xác minh</summary>
    public string verify { get; set; }

    /// <summary>Các kế hoạch đã ghim</summary>
    public List<string> plans_pin { get; set; } = new();

    /// <summary>Các kế hoạch đã lưu trữ</summary>
    public List<string> plans_hide { get; set; } = new();

    /// <summary>Chế độ làm việc linh động</summary>
    public bool is_hybrid { get; set; }

    /// <summary>Cookie thiết bị/summary>
    public string device_id { get; set; }

    /// <summary>Tên thiết bị/summary>
    public string device_name { get; set; }

    //public List<string> sidebar_star { get; set; } = new();

    public Custom custom { get; set; } = new();


    /// <summary>Công ty trực thuộc</summary>
    public class Company
    {
      public string id { get; set; }
      public string name { get; set; }
    }

    [BsonIgnoreExtraElements]
    /// <summary>Quyền hạn chức năng</summary>
    public class RoleManage
    {
      /// <summary>Cơ cấu</summary>
      public bool system { get; set; }

      /// <summary>OKRs - CFRs</summary>
      public bool okrs { get; set; }

      /// <summary>Kaizen</summary>
      public bool kaizen { get; set; }

      /// <summary>Đào tạo</summary>
      public bool educate { get; set; }

      /// <summary>Đổi quà</summary>
      public bool store { get; set; }

      /// <summary>Tiện ích</summary>
      public bool other { get; set; }

      /// <summary>Hồ sơ nhân sự</summary>
      public bool hrrecords { get; set; }

      /// <summary>Chấm công</summary>
      public bool timekeeping { get; set; }

      /// <summary>Lưu trữ</summary>
      public bool storage { get; set; }

      /// <summary>Ghi nhận</summary>
      public bool cfr { get; set; }

      /// <summary>kpis</summary>
      public bool kpis { get; set; }

      /// summary>todolist</summary>
      public bool todolist { get; set; }
    }

    public string FullName
    {
      get { return $"{last_name} {first_name}"; }
    }
  }

  [BsonIgnoreExtraElements]
  /// <summary>Thông tin tài khoản</summary>
  public class MemberModel
  {
    public string id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string avatar { get; set; }

    /// <summary>Chức danh: 1. quản lý, 2. phó quản lý, 3. nhân viên</summary>
    public int title { get; set; }

    /// <summary>Danh sách ID phòng ban trực thuộc</summary>
    public List<string> departments_id { get; set; }
    /// <summary>Danh sách tên phòng ban trực thuộc</summary>
    public string departments_name { get; set; }

    /// <summary>Phòng ban mặc định cho các bộ lọc</summary>
    public string department_default { get; set; }

    /// <summary>Quyền trong công ty: 1. Admin, 2. QLHT, 3. Nhân viên</summary>
    public int role { get; set; }

    /// <summary>Quyền theo chức năng</summary>
    public RoleManage role_manage { get; set; }

    // <summary>Xử lý cho thống kê chấm công</summary>
    public int count { get; set; }
  }

  [BsonIgnoreExtraElements]
  public class Custom
  {
    public string okrs_cycle { get; set; }

    public string kpis_cycle { get; set; }

    public bool sidebar_pin { get; set; }

    public bool sidebar_fwork { get; set; } = true;

    public bool sheets_less { get; set; } = true;

    public List<NotificationService.NotiType> notifications { get; set; } = new();

    public bool email_notification { get; set; } = true;

    //public Beta beta_user { get; set; } = new();
  }
  [BsonIgnoreExtraElements]
  public class Beta
  {
    public bool accept { get; set; }

    public long update { get; set; }
  }
}