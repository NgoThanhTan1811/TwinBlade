using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Game;

public sealed record ActivateBossMapCommand(Guid RoomId, Guid PlayerId) : IRequest<RoomStateResponse>;