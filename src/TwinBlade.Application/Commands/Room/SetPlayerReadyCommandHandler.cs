using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;
using TwinBlade.Domain.Enums;

namespace TwinBlade.Application.Commands.Room;

public sealed class SetPlayerReadyCommandHandler(IRoomRepository roomRepository)
    : IRequestHandler<SetPlayerReadyCommand, RoomResponse>
{
    public async Task<RoomResponse> Handle(SetPlayerReadyCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
                   ?? throw new InvalidOperationException("Room not found.");

        if (room.Status != RoomStatus.Waiting)
            throw new InvalidOperationException("Room is not in waiting state.");

        var player = room.Players.FirstOrDefault(p => p.PlayerId == request.PlayerId)
                     ?? throw new InvalidOperationException("Player not in room.");

        player.IsReady = request.IsReady;
        await roomRepository.SaveChangesAsync(cancellationToken);

        return new RoomResponse(
            room.Id, room.RoomCode, room.HostPlayerId, room.Status, room.MaxPlayers,
            room.Players.Select(p => new RoomPlayerResponse(p.PlayerId, p.DisplayName, p.IsReady)).ToList(),
            room.CreatedAt
        );
    }
}
