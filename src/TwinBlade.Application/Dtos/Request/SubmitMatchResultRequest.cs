namespace TwinBlade.Application.Dtos.Request;

public sealed record SubmitMatchResultRequest(
    Guid RoomId,
    List<PlayerMatchResultRequest> Players
);

public sealed record PlayerMatchResultRequest(
    Guid PlayerId,
    int Score,
    int EarnedGold,
    List<RuntimePlayerItemDto> PickedItems
);

public sealed record RuntimePlayerItemDto(Guid ItemId, int Quantity);
