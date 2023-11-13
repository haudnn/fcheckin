using OnetezSoft.Data;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class GlobalService
  {
    /// <summary>
    /// Dữ liệu công ty
    /// </summary>
    private readonly ConcurrentDictionary<string, SharedStorage> companys;
    /// <summary>
    /// User online: user id / company_page
    /// </summary>
    private readonly ConcurrentDictionary<string, string> onlines;

    private readonly ConcurrentDictionary<string, long> lastOnlines;
    /// <summary>
    /// Usser online: hub / userid
    /// </summary>
    private readonly ConcurrentDictionary<string, string> onlinesHub;

    public GlobalService()
    {
      this.companys = new();
      this.onlines = new();
      this.onlinesHub = new();
      this.lastOnlines = new();
    }

    #region Hubs
    public string GetStatis()
    {
      return $"Total Companys: {companys.Count}, total Onlines: {onlines.Count}, total Hubs: {onlinesHub.Count}";
    }

    public async Task SetHub(string hub, string userId)
    {
      if (hub.IsEmpty())
        return;

      await Task.WhenAll(
        Task.Run(() => TryAddOnlineHub(hub, userId)));
    }

    public async Task RemoveHub(string hub)
    {
      await Task.WhenAll(
        Task.Run(() => TryRemoveOnlineHub(hub)),
        Task.Run(() => TryRemoveAllOnlineHub()));
    }

    public async Task RemoveHubByValue(string id)
    {
      await Task.WhenAll(
        Task.Run(async () =>
        {
          if (onlinesHub.Values.Contains(id))
          {
            var hubs = onlinesHub.Keys.ToList();
            foreach (var hub in hubs)
            {
              if (onlinesHub.TryGetValue(hub, out var userId))
              {
                if (userId == id)
                {
                  await RemoveHub(hub);
                  break;
                }
              }
            }
          }
        }));
    }

    public async Task<Dictionary<string, string>> GetHubs()
    {
      var result = new Dictionary<string, string>();
      await Task.WhenAll(Task.Run(() => TryGetHubs(out result)));
      return result;
    }

    public async Task RemoveHubs()
    {
      await Task.WhenAll(Task.Run(() => TryRemoveAllOnlineHub()));
    }

    private void TryAddOnlineHub(string hub, string userId)
    {
      if (hub.IsEmpty())
        return;

      if (onlinesHub.ContainsKey(hub))
      {
        var old = onlinesHub[hub];
        onlinesHub.TryUpdate(hub, userId, old);
      }
      else
      {
        onlinesHub.TryAdd(hub, userId);
      }
      //if (!userId.IsEmpty())
      //{
      //  var empty = onlinesHub.Where(x => x.Value.IsEmpty()).Select(x => x.Key);

      //  foreach (var key in empty)
      //  {
      //    RemoveHub(key);
      //  }
      //}
    }

    private void TryRemoveOnlineHub(string hub)
    {
      if (hub.IsEmpty())
        return;

      if (onlinesHub.ContainsKey(hub))
      {
        onlinesHub.TryRemove(hub, out string value);
      }
    }

    private void TryRemoveAllOnlineHub()
    {
      var empty = onlinesHub.Where(x => x.Value.IsEmpty()).Select(x => x.Key);

      foreach (var key in empty)
      {
        TryRemoveOnlineHub(key);
      }
    }

    private Dictionary<string, string> TryGetHubs(out Dictionary<string, string> value)
    {
      var array = onlinesHub.ToArray();
      value = array.ToDictionary(x => x.Key, x => x.Value);
      return value;
    }
    #endregion

    #region ShareStorage
    /// <summary>
    /// Add Or Update share data and tracking
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    public async Task<SharedStorage> AddOrUpdate(string companyId)
    {
      var users = await DbUser.GetAll(companyId, null);
      var members = users.Select(x => UserService.ConvertToMember(x)).ToList();
      var dayOff = await DbDayOff.GetAll(companyId);
      var okrConfig = await DbOkrConfig.Get(companyId);
      var products = await DbMainProduct.GetAll();
      var KpisCycles = await DbKpisCycle.GetList(companyId);
      var company = await DbMainCompany.Get(companyId);

      var data = new SharedStorage()
      {
        MemberList = members,
        UserList = users,
        DayOffList = dayOff,
        OkrConfig = okrConfig,
        Products = products,
        update = DateTime.Now.Ticks,
        KpisCycles = KpisCycles,
        comany = company,
      };

      // Add or Update userList
      await Task.WhenAll(Task.Run(() => AddOrUpdateShareStorage(companyId, data)));
      return data;
    }

    public async Task<bool> Update(string companyId)
    {
      if (await CheckIfExist(companyId))
      {
        await AddOrUpdate(companyId);
        return true;
      }

      return false;
    }

    public void AddOrUpdateShareStorage(string companyId, SharedStorage data)
    {
      try
      {
        // Share data
        if (companys.ContainsKey(companyId))
        {
          var old = companys[companyId];
          companys.TryUpdate(companyId, data, old);
        }
        else
        {
          companys.TryAdd(companyId, data);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    /// <summary>
    /// Kiểm tra dữ liệu chia sẻ của công ty đã có chưa
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    public async Task<bool> CheckIfExist(string companyId)
    {
      return companys.ContainsKey(companyId);
    }

    public async Task<SharedStorage> GetShareStorage(string companyId)
    {
      if (companys.TryGetValue(companyId, out var result))
        return result;

      return await AddOrUpdate(companyId);
    }

    public async Task<bool> RemoveShareStorage(string companyId)
    {
      if (this.companys.TryRemove(companyId, out var result))
        return true;

      return false;
    }

    /// <summary>
    /// Get UserList by CompanyId
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    public async Task<List<MemberModel>> GetUserList(string companyId)
    {
      var exist = companys.TryGetValue(companyId, out var data);

      if (exist)
      {
        if (data == null)
          data = new();
      }
      else
      {
        data = await AddOrUpdate(companyId);
      }

      return data.MemberList;
    }

    /// <summary>
    /// Get UserList of All Company
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, List<MemberModel>>> GetAllMemberList()
    {
      return this.companys.ToDictionary(x => x.Key, x => x.Value.MemberList);
    }

    public async Task<Dictionary<string, List<UserModel>>> GetAllUserList()
    {
      return this.companys.ToDictionary(x => x.Key, x => x.Value.UserList);
    }
    #endregion

    /// <summary>
    /// Set User Online with speical module
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task SetOnline(string id, string module)
    {
      if (onlines.ContainsKey(id))
      {
        onlines.TryGetValue(id, out var old);
        lastOnlines.TryGetValue(id, out var oldLong);
        onlines.TryUpdate(id, module, old);
        lastOnlines.TryUpdate(id, DateTime.Now.Ticks, oldLong);
      }
      else
      {
        onlines.TryAdd(id, module);
        lastOnlines.TryAdd(id, DateTime.Now.Ticks);
      }
    }

    /// <summary>
    /// Set User Online by time
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task SetOnline(string id)
    {
      if (lastOnlines.ContainsKey(id))
      {
        var oldLong = lastOnlines[id];
        lastOnlines.TryUpdate(id, DateTime.Now.Ticks, oldLong);
      }
      else
      {
        lastOnlines.TryAdd(id, DateTime.Now.Ticks);
      }
    }

    /// <summary>
    /// Lấy danh sách user Online theo module và công ty
    /// </summary>
    /// <returns></returns>
    public async Task<List<MemberModel>> GetOnline(string module, string company)
    {
      var sameModule = onlines.Where(x => x.Value == module);
      if (sameModule.Any())
      {
        var userIds = sameModule.Select(x => x.Key);
        var list = companys.FirstOrDefault(x => x.Key == company);
        return list.Value.MemberList.Where(x => userIds.Contains(x.id)).ToList();
      }

      return new List<MemberModel>();
    }

    public async Task<Dictionary<string, string>> GetOnline()
    {
      return onlines.ToDictionary(x => x.Key, x => x.Value);
    }

    public async Task<Dictionary<string,long>> GetLastOnline()
    {
      return lastOnlines.ToDictionary(x => x.Key, x => x.Value);
    }

    public async Task RemoveLasOnline(string id)
    {
      try 
      {
        lastOnlines.TryRemove(id, out long value);
      }
      catch(Exception ex)
      {

      }
    }

    /// <summary>
    /// Lấy danh sách user Online theo module
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public async Task<List<MemberModel>> GetOnline(string module)
    {
      var sameModule = onlines.Where(x => x.Value == module);
      if (sameModule.Any())
      {
        var userIds = sameModule.Select(x => x.Key);
        var all = companys.Select(x => x.Value).SelectMany(x => x.MemberList).ToList();
        var list = new List<MemberModel>();

        foreach (var item in all)
        {
          if (list.FirstOrDefault(x => x.id == item.id) == null)
            list.Add(item);
        }
        return list.Where(x => userIds.Contains(x.id)).ToList();
      }

      return new List<MemberModel>();
    }

    /// <summary>
    /// Lấy tất cả user đang Online
    /// </summary>
    /// <param name="dictionary">True : theo module; False: theo thời gian</param>
    /// <returns></returns>
    public async Task<List<MemberModel>> GetOnline(bool dictionary)
    {
      if (!dictionary)
      {
        var all = await DbMainUser.GetAll();
        return all.Where(x => x.online >= DateTime.Now.AddMinutes(-1).Ticks).Select(x => UserService.ConvertToMember(x)).ToList();
      }
      else
      {
        var list = new List<MemberModel>();
        var all = await GetAllMemberList();
        var allUser = all.SelectMany(x => x.Value).ToList();

        foreach (var item in onlines.Select(x => x.Key))
        {
          var user = allUser.FirstOrDefault(x => x.id == item);
          if (user != null)
          {
            if (list.FirstOrDefault(x => x.id == user.id) == null)
              list.Add(user);
          }
        }
        return list;
      }
    }

    public async Task SetOffline(string id)
    {
      if (string.IsNullOrEmpty(id))
        return;

      if (onlines.ContainsKey(id))
      {
        onlines.TryRemove(id, out string value);
        lastOnlines.TryRemove(id, out long error);
      }
    }

    public async Task SetOffline()
    {
      onlines.Clear();
      lastOnlines.Clear();
      //foreach (var kvp in onlines)
      //{
      //  // Please note that by now the frame may have been already
      //  // removed by another thread.
      //  onlines.TryRemove(kvp.Key, out var ignored);
      //}
    }

    /// <summary>
    /// Clean Up UserList after 1 day
    /// </summary>
    public async Task CleanUp()
    {
      try
      {
        var values = onlines.Select(x => x.Value);

        if (values.Any())
        {
          var empty = companys.Where(x => values.FirstOrDefault(y => y.StartsWith(x.Key)) == null);
          var current = DateTime.Now.Ticks;
          // Remove data nếu không còn ai trong công ty online và có thời gian tạo > 30p
          foreach (var item in empty)
          {
            if (item.Value != null && item.Value.update > 0 && new TimeSpan(current - item.Value.update).TotalMinutes > 30)
            {
              if (!item.Key.IsEmpty())
                if (companys.ContainsKey(item.Key))
                  companys.TryRemove(item.Key, out var str1);
            }
          }
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }
  }

  public class SharedStorage
  {
    public long update { get; set; }

    public List<MemberModel> MemberList { get; set; } = new();

    public List<UserModel> UserList { get; set; } = new();

    public List<DayOffModel> DayOffList { get; set; } = new();

    public OkrConfigModel OkrConfig { get; set; } = new();

    public List<ProductModel> Products { get; set; } = new();

    public List<KpisCycleModel> KpisCycles { get; set; } = new();

    public CompanyModel comany { get; set; } = new();
  }
}
