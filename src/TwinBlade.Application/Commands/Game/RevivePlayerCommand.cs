using MediatR;

namespace TwinBlade.Application.Commands.Game;

public sealed record RevivePlayerCommand(Guid RoomId, Guid ReviverPlayerId, Guid TargetPlayerId) : IRequest<Unit>;
