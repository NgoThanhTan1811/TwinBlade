using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Room;

public sealed record StartGameCommand(Guid RoomId, Guid HostPlayerId) : IRequest<RoomResponse>;
