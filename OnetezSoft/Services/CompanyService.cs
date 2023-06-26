using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class CompanyService
  {
    /// <summary>
    /// Cập nhật số lượng người dùng sản phẩm
    /// </summary>
    public static async Task<CompanyModel> UpdateProductAccess(string companyId)
    {
      var company = await DbMainCompany.Get(companyId);
      if (company != null)
      {
        var members = DbUser.GetAll(company.id);

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
    public static async Task AddStaff(CompanyModel company, UserModel user)
    {
      // Liên kết tài khoản với công ty
      if (user.companys == null)
        user.companys = new();
      if (user.companys.Any(x => x.id == company.id) == false)
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
            checkEmail.products = user.products;
            checkEmail.active = true;
            checkEmail.delete = false;
            await DbUser.Update(company.id, checkEmail);
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
        {
          // Có tài khoản rồi
          return;
        }
      }
      else
      {
        // Chưa có tài khoản trong tổ chức
        await DbUser.Create(company.id, user);
      }
      // Cập nhật dữ liệu chính
      await DbMainUser.Update(user);
    }
  }
}