using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class UserService
  {
    /// <summary>
    /// Chuyển UserModel thành MemberModel
    /// </summary>
    public static MemberModel ConvertToMember(UserModel user)
    {
      return new MemberModel()
      {
        id = user.id,
        name = user.FullName,
        email = user.email,
        avatar = user.avatar,
      };
    }

    /// <summary>
    /// Lấy 1 tài khoản trong danh sách tài khoản theo ID
    /// </summary>
    public static UserModel GetUser(List<UserModel> list, string userId)
    {
      var user = list.SingleOrDefault(x => x.id == userId);
      if (user != null)
        return user;
      else
        return new UserModel()
        {
          //id = userId,
          last_name = "Tài khoản đã xóa",
          avatar = "/images/avatar.png"
        };
    }

    /// <summary>
    /// Lấy 1 tài khoản trong danh sách tài khoản theo ID
    /// </summary>
    public static MemberModel GetMember(List<UserModel> list, string userId)
    {
      var user = list.SingleOrDefault(x => x.id == userId);
      if (user != null)
        return ConvertToMember(user);
      else
        return new MemberModel()
        {
          //id = userId,
          name = "Tài khoản đã xóa",
          avatar = "/images/avatar.png"
        };
    }

    /// <summary>
    /// Danh sách nhân sự trong phòng ban, sắp xếp theo cấp bậc
    /// </summary>
    public static async Task<List<UserModel>> GetUserListInDepartment(string companyId, string departmentId)
    {
      var results = new List<UserModel>();
      var department = await DbDepartment.Get(companyId, departmentId);
      if (department != null)
      {
        var userList = await DbUser.GetAll(companyId, departmentId);
        foreach (var member in department.members_list)
        {
          var user = userList.SingleOrDefault(x => x.id == member.id);
          if (user != null)
            results.Add(user);
        }
      }
      return results;
    }
  }
}