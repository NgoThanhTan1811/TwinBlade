namespace TwinBlade.Domain.Entities;

/// <summary>
/// Entry trong leaderboard tạm thời (Redis sorted set)
/// </summary>
public class LeaderboardEntry
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int HighestFloor { get; set; }
    public DateTime AchievedAt { get; set; }
}
