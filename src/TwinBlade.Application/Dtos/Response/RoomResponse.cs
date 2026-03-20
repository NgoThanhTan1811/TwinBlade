using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Dtos.Response;

public sealed record RoomResponse(
    Guid Id,
    string RoomCode,
    Guid HostPlayerId,
    RoomStatus Status,
    int MaxPlayers,
    List<RoomPlayerResponse> Players,
    DateTime CreatedAt
);

public sealed record RoomPlayerResponse(
    Guid PlayerId,
    string DisplayName,
    bool IsReady
);
