using DocumentFormat.OpenXml.Spreadsheet;
using OnetezSoft.Data;
using OnetezSoft.Models;
using OnetezSoft.Pages.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class CompanyService
  {
    private static string name;

    /// <summary>
    /// Cập nhật số lượng người dùng sản phẩm
    /// </summary>
    public static async Task<CompanyModel> UpdateProductAccess(string companyId, GlobalService globalService)
    {
      var company = await DbMainCompany.Get(companyId);
      if (company != null)
      {
        var members = await DbUser.GetAll(company.id, globalService);

        foreach (var product in company.products)
        {
          product.used = members.Where(x => x.products.Contains(product.id)).Count();
        }

        company.members = members.Count;
        await DbMainCompany.Update(company);
      }
      return company;
    }

    /// <summary>
    /// Thêm tài khoản vào tổ chức
    /// </summary>
    /// <param name="company">Tổ chức</param>
    /// <param name="user">Tài khoản</param>
    public static async Task AddStaff(CompanyModel company, UserModel user, GlobalService globalService)
    {
      // Liên kết tài khoản với công ty
      user.companys ??= new();
      if (!user.companys.Any(x => x.id == company.id))
        user.companys.Add(new UserModel.Company { id = company.id, name = company.name });

      // Tạo User của công ty
      var checkEmail = await DbUser.GetDelete(company.id, null, user.email);
      if (checkEmail != null)
      {
        // Có rồi nhưng bị xóa
        if (checkEmail.delete)
        {
          if (checkEmail.id == user.id)
          {
            checkEmail.page_default = null;
            checkEmail.products = user.products;
            checkEmail.active = true;
            checkEmail.delete = false;
            await DbUser.Update(company.id, checkEmail, globalService);
          }
          else
          {
            // Xóa tài khoản trùng
            await DbUser.Delete(company.id, checkEmail.id);
            // Tạo lại tài khoản
            await DbUser.Create(company.id, user);
          }
        }
        else
          return;
      }
      else
        await DbUser.Create(company.id, user);

      await DbMainUser.Update(user, globalService);
    }

    /// <summary>
    /// Cập nhật dung lượng dữ liệu sử dụng
    /// </summary>
    public static async Task UpdateStorageUsed(string companyId)
    {
      try
      {
        var company = await DbMainCompany.Get(companyId);
        if (company != null)
        {
          // Gói lưu trữ hiện tại
          var storage = company.products.FirstOrDefault(x => x.id == "storage");
          if (storage != null)
          {
            long unit = 1024;
            long dataUsed = await StorageService.GetStorageUsed(companyId);

            storage.used = Convert.ToInt32(dataUsed / (unit * 1000));
            await DbMainCompany.Update(company);
            Console.WriteLine(string.Format("Storage {0}: {1}/{2} MB", companyId, storage.used, storage.total * 1000));
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }
    /// <summary>
    /// Hàm tạo tổ chức theo mẫu
    /// </summary>
    /// <param name="id">ID công ty</param>
    /// <param name="name">Tên công ty</param>
    /// <param name="modules">Danh sách modules cần kích hoạt</param>
    /// <returns></returns>
    public static async Task<bool> InitCompanyTemplate(string id, string name, List<string> modules, List<UserModel> users, GlobalService globalService)
    {
      // Mẫu phòng ban
      var departments = new List<DepartmentModel>();
      var randomParent = Mongo.RandomId();
      var membersDepartment = new List<DepartmentModel.MembersList>();
      var membersId = new List<string>();
      foreach (var user in users)
      {
        membersDepartment.Add(new DepartmentModel.MembersList { id = user.id, role = 3 });
        membersId.Add(user.id);
        user.departments_id.Add(randomParent);
        user.departments_name = "Công ty A";
        await DbUser.Update(id, user, globalService);
      }
      departments.Add(new DepartmentModel { id = randomParent, name = "Công ty A", parent = null, manager = "Giám đốc", deputy = "Phó giám đốc", pos = 0, members_id = membersId, members_list = membersDepartment });
      departments.Add(new DepartmentModel { name = "Phòng ban 1", parent = randomParent, manager = "Trưởng phòng", deputy = "Phó phòng", pos = 0, members_id = new(), members_list = new() });
      departments.Add(new DepartmentModel { name = "Phòng ban 2", parent = randomParent, manager = "Trưởng phòng", deputy = "Phó phòng", pos = 0, members_id = new(), members_list = new() });
      departments.Add(new DepartmentModel { name = "Phòng ban 3", parent = randomParent, manager = "Trưởng phòng", deputy = "Phó phòng", pos = 0, members_id = new(), members_list = new() });

      foreach (var department in departments)
      {
        await DbDepartment.Create(id, department);
      }

      // Mẫu ngày nghi
      var holiday = new DayOffModel
      {
        name = "Chủ nhật",
        start = DateTime.Today.Ticks,
        end = DateTime.Today.AddDays(7).Ticks,
        create = DateTime.Now.Ticks,
        loop = 2,
        loop_week = new DayOffModel.Week
        {
          mon = false,
          tue = false,
          wed = false,
          thu = false,
          fri = false,
          sat = false,
          sun = true
        },
        has_salary = false,
      };
      await DbDayOff.Create(id, holiday);


      // Mẫu châm ngôn
      var quotes = new List<QuotesModel>
      {
        new QuotesModel { name = "Attitude is Everything", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Đừng để lại ngày mai những thứ bạn có thể làm hôm nay.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Bạn làm thì có thể thất bại nhưng nếu không làm thì chắc chắn thất bại.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Nếu bạn không tìm ra cách kiếm tiền trong cả khi bạn đang ngủ, bạn sẽ phải làm việc cho đến khi chết.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Tất cả những giấc mơ rồi đều sẽ thành hiện thực, nếu bạn có đủ quyết tâm.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Sống phải có đam mê, tôi chưa thấy ai thành công mà không có đam mê của riêng mình.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Hôm nay thật khắc nghiệt, ngày mai còn khắc nghiệt hơn, nhưng nếu bạn cố gắng, ngày kia sẽ là ngày tươi sáng.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Hãy sợ hãi khi những người khác tham lam. Hãy tham lam khi những người khác sợ hãi.", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Hãy làm việc bằng chính tình yêu và lòng đam mê nghề, để mỗi ngày chúng ta cùng rong chơi với nó, chứ không phải \" đi làm \" !!", author = "Sưu tầm", date = DateTime.Now.Ticks },
        new QuotesModel { name = "Đừng lựa chọn an nhàn khi còn trẻ. Một ngày cực tuổi trẻ, bằng 10 ngày khoẻ tuổi già !", author = "Sưu tầm", date = DateTime.Now.Ticks }
      };

      foreach (var quote in quotes)
      {
        await DbQuotes.Create(id, quote);
      }


      // Mẫu banner chào mừng 
      var banner = new BannerModel
      {
        name = "Chào mừng Công ty " + name,
        link = "",
        image = "/images/demo/banner-chao-mung.png",
        department = "",
        pin = false,
        date = DateTime.Now.Ticks,
        pos = 0,
        pages = new()
      };
      await DbBanner.Create(id, banner);


      // fOkrs
      if (modules.Contains("okrs"))
      {
        var okr = await DbOkrConfig.Get(id);
        long startCycle = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).Ticks;
        long endCycle = new DateTime(startCycle).AddMonths(3).AddDays(-1).Ticks;
        string year = DateTime.Today.Year.ToString();

        var okrSuggests = new List<OkrConfigModel.Suggest>
        {
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Chuyển đổi số và áp dụng công nghệ nhiều hơn" },
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Gia tăng giá trị sản phẩm" },
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Gia tăng khách hàng (số lượng, trải nghiệm, giá trị đơn hàng...)" },
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Hệ thống quản trị tốt hơn" },
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Hệ thống vận hành (quy trình, quy định, biểu mẫu..) tốt hơn" },
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Nhân sự tốt hơn" },
          new OkrConfigModel.Suggest { id = Mongo.RandomId(), name = "Phát triển văn hóa doanh nghiệp tốt hơn" }
        };

        var okrCycles = new List<OkrConfigModel.Cycle>
        {
          new OkrConfigModel.Cycle { id = Mongo.RandomId(), name = "QUÝ DEMO " + year, start = startCycle, end = endCycle, done = false }
        };

        okr.suggests = okrSuggests;
        okr.cycles = okrCycles;
        await DbOkrConfig.Update(id, okr);
      }

      // fShop
      if (modules.Contains("fShop"))
      {
        // Mẫu danh mục cửa hàng
        var giftCategoryList = new List<StaticModel>
        {
          new StaticModel { id = Handled.Shared.RandomInt(100000, 999999), name = "Đồ ăn, thức uống", icon = "/images/demo/danh-muc-do-an.png" },
          new StaticModel { id = Handled.Shared.RandomInt(100000, 999999), name = "Áo quần", icon = "/images/demo/danh-muc-ao-quan.png" },
          new StaticModel { id = Handled.Shared.RandomInt(100000, 999999), name = "Văn phòng phẩm", icon = "/images/demo/danh-muc-vpp.png" },
          new StaticModel { id = Handled.Shared.RandomInt(100000, 999999), name = "Khác", icon = "/images/demo/danh-muc-khac.png" }
        };


        // Mẫu sản phẩm
        var giftProductList = new List<GiftProductModel>();
        giftProductList.Add(new GiftProductModel
        {
          name = "Trà sữa thơm ngon",
          category = giftCategoryList[0].id,
          price_list = 25,
          price_sale = 0,
          image = "/images/demo/san-pham-tra-sua.jpg",
          description = "",
          show = true,
          sold = 0
        });
        giftProductList.Add(new GiftProductModel
        {
          name = "PHONE 14 siêu hịn",
          category = giftCategoryList[3].id,
          price_list = 40000,
          price_sale = 0,
          image = "/images/demo/san-pham-iphone.jpeg",
          description = "",
          show = true,
          sold = 0
        });
        giftProductList.Add(new GiftProductModel
        {
          name = "Cờ đỏ sao vàng",
          category = giftCategoryList[1].id,
          price_list = 150,
          price_sale = 0,
          image = "/images/demo/san-pham-ao.jpg",
          description = "",
          show = true,
          sold = 0
        });
        giftProductList.Add(new GiftProductModel
        {
          name = "Cơm trưa vui vẻ",
          category = giftCategoryList[0].id,
          price_list = 30,
          price_sale = 0,
          image = "/images/demo/san-pham-com.jpg",
          description = "",
          show = true,
          sold = 0
        });
        giftProductList.Add(new GiftProductModel
        {
          name = "Bút bi chăm chỉ",
          category = giftCategoryList[1].id,
          price_list = 8,
          price_sale = 0,
          image = "/images/demo/san-pham-but-bi.jpg",
          description = "",
          show = true,
          sold = 0
        });

        foreach (var product in giftProductList)
        {
          await DbGiftProduct.Create(id, product);
        }

        var company = await DbMainCompany.Get(id);
        company.gift_category = giftCategoryList;
        await DbMainCompany.Update(company);
      }

      // fTrain
      if (modules.Contains("train"))
      {
        // Danh mục đào tạo 
        var educateCategoryList = new List<EducateCategoryModel>
        {
          new EducateCategoryModel { name = "Bán hàng", image = "/images/demo/danh-muc-ban-hang.png" },
          new EducateCategoryModel { name = "Hội nhập", image = "/images/demo/danh-muc-hoi-nhap.png" },
          new EducateCategoryModel { name = "Quản lý vận hành", image = "/images/demo/danh-muc-van-hanh-quan-ly.png" },
          new EducateCategoryModel { name = "Sản xuất", image = "/images/demo/danh-muc-san-xuat.png" },
          new EducateCategoryModel { name = "Tài chính", image = "/images/demo/danh-muc-tai-chinh.png" }
        };
        foreach (var category in educateCategoryList)
        {
          await DbEducateCategory.Create(id, category);
        }

        // Chứng chỉ đào tạo
        var educateCertificate = new EducateCertificateModel { name = "Chứng chỉ 1", image = "/images/demo/chung-chi.jpg", color = "", date = DateTime.Now.Ticks, pin = true };
        await DbEducateCertificate.Create(id, educateCertificate);
      }

      // fNews
      if (modules.Contains("fNews"))
      {
        var blogs = new List<BlogModel>
        {
          new BlogModel
          {
            name = "Chào mừng " + name + " đến với Fastdo!",
            desc = null,
            link = "",
            image = "/images/demo/poster-workdo.png",
            content = "<h2><strong style=\"color: rgb(0, 0, 0); background-color: transparent;\">Fastdo - Công ty hàng đầu cung cấp giải pháp quản trị cho doanh nghiệp SMEs tại Việt Nam!</strong></h2><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">Tự hào đồng hành với hơn 100+ SMEs trong quá trình chuyển đổi số, giúp họ đạt được sự tăng trưởng mạnh mẽ. Fastdo cam kết mang lại những giải pháp quản trị công việc tối ưu, mang lại trải nghiệm làm việc tuyệt vời cho nhân sự của bạn.</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">Hiện nay, bộ sản phẩm Fastdo xoay quanh các giải pháp về quản trị công việc bao gồm:</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">✅ fOKRs - Công cụ quản trị toàn diện các hoạt động của OKRs.</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">➡️ https://fastdo.vn/fokrs/</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">✅ fWork - Công cụ quản trị cho việc lập và quản lý kế hoạch.</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">➡️ https://fastdo.vn/fwork/</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">✅ fTodolist - Công cụ giúp theo sát công việc hàng ngày của nhân sự.</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">➡️ https://fastdo.vn/f-todolist/</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">🔖Ngoài ra còn có các phần mềm về Quản lý đào tạo nội bộ - fTrain, Ghi nhận, khen thưởng - fCFRs và tặng quà nội bộ fShop.&nbsp;</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">Chúng tôi có sẵn một đội ngũ chuyên gia 10+ kinh nghiệm trong lĩnh vực quản trị doanh nghiệp và công nghệ thông tin, luôn sẵn sàng tư vấn và hỗ trợ bạn trong mọi chặng đường phát triển.&nbsp;</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">👉Hãy để Fastdo đồng hành cùng với sự phát triển của doanh nghiệp bạn trong quá trình chuyển đổi số.</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">--------------------------------------------------------------------------------------------</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\">FASTDO - Nền tảng quản trị công việc cho SMEs</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\"><img src=\"https://lh6.googleusercontent.com/EAU9zKK-NIIjBiSAAKKwTxKsjw7tfVKC7o4ZhJW5VPgrj42YYPQWPBIzF7zq42ws2TIArD6z2U1E9a0tNjPleOyJt4C30zhLxi5pb5AuagcJzNg1nKhM7t20gaDRyqdHZZ76vGJ6ydugL6YTQmrIazE\" alt=\"🏠\" height=\"16\" width=\"16\">Văn phòng trụ sở Hà Nội: Tầng 6, số 11 Vương Thừa Vũ, Quận Thanh Xuân, Thành Phố Hà Nội</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\"><img src=\"https://lh4.googleusercontent.com/kyipIbk0epIRV-9obrMT2E1rl8KnUAH0pYW-bnDNlnuby6BeRItB9BJ2xCCUP9V0I8R3Vqzrg8ZFNOGV5ODvbRLkmHIAHjEzN3VJQsWmkZrjuW2dMweZamnQmTyZxIzLzn17U2VJrnyIu3y4mRD8LIY\" alt=\"🏠\" height=\"16\" width=\"16\">Văn phòng chi nhánh Đà Nẵng: Tầng 2, số 23 Trường Thi 1, Quận Hải Châu, Thành Phố Đà Nẵng</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\"><img src=\"https://lh4.googleusercontent.com/scBpv1wI4B9_LprnoFbrFi1kysPO2w7cZgUw8kbKunva6BOx_b-9a2u2tM1gdWrzBofU6TbEbKEG2fBJBUJ_CwcmaFnziTQv5Jrqa_aL-Lv2VuoQK_gHv40Hs2a6ADGuZRJvskqMOGdPMofV6JAE_sc\" alt=\"📞\" height=\"16\" width=\"16\">Hotline/Zalo: 0905 852 933</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\"><img src=\"https://lh4.googleusercontent.com/ktFq2w74v5Y__TeRf9wfCxgN3O3KEvIlRG0MtqvxH8WH_kdW4jLJE5VsMajVuc4xoCtg1yGVz4wo1aZ3Wp3BVyW11g7VJMJVjtNG8TlcDvYO4bI2jKqPQNgWUW1Y7F-h_ZX-u6V__-oXT4pF8S-lW1o\" alt=\"📩\" height=\"16\" width=\"16\">Email: support@fastdo.vn</span></p><p><span style=\"color: rgb(0, 0, 0); background-color: transparent;\"><img src=\"https://lh6.googleusercontent.com/Cl16FcAOxXloDXSDV9UNEjWVgeNuZTT_-Vu5KZZm4sy_NGiaITtPmsnV0sV7f2OwZQVLcq-jHOQ4ERawYt5jAYuaSvZN6HgMtOm4v6LG1UGJepxDADBNQWb5jRsyIsEj2_cqhI1KjNSdjqEGMLzRURg\" alt=\"🌏\" height=\"16\" width=\"16\">Website:</span><a href=\"https://l.facebook.com/l.php?u=https%3A%2F%2Ffastdo.vn%2F%3Ffbclid%3DIwAR29ZRf87AFpV54P5HYBm8ZSGjzRrtMNIOGSjijw7blWMX-cgSwqu8xvxQc&amp;h=AT0JeDY5c3px7xaPEXQVNq-TSe_Ljhe3g_gTkMlbt3PLFtVhBl9wvMTbtHsHkypdbOprU8ZiBNV2GBnomLU9AhChkjzOzM4aSFbEDKAI-yuS_DBhMuYesIU80B7reZtlWdvboaIVlUpt3qTue7oS&amp;__tn__=-UK-R&amp;c[0]=AT1J9dyK9oNxkyjz_QgmgawPIkIqccP9I86LUj8RFJeamNZ5lxmv3ltJSGPP896P5qnRa7WNVEbjgdyJohO5bNqEHZsFB31g0FctFn73O6ehe5VFRfBSsKOrzsuVMO_-QYu-k4R8TuvZBa6gB60oE_GOqJ7siPFoIFUTg2SUY7SBS4TvnBBtjJ5lBq9YOnHjVFhSaJyfPRh2\" target=\"_blank\" style=\"color: rgb(0, 0, 0); background-color: transparent;\"> https://fastdo.vn/</a></p><p><br></p>",
            department = randomParent,
            category = null,
            date = DateTime.Now.Ticks,
            created = 0,
            pos = 0,
            pin = false,
            is_show = true,
            is_all = true,
          }
        };

        foreach (var blog in blogs)
        {
          await DbBlog.Create(id, blog);
        }
      }

      // fCheckin
      if (modules.Contains("timekeeping"))
      {
        var shifts = new List<HrmWorkShiftModel>
        {
          new HrmWorkShiftModel { created = DateTime.Now.Ticks, name = "Ca sáng", checkin = "07:30", checkout = "12:00", value = 0.5, color = "#484848" },
          new HrmWorkShiftModel { created = DateTime.Now.Ticks, name = "Ca chiều", checkin = "13:30", checkout = "17:00", value = 0.5, color = "#484848" }
        };
        foreach (var shift in shifts)
        {
          await DbHrmWorkShift.Create(id, shift);
        }
      }

      // fKaizen
      if (modules.Contains("kaizen"))
      {
        var kaizens = new List<CompanyModel.Kaizen>
        {
          new CompanyModel.Kaizen { id = Mongo.RandomId(), name = "Gia tăng chất lượng sản phẩm", image = "/images/demo/kaizen-sanpham.png" },
          new CompanyModel.Kaizen { id = Mongo.RandomId(), name = "Gia tăng năng suất", image = "/images/demo/kaizen-nangsuat.png" },
          new CompanyModel.Kaizen { id = Mongo.RandomId(), name = "Cơ sở hạ tầng", image = "/images/demo/kaizen-csht.png" },
          new CompanyModel.Kaizen { id = Mongo.RandomId(), name = "Ý tưởng khác", image = "/images/demo/kaizen-idea.png" }
        };
        var company = await DbMainCompany.Get(id);
        company.kaizen = kaizens;
        await DbMainCompany.Update(company);
      }

      // fCfrs
      if (modules.Contains("cfr"))
      {
        var evaluates = new List<CfrEvaluateModel>
        {
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Tôi muốn ghi nhận bạn", star = 5, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Tôi muốn ghi nhận, cố gắng hơn bạn nhé", star = 10, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn đang làm tốt, tiếp tục phát huy nhé", star = 15, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Chúc mừng bạn đã hoàn thành công việc", star = 20, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn hỗ trợ đồng đội tuyệt vời", star = 30, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn đã có sự nỗ lực đáng ghi nhận", star = 40, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn thật sự là đồng đội tuyệt vời mà tôi có", star = 60, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn thật đẳng cấp, cách làm việc của bạn có ảnh hưởng đến sự phát triển của team", star = 100, type = 2, special = false },
          new CfrEvaluateModel { id = Mongo.RandomId(), name = "WOW, Thật tuyệt vời, bạn đã vượt xa kỳ vọng của tôi!", star = 150, type = 2, special = false }
        };

        foreach (var evaluate in evaluates)
        {
          await DbCfrEvaluate.Create(id, evaluate);
        }
      }

      return true;
    }
  }
}