using Microsoft.AspNetCore.SignalR;
using TwinBlade.Api.Hubs;
using TwinBlade.Application.Abstractions.Realtime;

namespace TwinBlade.Api.Services;

/// <summary>
/// Session event notification service
/// NOTE: Gameplay events (HP, items, combat) are handled by Unity Mirror
/// </summary>
public sealed class GameHubService(IHubContext<GameHub, IGameHub> hubContext) : IGameHubService
{
    private static string RoomGroup(Guid roomId) => $"room:{roomId}";

    public Task NotifyPlayerJoinedAsync(Guid roomId, Guid playerId, string displayName, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerJoined(new { playerId, displayName });

    public Task NotifyPlayerLeftAsync(Guid roomId, Guid playerId, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerLeft(new { playerId });

    public Task NotifyGameStartedAsync(Guid roomId, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .GameStarted(new { roomId });
}