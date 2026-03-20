namespace TwinBlade.Application.Dtos.Response;

public sealed record LeaderboardResponse(
    List<LeaderboardEntryResponse> Entries,
    int? CurrentPlayerRank
);

public sealed record LeaderboardEntryResponse(
    int Rank,
    Guid PlayerId,
    string DisplayName,
    int Score,
    int HighestFloor,
    DateTime AchievedAt
);
