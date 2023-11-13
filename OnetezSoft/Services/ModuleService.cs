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
        "okrs",
        "okr",
        "cfr",
        "work",
        "todolist",
        "educate",
        "train",
        "kaizen",
        "timekeeping",
        "hrm",
        "storage",
        "kpis",
        "kpi",
      };
    }

    public static List<NavModel> GetList(CompanyModel company, UserModel user, bool isAdmin = false)
    {
      var ListNav = new List<NavModel>
      {
        new NavModel
        {
          name = "Tin tức",
          link = "blogs",
          icon = "blogs",
        }
      };

      var report = new NavModel
      {
        name = "Thống kê",
        link = "reports/achievement",
        icon = "report",
        childs = new(),
      };

      if (CheckAccess(company, user, "okrs") || isAdmin)
        report.childs.Add(new NavModel
        {
          name = "OKRs",
          link = "reports/okrs",
          icon = "track_changes",
          childs = new List<NavModel>()
              {
                new NavModel()
                {
                    name = "Tổng quan OKRs",
                    link = "reports/okrs/overview",
                },
                new NavModel()
                {
                    name = "Check-in OKRs",
                    link = "reports/okrs/checkin",
                }
              }
        });
      if (CheckAccess(company, user, "cfr") || isAdmin)
        report.childs.Add(new NavModel
        {
          name = "Ghi nhận",
          link = "reports/cfrs",
          icon = "face",
          childs = new List<NavModel>()
        });
      if (CheckAccess(company, user, "work") || isAdmin)
        report.childs.Add(new NavModel
        {
          name = "Kế hoạch",
          link = "reports/work",
          icon = "work",
        });
      if (CheckAccess(company, user, "todolist") || isAdmin)
        report.childs.Add(new NavModel
        {
          name = "Todolist",
          link = "reports/todolist",
          icon = "task",
          childs = new List<NavModel>()
              {
                new NavModel()
                {
                    name = "Kỷ luật",
                    link = "reports/todolist/fine",
                },
                new NavModel()
                {
                    name = "Tỷ lệ hoàn thành",
                    link = "reports/todolist/done",
                }
              }
        });
      if (CheckAccess(company, user, "kaizen") || isAdmin)
        report.childs.Add(new NavModel
        {
          name = "Kaizen",
          link = "reports/kaizen",
          icon = "trending_up",
        });

      report.childs.Add(new NavModel
      {
        name = "Thành tựu",
        link = "reports/achievement",
        icon = "emoji_events",
      });

      ListNav.Add(report);

      ListNav.Add(new NavModel
      {
        name = "Đội nhóm",
        link = "teams",
        icon = "group"
      });

      ListNav.Add(new NavModel
      {
        name = "Todolist",
        link = "todolist",
        icon = "todolist",
        childs = new List<NavModel>()
        {
          new NavModel
          {
              name = "Công việc của tôi",
              link = "todolist",
              icon = "task",
          },
          new NavModel
          {
              name = "Công việc được giao",
              link = "todolist/receive",
              icon = "add_task",
          },
          new NavModel
          {
              name = "Giao việc",
              link = "todolist/send",
              icon = "assignment",
          },
        }
      });

      ListNav.Add(new NavModel
      {
        name = "Kế hoạch",
        link = "work",
        icon = "work"
      });

      ListNav.Add(new NavModel
      {
        name = "OKRs",
        link = "okr/checkin",
        icon = "okrs",
        childs = new List<NavModel>()
               {
                  new NavModel()
                  {
                     name = "Thiết lập",
                     link = "okr/setup",
                     icon = "settings",
                     childs = new List<NavModel>()
                     {
                        new NavModel()
                        {
                           name = "Timeline",
                           link = "okr/setup/timeline",
                        },
                        new NavModel()
                        {
                           name = "Góp ý mục tiêu",
                           link = "okr/setup/suggest",
                        },
                        new NavModel()
                        {
                           name = "Công bố mục tiêu",
                           link = "okr/setup/target",
                        },
                        new NavModel()
                        {
                           name = "Tạo OKRs",
                           link = "okr/setup/detail",
                        },
                     }
                  },
                  new NavModel()
                  {
                     name = "Hành động",
                     link = "okr/tasks",
                     icon = "task",
                  },
                  new NavModel()
                  {
                     name = "Tổng quan",
                     link = "okr/overview",
                     icon = "account_tree",
                  },
                  new NavModel()
                  {
                     name = "Check in",
                     link = "okr/checkin",
                     icon = "fact_check",
                  },
                  new NavModel()
                  {
                     name = "Đánh giá",
                     link = "okr/review",
                     icon = "reviews",
                  },
               },
      });

      ListNav.Add(new NavModel
      {
        name = "Ghi nhận",
        link = "cfr",
        icon = "cfrs"
      });

      ListNav.Add(new NavModel
      {
        name = "Kaizen",
        link = "kaizen",
        icon = "kaizen"
      });

      ListNav.Add(new NavModel
      {
        name = "Đổi quà",
        link = "gift/product",
        icon = "gift",
        childs = new List<NavModel>()
               {
                  new NavModel()
                  {
                     name = "Cửa hàng",
                     link = "gift/product",
                     icon = "storefront"
                  },
                  new NavModel()
                  {
                     name = "Lịch sử đổi quà",
                     link = "gift/exchange",
                     icon = "history",
                     childs = new List<NavModel>()
                     {
                       new NavModel()
                        {
                           name = "Đổi quà",
                           link = "gift/exchange/pay",
                        },
                        new NavModel()
                        {
                           name = "Quà nhận được",
                           link = "gift/exchange/receive",
                        },
                     }
                  }
               }
      });

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

      ListNav.Add(new NavModel
      {
        name = "KPIs",
        link = "kpis/person",
        icon = "kpis",
        childs = new List<NavModel>
        {
          new NavModel { name = "KPIs của tôi", link = "kpis/person", icon="trending_up" },
          new NavModel
          {
            name = "KPIs quản lý",
            link = "kpis/manager",
            icon="manage_accounts" ,
            childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Danh sách",
                link = "kpis/manager/list",
              },
              new NavModel()
              {
                name = "Chờ xác nhận",
                link = "kpis/manager/confirm",
              }
            }
          },
          new NavModel { name = "Cây KPIs", link = "kpis/root", icon="account_tree" }
        }
      });

      ListNav.Add(new NavModel
      {
        name = "Đào tạo",
        link = "educate/course/list",
        icon = "educate",
        childs = new List<NavModel>()
               {
                  new NavModel()
                  {
                     name = "Khóa học",
                     link = "educate/course/list",
                     icon = "class"
                  },
                  new NavModel()
                  {
                     name = "Quản lý khóa học",
                     link = "educate/course/manager",
                     icon = "manage_search"
                  },
                  new NavModel()
                  {
                     name = "Chấm thi",
                     link = "educate/exam/manager",
                     icon = "fact_check"
                  },
                  new NavModel()
                  {
                     name = "Cấp chứng chỉ",
                     link = "educate/certificate/manager",
                     icon = "approval"
                  },
                  new NavModel()
                  {
                     name = "Chứng chỉ đạt được",
                     link = "educate/certificate/list",
                     icon = "workspace_premium"
                  }
               }
      });

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
              new NavModel()
              {
                name = "Thiết lập ngày nghỉ",
                icon = "DayOff",
                link = "configs/structure/dayoff",
              },
            }
        });
      }

      if (CheckAccess(company, user, "okrs") && (user.role == 1 || user.role_manage.okrs))
      {
        nav.childs.Add(new NavModel()
        {
          name = "OKRs",
          link = "configs/okrs",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Chu kỳ OKRs",
                icon = "OkrCycles",
                link = "configs/okrs/cycles",
              },
              new NavModel()
              {
                name = "Phiếu góp ý",
                icon = "OkrSuggests",
                link = "configs/okrs/suggests",
              },
            }
        });
      }

      if (CheckAccess(company, user, "cfr") && (user.role == 1 || user.role_manage.cfr))
      {
        nav.childs.Add(new NavModel()
        {
          name = "Ghi nhận",
          link = "configs/cfr",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Tiêu chí ghi nhận",
                icon = "CfrEvaluates",
                link = "configs/cfr/evaluates",
              },
              new NavModel()
              {
                name = "Cấp sao",
                icon = "CfrStarList",
                link = "configs/cfr/star?add=true",

              },
              new NavModel()
              {
                name = "Trừ sao",
                icon = "CfrStarList",
                link = "configs/cfr/star?add=false",
              },
            }
        });
      }

      if (CheckAccess(company, user, "todolist") && (user.role == 1 || user.role_manage.todolist))
      {
        nav.childs.Add(new NavModel()
        {
          name = "Todolist",
          icon = "Todolist",
          link = "configs/todolist",
        });
      }

      if (CheckAccess(company, user, "kaizen") && (user.role == 1 || user.role_manage.kaizen))
      {
        nav.childs.Add(new NavModel()
        {
          name = "Kaizen",
          icon = "Kaizen",
          link = "configs/kaizen",
        });
      }

      if (user.role == 1 || user.role_manage.store)
      {
        nav.childs.Add(new NavModel()
        {
          name = "Đổi quà",
          icon = "configs/gift",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Cửa hàng",
                icon = "GiftProducts",
                link = "configs/gift/product",
              },
              new NavModel()
              {
                name = "Đơn hàng",
                icon = "GiftExchange",
                link = "configs/gift/exchange",
              },
              new NavModel()
              {
                name = "Danh mục",
                icon = "GiftCategory",
                link = "configs/gift/category",
              },
              new NavModel()
              {
                name = "Ưu đãi",
                icon = "GiftBanner",
                link = "configs/gift/banner",
              },
            }
        });
      }

      if (CheckAccess(company, user, "train") && (user.role == 1 || user.role_manage.educate))
      {
        nav.childs.Add(new NavModel()
        {
          name = "Đào tạo",
          icon = "configs/educate",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Danh mục",
                icon = "EducateCategory",
                link = "configs/educate/category",
              },
              new NavModel()
              {
                name = "Mẫu chứng chỉ",
                icon = "EducateCertificate",
                link = "configs/educate/certificate",
              },
            }
        });
      }

      if (user.role == 1 || user.role_manage.other)
      {
        nav.childs.Add(new NavModel()
        {
          name = "Tiện ích",
          icon = "configs/other",
          childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Banner",
                icon = "Banners",
                link = "configs/other/banner",
              },
              new NavModel()
              {
                name = "Tin tức",
                icon = "Blogs",
                link = "configs/other/blogs",
              },
              new NavModel()
              {
                name = "Châm ngôn",
                icon = "Quotes",
                link = "configs/other/quotes",
              },
              new NavModel()
              {
                name = "Thành tựu",
                icon = "Achievement",
                link = "configs/other/achievement",
              }
            }
        });
      }

      if (ProductService.CheckStorage(company))
      {
        if (user.role == 1 || user.role_manage.storage)
        {
          nav.childs.Add(new NavModel()
          {
            name = "Quản lý lưu trữ",
            icon = "configs/storage",
            childs = new List<NavModel>()
            {
              new NavModel()
              {
                name = "Quản lý lưu trữ",
                icon = "StorageFiles",
                link = "configs/storage/store",
              },
               new NavModel()
              {
                name = "Lịch sử xóa file",
                icon = "StorageLogs",
                link = "configs/storage/history",
              },
            }
          });
        }
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

      if (CheckAccess(company, user, "kpis") && (user.role == 1 || user.role_manage.kpis))
      {
        nav.childs.Add(new NavModel
        {
          name = "KPIs",
          link = "config/kpis",
          childs = new()
            {
              new NavModel() { name = "Chu kỳ", link = "configs/kpis/cycle", icon = "KpisCycleList"  },
              new NavModel() { name = "Chỉ số KPIs", link = "configs/kpis/metric", icon = "KpisMetricList" }
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
