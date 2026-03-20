using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Realtime;

/// <summary>
/// Service để gửi SignalR events từ backend logic
/// </summary>
public interface IGameHubService
{
    Task NotifyPlayerHpChangedAsync(Guid roomId, Guid playerId, int currentHp, int maxHp, CancellationToken ct = default);
    Task NotifyPlayerPickedReviveCardAsync(Guid roomId, Guid playerId, int reviveCardsCount, CancellationToken ct = default);
    Task NotifyPlayerRevivedAsync(Guid roomId, Guid revivedPlayerId, Guid reviverPlayerId, int restoredHp, CancellationToken ct = default);
    Task NotifyRoomStateSyncedAsync(Guid roomId, RoomRuntimeState state, CancellationToken ct = default);
    Task NotifyPlayerJoinedAsync(Guid roomId, Guid playerId, string displayName, CancellationToken ct = default);
    Task NotifyPlayerLeftAsync(Guid roomId, Guid playerId, CancellationToken ct = default);
    Task NotifyGameStartedAsync(Guid roomId, CancellationToken ct = default);
    Task NotifyFloorCompletedAsync(Guid roomId, int floorNumber, CancellationToken ct = default);
}
