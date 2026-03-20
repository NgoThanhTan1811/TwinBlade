using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Caching;

/// <summary>
/// Quản lý runtime state của room trong Redis
/// </summary>
public interface IRoomStateService
{
    Task<RoomRuntimeState?> GetRoomStateAsync(Guid roomId, CancellationToken ct = default);
    Task SetRoomStateAsync(RoomRuntimeState state, CancellationToken ct = default);
    Task<bool> DeleteRoomStateAsync(Guid roomId, CancellationToken ct = default);
    
    // Player state trong room
    Task UpdatePlayerStateAsync(Guid roomId, RoomPlayerState playerState, CancellationToken ct = default);
    Task<RoomPlayerState?> GetPlayerStateAsync(Guid roomId, Guid playerId, CancellationToken ct = default);
    
    // Lock để tránh race condition
    Task<bool> AcquireLockAsync(string lockKey, TimeSpan expiry, CancellationToken ct = default);
    Task ReleaseLockAsync(string lockKey, CancellationToken ct = default);
}
