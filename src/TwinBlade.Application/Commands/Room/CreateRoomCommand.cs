using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Room;

public sealed record CreateRoomCommand(Guid HostPlayerId, int MaxPlayers) : IRequest<RoomResponse>;
