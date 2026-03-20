namespace TwinBlade.Application.Dtos.Response;

public sealed record MatchResultResponse(
    Guid Id,
    Guid RoomId,
    DateTime FinishedAt,
    List<PlayerMatchResultResponse> Players
);

public sealed record PlayerMatchResultResponse(
    Guid PlayerId,
    int Score,
    int EarnedGold
);
