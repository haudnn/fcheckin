using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace OnetezSoft.Hubs
{
  public class WorkHub : Hub
  {
    public async Task TaskUpdate(string planId, string userId, string message = "")
    {
      await Clients.All.SendAsync("TaskUpdate", planId, userId, message);
    }
  }
}
