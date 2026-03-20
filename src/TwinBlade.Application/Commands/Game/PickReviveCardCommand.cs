using MediatR;

namespace TwinBlade.Application.Commands.Game;

public sealed record PickReviveCardCommand(Guid RoomId, Guid PlayerId) : IRequest<Unit>;
