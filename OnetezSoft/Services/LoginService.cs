using Microsoft.Extensions.Configuration;
using OnetezSoft.Models;
using System;
using System.Runtime.Caching;

namespace OnetezSoft.Services
{
  public class LoginService
  {
    public class LoginData
    {
      public UserModel user { get; set; }
      public CompanyModel company { get; set; }
      public long create { get; set; }
    }

    public static void SetCache(string circuit, UserModel user, CompanyModel company)
    {
      if (string.IsNullOrEmpty(circuit))
      {
        return;
      }
      ObjectCache cache = MemoryCache.Default;
      CacheItemPolicy policy = new CacheItemPolicy();
      policy.AbsoluteExpiration = DateTimeOffset.Now.AddDays(1);

      var obj = new LoginData() { company = company, user = user, create = DateTime.Now.Ticks };

      cache.Add(circuit, obj, policy, null);
    }

    public static LoginData GetCache(string circuit)
    {
      if (string.IsNullOrEmpty(circuit))
      {
        return null;
      }

      ObjectCache cache = MemoryCache.Default;

      if (cache[circuit] == null)
      {
        return null;
      }
      else
      {
        var result = (LoginData)cache.Get(circuit);
        cache.Remove(circuit);

        return result;
      }
    }

    public static string GetVersion()
    {
      IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
      return config.GetValue<string>("FastdoAppVersion");
    }
  }
}
