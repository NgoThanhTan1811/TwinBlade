using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TwinBlade.Application.Abstractions.Realtime;

namespace TwinBlade.Api.Hubs;

/// <summary>
/// SignalR Hub for session management (lobby events, game start notifications)
/// NOTE: Gameplay (HP, items, combat) is handled by Unity Mirror
/// </summary>
[Authorize]
public sealed class GameHub : Hub<IGameHub>
{
    public async Task JoinRoom(string roomId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");

    public async Task LeaveRoom(string roomId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomId}");

    private Guid? GetPlayerId()
    {
        var sub = Context.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
