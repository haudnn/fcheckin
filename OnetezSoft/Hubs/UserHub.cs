using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnetezSoft.Data;
using OnetezSoft.Handled;
using OnetezSoft.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnetezSoft.Hubs
{
  public class UserHub : Hub
  {
    private GlobalService globalService;

    public UserHub([FromServices] GlobalService sv)
    {
      this.globalService = sv;
    }

    public override async Task OnConnectedAsync()
    {
      await globalService.SetHub(Context.ConnectionId, "");
      await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
      await globalService.RemoveHub(Context.ConnectionId);
      await base.OnDisconnectedAsync(exception);
    }

    public async Task On(string module, string userId, string company, string message = "")
    {
      try
      {
        await globalService.SetHub(Context.ConnectionId, userId);
        await globalService.SetOnline(userId, company + "_" + module);

        var allOnline = await globalService.GetOnline();
        var userInCompany = allOnline.Where(x => x.Value.StartsWith(company));
        var allHub = await globalService.GetHubs();

        if(message == "First")
        {
          foreach (var user in userInCompany)
          {
            if (allHub.ContainsValue(user.Key))
            {
              var key = allHub.FirstOrDefault(x => x.Value == user.Key);
              await Clients.Client(key.Key).SendAsync("Online", module, userId, company, message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        StorageService.CatchLog("UserHub", "On()", ex.ToString(), "Signin Loading...");
      }
    }

    public async Task UpdatePath(string module, string userId, string company, string message = "")
    {
      try
      {
        await globalService.SetHub(Context.ConnectionId, userId);
        await globalService.SetOnline(userId, company + "_" + module);
      }
      catch (Exception ex)
      {
        StorageService.CatchLog("UserHub", "UpdatePath()", ex.ToString(), "Signin Loading...");
      }
    }

    public async Task Off(string module, string userId, string company, string message = "")
    {
      try
      {
        await globalService.RemoveHub(Context.ConnectionId);
        await globalService.SetOffline(userId);

        var allOnline = await globalService.GetOnline();
        var userInCompany = allOnline.Where(x => x.Value.StartsWith(company));
        var allHub = await globalService.GetHubs();

        foreach (var user in userInCompany)
        {
          if (allHub.ContainsValue(user.Key))
          {
            var key = allHub.FirstOrDefault(x => x.Value == user.Key);
            await Clients.Client(key.Key).SendAsync("Offline", module, userId, company, message);
          }
        }
      }
      catch (Exception ex)
      {
        StorageService.CatchLog("UserHub", "Off()", ex.ToString(), "Signin Loading...");
      }
    }

    public async Task PushNotificationGlobal(string message, string companyId, bool warning)
    {
      await Clients.All.SendAsync("PushNotificationGlobal", message, companyId, warning);
    }

    public async Task TaskUpdate(string planId, string userId, string companyId, string message = "")
    {
      try
      {
        var userOnline = await globalService.GetOnline();
        var userOnlineInCompany = userOnline.Where(x =>
        {
          return x.Value.StartsWith(companyId)
              && (x.Value.Contains("work/my-task") || x.Value.Contains(planId));
        });
        var allHub = await globalService.GetHubs();

        foreach (var user in userOnlineInCompany)
        {
          if (allHub.ContainsValue(user.Key))
          {
            var key = allHub.FirstOrDefault(x => x.Value == user.Key);
            await Clients.Client(key.Key).SendAsync("TaskUpdate", planId, userId, companyId, message);
          }
        }
      }
      catch (Exception ex)
      {
        StorageService.CatchLog("UserHub", "TaskUpdate()", ex.ToString(), "Signin Loading...");
      }
    }
  }
}
