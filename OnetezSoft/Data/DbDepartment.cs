using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OnetezSoft.Data
{
  public class DbDepartment
  {
    private static string _collection = "departments";

    public static async Task<DepartmentModel> Create(string companyId, DepartmentModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      if (string.IsNullOrEmpty(model.id))
        model.id = Mongo.RandomId();
      if (model.members_id == null)
        model.members_id = new();
      if (model.members_list == null)
        model.members_list = new();
      model.delete = false;

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      await collection.InsertOneAsync(model);

      return model;
    }


    public static async Task<DepartmentModel> Update(string companyId, DepartmentModel model)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      var option = new ReplaceOptions { IsUpsert = false };

      var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

      return model;
    }


    public static async Task<DepartmentModel> Get(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      var result = await collection.Find(x => x.id == id && !x.delete).FirstOrDefaultAsync();

      if(result != null)
      {
        if (result.members_id == null)
          result.members_id = new();
        if (result.members_list == null)
          result.members_list = new();
      }
      
      return result;
    }


    public static async Task<bool> Delete(string companyId, string id)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      var result = await collection.DeleteOneAsync(x => x.id == id);

      if (result.DeletedCount > 0)
        return true;
      else
        return false;
    }


    public static List<DepartmentModel> GetAll(string companyId)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      var results = collection.Find(x => x.delete == false).ToList();

      return (from x in results orderby x.parent, x.pos, x.name select x).ToList();
    }


    public static List<DepartmentModel> GetList(string companyId, string parent)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      var results = collection.Find(x => x.delete == false && x.parent == parent).ToList();

      return (from x in results orderby x.parent, x.pos, x.name select x).ToList();
    }


    /// <summary>
    /// Danh sách phòng ban dùng cho Select
    /// </summary>
    public static List<DepartmentModel.SelectList> GetSelectList(string companyId, string parent, int level, 
      List<DepartmentModel> listAll)
    {
      if (listAll == null)
        listAll = GetAll(companyId);

      var list = listAll.Where(x => x.parent == parent).ToList();

      var results = new List<DepartmentModel.SelectList>();
      foreach (var item in list)
      {
        var tree = string.Empty;
        if (level > 0)
        {
          for (int i = 0; i < level - 1; i++)
            tree += "' ";
          tree += "└─ ";
        }

        results.Add(new DepartmentModel.SelectList()
        {
          id = item.id,
          name = tree + item.name,
          level = level
        });

        results.AddRange(GetSelectList(companyId, item.id, level + 1, listAll));
      }

      return results;
    }


    /// <summary>
    /// Danh sách phòng ban của một người
    /// </summary>
    public static List<DepartmentModel.SelectList> GetSelectListOfUser(string companyId, List<string> departments_id, 
      List<DepartmentModel> listAll)
    {
      if(listAll == null)
        listAll = GetAll(companyId);

      // Lấy data phòng ban của một người
      var departmentsUser = listAll.Where(x => departments_id.Contains(x.id)).ToList();

      return GetSelectList(companyId, null, 0, departmentsUser);
    }


    /// <summary>
    /// Danh sách tên phòng ban của một người
    /// </summary>
    public static List<string> GetNameListOfUser(string companyId, List<string> departments_id, 
      List<DepartmentModel> departments)
    {
      var results = new List<string>();
      var department = DbDepartment.GetSelectListOfUser(companyId, departments_id, departments);
      foreach (var item in department)
      {
        results.Add(item.name.Replace("'", "").Replace("└─", "").Trim());
      }
      return results;
    }


    /// <summary>
    /// Tất cả phòng ban cấp dưới của 1 phòng ban
    /// </summary>
    public static async Task<List<DepartmentModel>> GetAllChilds(string companyId, string parent, 
      List<DepartmentModel> departments)
    {
      if (departments == null)
        departments = GetAll(companyId);

      var results = new List<DepartmentModel>();

      var list = departments.Where(x => x.parent == parent).ToList();
      results.AddRange(list);

      foreach (var item in list)
      {
        var childs = await GetAllChilds(companyId, item.id, departments);
        results.AddRange(childs);
      }

      return results;
    }


    /// <summary>
    /// Tất cả phòng ban cấp trên của 1 phòng ban
    /// </summary>
    public static async Task<List<DepartmentModel>> GetAllParents(string companyId, string parent)
    {
      var results = new List<DepartmentModel>();

      var item = await Get(companyId, parent);
      if(item != null)
      {
        results.Add(item);
        if (!string.IsNullOrEmpty(item.parent))
          results.AddRange(await GetAllParents(companyId, item.parent));
      }

      return results;
    }


    /// <summary>
    /// Thêm nhân viên vào phòng ban
    /// </summary>
    /// <returns></returns>
    public static async Task<DepartmentModel> AddMember(string companyId, DepartmentModel department, 
      string memberId, int memberRole)
    {
      if (department.members_id == null)
        department.members_id = new();
      if (department.members_list == null)
        department.members_list = new();

      // Đổi quyền quản lý cũ
      if (memberRole == 1)
      {
        var manager = department.members_list.SingleOrDefault(x => x.role == 1);
        if (manager != null)
        {
          manager.role = 3;
          var mangerUser = await DbUser.Get(companyId, manager.id);
          if(mangerUser != null)
          {
            mangerUser.title = 3;
            mangerUser.title_name = string.Empty;
            await DbUser.Update(companyId, mangerUser);
          }
        }
      }

      // Kiểm tra phòng ban có nhân viên chưa
      var member = department.members_list.SingleOrDefault(x => x.id == memberId);
      if (member == null)
      {
        department.members_id.Add(memberId);
        department.members_list.Add(new()
        {
          id = memberId,
          role = memberRole == 0 ? 3 : memberRole
        });
      }
      else if (memberRole != 0 && memberRole != member.role)
      {
        member.role = memberRole;
      }

      // Update data department
      department.members_list = department.members_list.OrderBy(x => x.role).ToList();
      department = await Update(companyId, department);

      // Update data user
      var user = await DbUser.Get(companyId, memberId);
      if(user != null)
      {
        if (user.departments_id == null)
          user.departments_id = new();
        if (user.departments_id.Where(x => x == department.id).Count() == 0)
          user.departments_id.Add(department.id);

        // Set chức danh theo lần set cuối cùng
        if (memberRole != 0 && memberRole != 3)
          user.title = memberRole;
        
        // Nếu chưa có chức danh thì là nhân viên
        if (user.title == 0)
          user.title = 3;
        
        // Set tên chức danh
        if (user.title == 1)
          user.title_name = department.manager;
        else if (user.title == 2)
          user.title_name = department.deputy;
        else
          user.title_name = string.Empty;
        await DbUser.Update(companyId, user);
      }

      // Thêm vào phòng ban cấp trên
      var parents = await GetAllParents(companyId, department.parent);
      foreach (var item in parents)
        await AddMember(companyId, item, memberId, 0);

      return department;
    }


    /// <summary>
    /// Lấy quyền hạn của một người trong phòng ban
    /// </summary>
    /// <returns>0. Không thuộc, 1. Quản lý, 2. Phó quản lý, 3. Nhân viên</returns>
    public static int GetRole(string companyId, string departmentId, string userId, out string title)
    {
      var _db = Mongo.DbConnect("fastdo_" + companyId);

      var collection = _db.GetCollection<DepartmentModel>(_collection);

      var department = collection.Find(x => x.id == departmentId && !x.delete).FirstOrDefault();

      if(department != null)
      {
        if (department.members_list == null)
          department.members_list = new();

        var member = department.members_list.SingleOrDefault(x => x.id == userId);
        if (member != null)
        {
          title = member.role == 1 ? department.manager : member.role == 2 ? department.deputy : "";
          return member.role;
        }
      }

      title = string.Empty;
      return 0;
    }


    /// <summary>
    /// Danh sách quản lý cấp trên của một người
    /// </summary>
    public static List<UserModel> GetManagerList(string companyId, UserModel user)
    {
      var results = new List<UserModel>();

      if(user.departments_id != null)
      {
        // Lấy data phòng ban của một người
        var departmentAll = GetAll(companyId);
        var departments = departmentAll.Where(x => user.departments_id.Contains(x.id)).ToList();
        var managerList = new List<string>();
        foreach (var item in departments)
        {
          if(item.members_list != null)
          {
            var manager = item.members_list.SingleOrDefault(x => x.role == 1);
            if (manager != null && manager.id != user.id && !managerList.Contains(manager.id))
              managerList.Add(manager.id);
          }
        }

        // Lấy thông tin quản lý
        if(managerList.Count > 0)
        {
          var userList = DbUser.GetAll(companyId);
          results = userList.Where(x => managerList.Contains(x.id)).ToList();
        }
      }

      return results;
    }


    /// <summary>
    /// Kiểm tra manager có phải quản lý/phó quản lý của user
    /// </summary>
    public static bool CheckManagerRole(string companyId, UserModel user, string manager, List<DepartmentModel> departmentAll)
    {
      if(user.departments_id != null)
      {
        // Lấy data phòng ban của một người
        if(departmentAll == null)
          departmentAll = GetAll(companyId);
        var departments = departmentAll.Where(x => user.departments_id.Contains(x.id)).ToList();
        foreach (var item in departments)
        {
          if(item.members_list != null)
          {
            // Lấy quản lý và phó quản lý
            var managerList = item.members_list.Where(x => x.role == 1 || x.role == 2).ToList();
            // user không phải là quản lý thì mới xét tiếp
            if(managerList.Where(x => x.id == user.id && x.role == 1).Count() == 0)
            {
              // Kiểm tra manager có phải là quản lý/phó quản lý
              if (managerList.Where(x => x.id == manager).Count() > 0)
                return true;
            }
          }
        }
      }

      return false;
    }
  }
}
