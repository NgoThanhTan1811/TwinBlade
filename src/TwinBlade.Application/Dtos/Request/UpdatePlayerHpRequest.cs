namespace TwinBlade.Application.Dtos.Request;

public sealed record UpdatePlayerHpRequest(Guid RoomId, int Damage);
