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
      return new MemberModel ()
      {
        id = user.id,
        name = user.FullName,
        email = user.email,
        avatar = user.avatar,
      };
    }
  }
}