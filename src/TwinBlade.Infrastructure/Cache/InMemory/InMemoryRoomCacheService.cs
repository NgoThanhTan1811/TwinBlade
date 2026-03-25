using System.Collections.Concurrent;
using TwinBlade.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;

namespace TwinBlade.Infrastructure.Cache.InMemory;

public sealed class InMemoryRoomCacheService(ILogger<InMemoryRoomCacheService> logger) : IRoomCacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<InMemoryRoomCacheService> _logger = logger;

    private sealed class CacheEntry
    {
        public string? Value { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    public Task SetRoomCodeAsync(
        string roomCode,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.AddHours(2);
        var entry = new CacheEntry
        {
            Value = roomId.ToString(),
            ExpiresAt = expiresAt
        };

        _cache.AddOrUpdate(roomCode, entry, (_, _) => entry);
        _logger.LogInformation("In-memory cache set for room code {RoomCode} with expiry at {ExpiresAt}", roomCode, expiresAt);
        return Task.CompletedTask;
    }

    public Task<Guid?> GetRoomIdByCodeAsync(
        string roomCode,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(roomCode, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(roomCode, out _);
                _logger.LogInformation("In-memory cache entry for room code {RoomCode} has expired", roomCode);
                return Task.FromResult<Guid?>(null);
            }

            if (Guid.TryParse(entry.Value, out var roomId))
            {
                _logger.LogInformation("In-memory cache hit for room code {RoomCode}", roomCode);
                return Task.FromResult<Guid?>(roomId);
            }
        }

        _logger.LogInformation("In-memory cache miss for room code {RoomCode}", roomCode);
        return Task.FromResult<Guid?>(null);
    }
}
