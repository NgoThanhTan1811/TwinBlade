using System.Text.Json;
using StackExchange.Redis;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Cache.Redis;

public sealed class LeaderboardService(IConnectionMultiplexer redis) : ILeaderboardService
{
    private const string LeaderboardKey = "leaderboard:score";
    private static string PlayerMetaKey(Guid playerId) => $"leaderboard:meta:{playerId}";

    public async Task AddOrUpdateScoreAsync(Guid playerId, string displayName, int score, int highestFloor, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();

        // Sorted set: score là rank key
        await db.SortedSetAddAsync(LeaderboardKey, playerId.ToString(), score);

        // Lưu metadata riêng
        var meta = new LeaderboardEntry
        {
            PlayerId = playerId,
            DisplayName = displayName,
            Score = score,
            HighestFloor = highestFloor,
            AchievedAt = DateTime.UtcNow
        };
        await db.StringSetAsync(PlayerMetaKey(playerId), JsonSerializer.Serialize(meta), TimeSpan.FromDays(7));
    }

    public async Task<List<LeaderboardEntry>> GetTopAsync(int count = 10, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        // Lấy top N theo score giảm dần
        var entries = await db.SortedSetRangeByRankWithScoresAsync(LeaderboardKey, 0, count - 1, Order.Descending);

        var result = new List<LeaderboardEntry>();
        foreach (var entry in entries)
        {
            if (!Guid.TryParse(entry.Element.ToString(), out var playerId)) continue;
            var meta = await GetPlayerScoreAsync(playerId, ct);
            if (meta is not null)
                result.Add(meta);
        }
        return result;
    }

    public async Task<int?> GetPlayerRankAsync(Guid playerId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var rank = await db.SortedSetRankAsync(LeaderboardKey, playerId.ToString(), Order.Descending);
        return rank.HasValue ? (int)rank.Value + 1 : null;
    }

    public async Task<LeaderboardEntry?> GetPlayerScoreAsync(Guid playerId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(PlayerMetaKey(playerId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<LeaderboardEntry>(value!);
    }
}
