using OnetezSoft.Data;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class ProductService
  {
    /// <summary>
    /// Kiểm tra người dùng có được phép truy cập sản phẩm hay không
    /// </summary>
    /// <param name="company">Công ty</param>
    /// <param name="user">Người dùng</param>
    /// <param name="productId">ID sản phẩm cần kiểm tra</param>
    public static bool CheckAccess(CompanyModel company, UserModel user, string productId, out string message)
    {
      message = string.Empty;
      if (company != null && company.products != null)
      {
        if (productId == "educate")
          productId = "train";
        if (productId == "okr")
          productId = "okrs";
        if (productId == "kpi")
          productId = "kpis";
        if (productId == "hrm")
          productId = "timekeeping";
        // Kiểm tra tổ chức có sản phẩm này không
        var product = company.products.FirstOrDefault(x => x.id == productId);
        if (product != null)
        {
          if (product.active)
          {
            if (product.end >= DateTime.Today.Ticks)
            {
              if (user != null && user.products != null)
              {
                if (CheckAccess(user.products, productId))
                  return true;
                else
                  message = "Bạn không được cho phép sử dụng sản phẩm này.";
              }
              else
                message = "Bạn không được cho phép sử dụng sản phẩm này.";
            }
            else
              message = "Sản phẩm này của tổ chức đã hết hạn.";
          }
          else
            message = "Sản phẩm này của tổ chức đã bị vô hiệu hóa.";
        }
        else
          message = "Tổ chức không sở hữu sản phẩm này.";
      }
      else
        message = "Tổ chức không sở hữu sản phẩm này.";

      return false;
    }

    /// <summary>
    /// Kiểm tra một người dùng có được phép truy cập sản phẩm hay không ?
    /// </summary>
    /// <param name="products">Sản phẩm được sử dụng</param>
    /// <param name="productId">ID sản phẩm cần kiểm tra</param>
    public static bool CheckAccess(List<string> products, string productId)
    {
      if (products == null)
        return false;
      return products.Contains(productId);
    }


    /// <summary>
    /// Kiểm tra tổ chức có được sử dụng sản phẩm fStorage hay không?
    /// </summary>
    /// <param name="company">Công ty</param>
    public static bool CheckStorage(CompanyModel company)
    {
      if (company != null && company.products != null)
      {
        // Kiểm tra tổ chức có sản phẩm này không
        var product = company.products.SingleOrDefault(x => x.id == "storage");
        if (product != null && product.active && product.end >= DateTime.Today.Ticks)
          return true;
      }

      return false;
    }


    /// <summary>
    /// Kiểm tra tổ chức có được sử dụng sản phẩm fStorage hay không?
    /// </summary>
    /// <param name="company">Công ty</param>
    public static bool CheckStorage(string companyId, out string message)
    {
      message = string.Empty;
      var company = DbMainCompany.GetById(companyId);
      if (company != null && company.products != null)
      {
        // Kiểm tra tổ chức có sản phẩm này không
        var product = company.products.SingleOrDefault(x => x.id == "storage");
        if (product != null)
        {
          if (product.active)
          {
            if (product.end >= DateTime.Today.Ticks)
            {
              if (product.used < product.total * 1024)
                return true;
              else
                message = "Tổ chức đã sử dụng hết dung lượng lưu trữ";
            }
            else
              message = "Sản phẩm fStorage của tổ chức đã hết hạn.";
          }
          else
            message = "Sản phẩm fStorage của tổ chức đã bị vô hiệu hóa.";
        }
        else
          message = "Tổ chức không sở hữu sản phẩm fStorage.";
      }
      else
        message = "Tổ chức không sở hữu sản phẩm fStorage.";

      return false;
    }


    /// <summary>
    /// Tính chi phí gia hạn sản phẩm
    /// </summary>
    /// <param name="staff">Số người nâng cấp thêm</param>
    /// <param name="price">Giá bán theo người/tháng</param>
    /// <param name="month">Số tháng gia hạn</param>
    public static long CalculateRenew(int staff, long price, int month)
    {
      long money = 0;

      if (staff > 0 && month > 0)
      {
        money = staff * month * price;
      }

      return money;
    }

    /// <summary>
    /// Tính chi phí nâng cấp sản phẩm
    /// </summary>
    /// <param name="staff">Số người nâng cấp thêm</param>
    /// <param name="price">Giá bán theo người/tháng</param>
    /// <param name="end">Thời gian hết hạn của gói hiện tại</param>
    public static long CalculateUpgrade(int staff, long price, long end)
    {
      long money = 0;

      if (staff > 0 && end > DateTime.Today.Ticks)
      {
        // Số ngày nâng cấp
        int totalDay = new DateTime(end).Subtract(DateTime.Today).Days;
        // Đơn giá theo ngày
        double priceDay = (double)price / 30;
        // Tính chi phí
        money = Convert.ToInt64(staff * totalDay * priceDay);
      }

      return money;
    }


    /// <summary>
    /// Lấy chương trình khuyến mãi khả dụng
    /// </summary>
    /// <param name="type">1: thời gian | 2: người dùng</param>
    /// <param name="condition">Điều kiện</param>
    public static async Task<int> Promotion(int type, int condition)
    {
      int discount = 0;

      if (condition > 0)
      {
        var promotions = await DbMainPromotion.GetList(type);
        var result = promotions.Where(x => x.condition <= condition).FirstOrDefault();
        if (result != null)
          discount = result.discount;
      }

      return discount;
    }
  }
}