using MediatR;

namespace TwinBlade.Application.Commands.Game;

public sealed record UpdatePlayerHpCommand(Guid RoomId, Guid PlayerId, int Damage) : IRequest<Unit>;
