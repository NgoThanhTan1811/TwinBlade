using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Dtos.Response;

public sealed record RoomStateResponse(
    Guid RoomId,
    string RoomCode,
    RoomStatus Status,
    bool BossMapActivated,
    bool BossDefeated,
    List<PlayerStateResponse> Players,
    DateTime GameStartedAt,
    DateTime LastActivityAt
);

public sealed record PlayerStateResponse(
    Guid PlayerId,
    string DisplayName
);
