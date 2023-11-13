using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using OnetezSoft.Handled;
using System;

namespace OnetezSoft.Services
{
  public class HubService : IDisposable
  {
    private readonly HubConnection _hubConnection;
    private string userId;
    private string companyId;
    private string module;

    public HubService(NavigationManager navigate)
    {
      _hubConnection = new HubConnectionBuilder()
          .WithUrl(navigate.ToAbsoluteUri("/userHub"))
          .Build();

      _hubConnection.StartAsync();
    }

    public HubConnection GetHubConnection()
    {
      return _hubConnection;
    }

    public void Initialized(string user, string company, string module)
    {
      this.userId = user;
      this.companyId = company;
      this.module = module;
    }

    public async void Dispose()
    {
      if (_hubConnection is not null)
      {
        if (!userId.IsEmpty() && !companyId.IsEmpty())
          await _hubConnection.SendAsync("Off", module, userId, companyId, "");

        await _hubConnection.DisposeAsync();
      }
    }
  }
}
