using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Caching;

/// <summary>
/// Leaderboard tạm thời trong Redis (sorted set)
/// </summary>
public interface ILeaderboardService
{
    Task AddOrUpdateScoreAsync(Guid playerId, string displayName, int score, int highestFloor, CancellationToken ct = default);
    Task<List<LeaderboardEntry>> GetTopAsync(int count = 10, CancellationToken ct = default);
    Task<int?> GetPlayerRankAsync(Guid playerId, CancellationToken ct = default);
    Task<LeaderboardEntry?> GetPlayerScoreAsync(Guid playerId, CancellationToken ct = default);
}
