using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Game;

public sealed record GetRoomStateQuery(Guid RoomId) : IRequest<RoomStateResponse?>;
