using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TwinBlade.Application.Abstractions.Realtime;
using TwinBlade.Application.Commands.Game;

namespace TwinBlade.Api.Hubs;

[Authorize]
public sealed class GameHub(IMediator mediator) : Hub<IGameHub>
{
    public async Task JoinRoom(string roomId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");

    public async Task LeaveRoom(string roomId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomId}");

    public async Task PickReviveCard(string roomId)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return;
        await mediator.Send(new PickReviveCardCommand(Guid.Parse(roomId), playerId.Value));
    }

    public async Task RevivePlayer(string roomId, string targetPlayerId)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return;
        await mediator.Send(new RevivePlayerCommand(Guid.Parse(roomId), playerId.Value, Guid.Parse(targetPlayerId)));
    }

    public async Task TakeDamage(string roomId, int damage)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return;
        await mediator.Send(new UpdatePlayerHpCommand(Guid.Parse(roomId), playerId.Value, damage));
    }

    private Guid? GetPlayerId()
    {
        var sub = Context.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
