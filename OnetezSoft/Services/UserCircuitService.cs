using Microsoft.AspNetCore.Components.Server.Circuits;
using System;
using System.Collections.Concurrent;

namespace OnetezSoft.Services
{
  public class UserCircuitService
  {
    public ConcurrentDictionary<string, string> Users { get; } = new();

    public event Action<Circuit>? UsersChanged;

    public void OnUsersChanged(Circuit circuit)
    {
      UsersChanged?.Invoke(circuit);
    }

    public void Update(string circuit, string userId)
    {
      Users.TryUpdate(circuit, userId, "");
    }
  }
}
