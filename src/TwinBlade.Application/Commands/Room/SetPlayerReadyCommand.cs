using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Room;

public sealed record SetPlayerReadyCommand(Guid RoomId, Guid PlayerId, bool IsReady) : IRequest<RoomResponse>;
