using OnetezSoft.Handled;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Linq;

namespace OnetezSoft.Services
{
  public static class ModuleService
  {
    public static List<string> GetModules()
    {
      return new List<string>()
      {
        "timekeeping",
        "hrm",
      };
    }

    public static List<NavModel> GetList(CompanyModel company, UserModel user, bool isAdmin = false)
    {
      var ListNav = new List<NavModel>();

      var timekeeping = new NavModel
      {
        name = "Chấm công",
        link = "hrm/timekeeping",
        icon = "checkin",
        childs = new(),
      };

      timekeeping.childs.Add(new NavModel
      {
        name = "Phân ca",
        link = "hrm/timelist",
        icon = "assignment",
        childs = new List<NavModel>()
        {
                       new NavModel()
                        {
                           name = "Phân ca",
                           link = "hrm/timelist/shift",
                        },
                        new NavModel()
                        {
                           name = "Thống kê đăng ký ca làm",
                           link = "hrm/timelist/report",
                        },
                        new NavModel()
                        {
                           name = "Đăng ký ca làm",
                           link = "hrm/timelist/register",
                        },
                     }
      });

      timekeeping.childs.Add(new NavModel
      {
        name = "Đơn từ",
        link = "hrm/form",
        icon = "feed",
      });

      timekeeping.childs.Add(new NavModel
      {
        name = "Lịch sử chấm công",
        link = "hrm/timekeeping",
        icon = "pending_actions",
      });

      if (CheckAccess(company, user, "timekeeping") && (user.role == 1 || user.role_manage.timekeeping))
      {
        timekeeping.childs.Add(new NavModel
        {
          name = "Thống kê chấm công",
          link = "hrm/statistical",
          icon = "leaderboard",
        });
      }


      timekeeping.childs.Add(new NavModel
      {
        name = "Bảng công",
        link = "hrm/timesheets",
        icon = "description",
      });

      ListNav.Add(timekeeping);

      return ListNav;
    }

    public static NavModel GetByModule(string name, CompanyModel company, UserModel user)
    {
      var list = GetList(company, user);
      if (name.IsEmpty())
        return new();

      name = name.ToLower();

      return list.FirstOrDefault(x => name.Contains(x.link.ToLower()) || (!x.link.IsEmpty() && x.link.ToLower().Contains(name)));
    }

    public static NavModel GetConfig(CompanyModel company, UserModel user)
    {
      var nav = new NavModel()
      {
        name = "Cấu hình",
        link = "configs",
        icon = "config",
      };

      if (user.role == 1 || user.role_manage.system)
      {
        nav.childs.Add(new NavModel()
        {
          name = "Cơ cấu",
          link = "configs/structure",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Phòng ban",
                link = "configs/structure/department",
                icon = "DepartmentList",
              },
              new NavModel()
              {
                name = "Nhân sự",
                icon = "UserList",
                link = "configs/structure/users",
              },
            }
        });
      }

      if (CheckAccess(company, user, "timekeeping") && (user.role == 1 || user.role_manage.timekeeping))
      {
        nav.childs.Add(new NavModel()
        {
          name = "Chấm công",
          icon = "configs/timekeeping",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Địa điểm",
                icon = "LocationList",
                link = "configs/timekeeping/location",
              },
              new NavModel()
              {
                name = "Phân địa điểm",
                icon = "LocationAssign",
                link = "configs/timekeeping/locationassign",
              },
              new NavModel()
              {
                name = "Ca làm",
                icon = "WorkShiftList",
                link = "configs/timekeeping/workshift",
              },
              new NavModel()
              {
                name = "Quy định",
                icon = "Rules",
                link = "configs/timekeeping/rules",
              },
              new NavModel()
              {
                name = "Ngày nghỉ",
                icon = "HrmDayOff",
                link = "configs/timekeeping/dayoff",
              },
              new NavModel()
              {
                name = "Thiết bị",
                icon = "DeviceManagement",
                link = "configs/timekeeping/device",
              }
            }
        });
      }
      return nav;
    }

    public static string GetIcon(string link, CompanyModel company, UserModel user)
    {
      var list = GetList(company, user);
      list.Add(new NavModel()
      {
        name = "Cấu hình",
        link = "configs",
        icon = "config",
      });

      var item = list.FirstOrDefault(x => x.link.Contains(link));
      return item == null ? "" : item.icon;
    }

    private static bool CheckAccess(CompanyModel company, UserModel user, string productId)
    {
      return ProductService.CheckAccess(company, user, productId, out string message);
    }
  }
}
