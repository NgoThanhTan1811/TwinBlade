using Microsoft.AspNetCore.SignalR;
using TwinBlade.Application.Abstractions.Realtime;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Realtime;

public sealed class GameHubService(IHubContext<Hub<IGameHub>, IGameHub> hubContext) : IGameHubService
{
    private static string RoomGroup(Guid roomId) => $"room:{roomId}";

    public Task NotifyPlayerHpChangedAsync(Guid roomId, Guid playerId, int currentHp, int maxHp, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerHpChanged(new { playerId, currentHp, maxHp });

    public Task NotifyPlayerPickedReviveCardAsync(Guid roomId, Guid playerId, int reviveCardsCount, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerPickedReviveCard(new { playerId, reviveCardsCount });

    public Task NotifyPlayerRevivedAsync(Guid roomId, Guid revivedPlayerId, Guid reviverPlayerId, int restoredHp, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerRevived(new { revivedPlayerId, reviverPlayerId, restoredHp });

    public Task NotifyRoomStateSyncedAsync(Guid roomId, RoomRuntimeState state, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .RoomStateSynced(state);

    public Task NotifyPlayerJoinedAsync(Guid roomId, Guid playerId, string displayName, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerJoined(new { playerId, displayName });

    public Task NotifyPlayerLeftAsync(Guid roomId, Guid playerId, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .PlayerLeft(new { playerId });

    public Task NotifyGameStartedAsync(Guid roomId, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .GameStarted(new { roomId });

    public Task NotifyFloorCompletedAsync(Guid roomId, int floorNumber, CancellationToken ct = default)
        => hubContext.Clients.Group(RoomGroup(roomId))
            .FloorCompleted(new { roomId, floorNumber });
}
