using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Room;

public sealed record GetRoomQuery(Guid RoomId) : IRequest<RoomResponse?>;
