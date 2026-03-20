using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Dtos.Response;

public sealed record RoomStateResponse(
    Guid RoomId,
    string RoomCode,
    RoomStatus Status,
    int CurrentFloor,
    int TotalFloors,
    bool BossDefeated,
    List<PlayerStateResponse> Players,
    DateTime GameStartedAt,
    DateTime LastActivityAt
);

public sealed record PlayerStateResponse(
    Guid PlayerId,
    string DisplayName,
    int CurrentHp,
    int MaxHp,
    int AttackPower,
    int Defense,
    bool IsAlive,
    int ReviveCardsCount,
    float PositionX,
    float PositionY,
    float PositionZ
);
