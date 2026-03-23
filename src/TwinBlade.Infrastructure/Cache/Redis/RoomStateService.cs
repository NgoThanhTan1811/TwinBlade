using System.Text.Json;
using StackExchange.Redis;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Cache.Redis;

public sealed class RoomStateService(IConnectionMultiplexer redis) : IRoomStateService
{
    private static readonly TimeSpan RoomStateTtl = TimeSpan.FromHours(4);
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(10);

    private static string RoomKey(Guid roomId) => $"runtime:room:{roomId}";
    private static string PlayerKey(Guid roomId, Guid playerId) => $"runtime:room:{roomId}:player:{playerId}";

    public async Task<RoomRuntimeState?> GetRoomStateAsync(Guid roomId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(RoomKey(roomId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<RoomRuntimeState>(value.ToString());
    }

    public async Task SetRoomStateAsync(RoomRuntimeState state, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        state.LastActivityAt = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(state);
        await db.StringSetAsync(RoomKey(state.RoomId), json, RoomStateTtl);

        // Sync individual player states
        foreach (var player in state.Players)
            await SetPlayerStateInternalAsync(db, state.RoomId, player);
    }

    public async Task<bool> DeleteRoomStateAsync(Guid roomId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        return await db.KeyDeleteAsync(RoomKey(roomId));
    }

    public async Task UpdatePlayerStateAsync(Guid roomId, RoomPlayerState playerState, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        await SetPlayerStateInternalAsync(db, roomId, playerState);

        // Update player inside room state too
        var roomState = await GetRoomStateAsync(roomId, ct);
        if (roomState is null) return;

        var idx = roomState.Players.FindIndex(p => p.PlayerId == playerState.PlayerId);
        if (idx >= 0)
        {
            roomState.Players[idx] = playerState;
            roomState.LastActivityAt = DateTime.UtcNow;
            roomState.Version++;
            await db.StringSetAsync(RoomKey(roomId), JsonSerializer.Serialize(roomState), RoomStateTtl);
        }
    }

    public async Task<RoomPlayerState?> GetPlayerStateAsync(Guid roomId, Guid playerId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(PlayerKey(roomId, playerId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<RoomPlayerState>(value.ToString());
    }

    public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan expiry, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        return await db.StringSetAsync($"lock:{lockKey}", "1", expiry, When.NotExists);
    }

    public async Task ReleaseLockAsync(string lockKey, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync($"lock:{lockKey}");
    }

    private static async Task SetPlayerStateInternalAsync(IDatabase db, Guid roomId, RoomPlayerState player)
    {
        var json = JsonSerializer.Serialize(player);
        await db.StringSetAsync(PlayerKey(roomId, player.PlayerId), json, RoomStateTtl);
    }
}
