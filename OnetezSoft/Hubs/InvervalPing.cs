using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OnetezSoft.Data;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using OnetezSoft.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnetezSoft.Hubs
{
  public class IntervalPing : BackgroundService
  {
    private readonly GlobalService global;
    private readonly IHubContext<UserHub> _hubContext;
    private int Frequency = 180000;

    public IntervalPing(IHubContext<UserHub> hubContext, GlobalService globalService)
    {
      _hubContext = hubContext;
      global = globalService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      if (global == null)
      {
        return;
      }
      try
      {
        Task.Run(async () =>
        {
          await OnlineLoop(stoppingToken);
        }, stoppingToken);

        Task.Run(async () =>
        {
          await JobLoop(stoppingToken);
        }, stoppingToken);
      }
      catch (Exception ex)
      {
        StorageService.CatchLog("Interval", "ExecuteAsync()/Loop 60s", ex.ToString(), "Signin Loading...");
      }
    }

    private async Task OnlineLoop(CancellationToken stoppingToken)
    {
      if (global == null)
      {
        return;
      }

      // Loop for online
      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          Console.WriteLine("===============");
          Console.WriteLine("Online Job started!");
          var sw = new Stopwatch();
          sw.Start();
          double time = 0;

          Dictionary<string, string> onlineHubs = await global.GetHubs();
          Dictionary<string, string> currentOnline = await global.GetOnline();

          var onlineKeys = currentOnline.Keys.ToList();
          foreach (var key in onlineKeys)
          {
            if (currentOnline.TryGetValue(key, out var user))
            {
              var hub = onlineHubs.FirstOrDefault(x => x.Value == key);
              if (!hub.Key.IsEmpty())
                await _hubContext.Clients.Client(hub.Key).SendAsync("PingAll");
            }
          }

          Console.WriteLine($"Server ping to all Online users: {Math.Round(sw.Elapsed.TotalSeconds - time, 1)}s");
          time = sw.Elapsed.TotalMilliseconds;

          // Sau khi ping to all online User, wait for 30s
          new Task(async () =>
          {
            await Task.Delay(15000);
            Console.WriteLine($"Server try receive data after 15s");
            // Lấy lại danh sách online theo thời gian
            Dictionary<string, long> lastOnlines = await global.GetLastOnline();
            // Lấy lại danh sách online hiện tại
            Dictionary<string, string> current = await global.GetOnline();
            var now = DateTime.Now.Ticks;

            var oldUserPerCompany = current
                      .Where(x => !x.Key.IsEmpty() && !x.Value.IsEmpty())
                      .ToDictionary(x => x.Key, x => x.Value.Split("_").FirstOrDefault())
                      .GroupBy(x => x.Value)
                      .ToDictionary(x => x.Key, x => x.Count());

            var userList = current.Keys.ToList();
            foreach (var id in userList)
            {
              if (current.TryGetValue(id, out var userValue))
              {
                if (lastOnlines.ContainsKey(id))
                {
                  if (lastOnlines.TryGetValue(id, out long value))
                  {
                    if (new TimeSpan(now - value).TotalSeconds > time + 20)
                    {
                      await global.SetOffline(id);
                      current.Remove(id);
                      lastOnlines.Remove(id);
                    }
                  }
                }
                else
                {
                  await global.SetOffline(id);
                  current.Remove(id);
                }
              }
            }

            var lastOnlineList = lastOnlines.Keys.ToList();
            foreach (var id in lastOnlineList)
            {
              if (!current.ContainsKey(id))
              {
                await global.RemoveLasOnline(id);
              }
            }
            Console.WriteLine($"Server try remove all user not response: {Math.Round(sw.Elapsed.TotalSeconds - time, 1)}s");
            time = sw.Elapsed.TotalMilliseconds;

            var newUserPerCompany = current
                      .Where(x => !x.Key.IsEmpty() && !x.Value.IsEmpty())
                      .ToDictionary(x => x.Key, x => x.Value.Split("_").FirstOrDefault())
                      .GroupBy(x => x.Value)
                      .ToDictionary(x => x.Key, x => x.Count());

            Dictionary<string, string> hubs = await global.GetHubs();
            foreach (var item in oldUserPerCompany)
            {
              Console.WriteLine($"Kiểm tra công ty: {item.Key} / {item.Value}");
              var changed = false;

              if (!newUserPerCompany.ContainsKey(item.Key))
              {
                Console.WriteLine($"Công ty không còn tồn tại");
                changed = true;
              }
              else
              {
                if (newUserPerCompany.TryGetValue(item.Key, out var count))
                {
                  if (item.Value != count)
                  {
                    Console.WriteLine($"Số lượng online thay đổi");
                    changed = true;
                  }
                }
              }

              if (changed)
              {
                Console.WriteLine($"Lấy danh sách user của công ty bị thay đổi");
                var userInCompany = current
                                    .Where(x => !x.Key.IsEmpty() && !x.Value.IsEmpty() && x.Value.StartsWith(item.Key))
                                    .Select(x => x.Key).ToList();
                foreach (var user in userInCompany)
                {
                  var userHub = hubs.FirstOrDefault(x => x.Value == user);
                  if (!userHub.Key.IsEmpty() && !userHub.Value.IsEmpty())
                  {
                    Console.WriteLine($"Ping tới những user còn online của công ty đó: {userHub.Value}");
                    await _hubContext.Clients.Client(userHub.Key).SendAsync("ReGetData");
                  }
                }
              }
            }

            time = sw.Elapsed.TotalMilliseconds;
          }).Start();

          sw.Stop();

          await Task.Delay((int)Math.Max((time + 40) * 1000, 60000), stoppingToken);
        }
        catch (Exception ex)
        {
          StorageService.CatchLog("Interval", "ExecuteAsync()/Loop 60s", ex.ToString(), "Signin Loading...");
          await Task.Delay(Frequency, stoppingToken);
        }
      }
    }

    private async Task JobLoop(CancellationToken stoppingToken)
    {
      IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
      var environtment = config.GetValue<string>("EnvironmentDevelopment");
      // Loop 3min for tracking, news...
      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          Console.WriteLine("===============");
          Console.WriteLine("Job started!");
          await global.CleanUp();

          Dictionary<string, string> currentOnline = await global.GetOnline();
          Dictionary<string, string> onlineHubs = await global.GetHubs();

          if (currentOnline.Count > 0)
          {
            var userData = new List<ActivitiesTrackingModel.UserTracking>();
            // Create Tracking
            var model = new ActivitiesTrackingModel();

            var companyList = currentOnline.Where(x => !x.Key.IsEmpty() && !x.Value.IsEmpty())
                                .Select(x => x.Value.Split("_").FirstOrDefault()).ToList();
            companyList = companyList.Distinct().ToList();

            // Loop for tracking
            var onlineKeys = currentOnline.Keys.ToList();
            foreach (var key in onlineKeys)
            {
              if (currentOnline.TryGetValue(key, out var user))
              {
                if (!user.IsEmpty() && !key.IsEmpty())
                {
                  if (userData.FirstOrDefault(x => x.user == key) == null)
                  {
                    userData.Add(new()
                    {
                      user = key,
                      company = user.Split("_").FirstOrDefault(),
                      url = user.Split("_").LastOrDefault(),
                    });
                  }
                }
              }
            }

            if (environtment == "production")
            {
              new Task(async () =>
              {
                model.update = DateTime.Now.Ticks;
                model.users = userData;
                // await DbMainActivitiesTracking.Create(model);
              }).Start();
            }

            // Loop for News
            foreach (var companyId in companyList)
            {
              if (!companyId.IsEmpty())
              {
                var userList = currentOnline
                                .Where(x => !x.Key.IsEmpty() && !x.Value.IsEmpty() && x.Value.StartsWith(companyId))
                                .Select(x => x.Key).ToList();
                var newNoti = await DbNotify.GetNews(companyId);
                if (newNoti.Count > 0)
                {
                  foreach (var user in userList)
                  {
                    var userHub = onlineHubs.FirstOrDefault(x => x.Value == user);
                    if (!userHub.Key.IsEmpty() && !userHub.Value.IsEmpty())
                    {
                      var userNews = newNoti.Where(x => x.user == userHub.Value).ToList();
                      if (userNews.Count > 0)
                      {
                        await _hubContext.Clients.Client(userHub.Key).SendAsync("DeliverNews", companyId, userNews);
                      }
                    }
                  }
                }
              }
            }
          }
          await Task.Delay(Frequency, stoppingToken);
        }
        catch (Exception ex)
        {
          StorageService.CatchLog("Interval", "ExecuteAsync()/Loop 3mins", ex.ToString(), "Signin Loading...");
          await Task.Delay(Frequency, stoppingToken);
        }
      }
    }
  }
}
