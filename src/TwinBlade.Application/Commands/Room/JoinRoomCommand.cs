using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Room;

public sealed record JoinRoomCommand(string RoomCode, Guid PlayerId, string DisplayName) : IRequest<RoomResponse>;
