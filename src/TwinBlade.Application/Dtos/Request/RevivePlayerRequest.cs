namespace TwinBlade.Application.Dtos.Request;

public sealed record RevivePlayerRequest(Guid RoomId, Guid TargetPlayerId);
