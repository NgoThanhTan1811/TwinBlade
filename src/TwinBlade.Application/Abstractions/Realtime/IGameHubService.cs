namespace TwinBlade.Application.Abstractions.Realtime;

/// <summary>
/// Service for sending session events via SignalR
/// NOTE: Gameplay events (HP, items, combat) are handled by Unity Mirror
/// </summary>
public interface IGameHubService
{
    Task NotifyPlayerJoinedAsync(Guid roomId, Guid playerId, string displayName, CancellationToken ct = default);
    Task NotifyPlayerLeftAsync(Guid roomId, Guid playerId, CancellationToken ct = default);
    Task NotifyGameStartedAsync(Guid roomId, CancellationToken ct = default);
}
