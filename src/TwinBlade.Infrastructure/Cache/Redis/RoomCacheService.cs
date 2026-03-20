using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace TwinBlade.Infrastructure.Cache.Redis;

public sealed class RoomCacheService : IRoomCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisOptions _options;

    public RoomCacheService(
        IConnectionMultiplexer redis,
        IOptions<RedisOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    public async Task SetRoomCodeAsync(
        string roomCode,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{_options.RoomKeyPrefix}{roomCode}";
        await db.StringSetAsync(key, roomId.ToString(), TimeSpan.FromHours(2));
    }

    public async Task<Guid?> GetRoomIdByCodeAsync(
        string roomCode,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{_options.RoomKeyPrefix}{roomCode}";
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        return Guid.TryParse(value!.ToString(), out var roomId) ? roomId : null;
    }
}