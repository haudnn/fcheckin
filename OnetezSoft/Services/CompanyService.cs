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
      if(company != null)
      {
        var members = DbUser.GetAll(company.id);

        foreach(var product in company.products)
        {
          product.used = members.Where(x => x.products.Contains(product.id)).Count();
        }

        company.members = members.Count;
        await DbMainCompany.Update(company);
      }
      return company;
    }
  }
}