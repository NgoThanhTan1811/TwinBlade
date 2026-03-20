namespace TwinBlade.Application.Dtos.Request;

public sealed record SubmitMatchResultRequest(
    Guid RoomId,
    List<PlayerMatchResultRequest> Players
);

public sealed record PlayerMatchResultRequest(
    Guid PlayerId,
    int Score,
    int EarnedGold
);
