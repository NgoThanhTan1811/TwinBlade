using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;
using TwinBlade.Domain.Enums;

namespace TwinBlade.Application.Commands.Room;

public sealed class JoinRoomCommandHandler(
    IRoomRepository roomRepository,
    IRoomCacheService roomCacheService)
    : IRequestHandler<JoinRoomCommand, RoomResponse>
{
    public async Task<RoomResponse> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var roomId = await roomCacheService.GetRoomIdByCodeAsync(request.RoomCode, cancellationToken);

        TwinBlade.Domain.Entities.Room? room = null;
        if (roomId.HasValue)
            room = await roomRepository.GetByIdAsync(roomId.Value, cancellationToken);

        if (room is null)
            room = await roomRepository.GetByCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new InvalidOperationException($"Room '{request.RoomCode}' not found.");

        if (room.Status != RoomStatus.Waiting)
            throw new InvalidOperationException("Room is not accepting players.");

        if (room.Players.Count >= room.MaxPlayers)
            throw new InvalidOperationException("Room is full.");

        if (room.Players.Any(p => p.PlayerId == request.PlayerId))
            throw new InvalidOperationException("Player already in room.");

        room.Players.Add(new RoomPlayer
        {
            PlayerId = request.PlayerId,
            DisplayName = request.DisplayName,
            IsReady = false
        });

        await roomRepository.SaveChangesAsync(cancellationToken);

        return new RoomResponse(
            room.Id,
            room.RoomCode,
            room.HostPlayerId,
            room.Status,
            room.MaxPlayers,
            room.Players.Select(p => new RoomPlayerResponse(p.PlayerId, p.DisplayName, p.IsReady)).ToList(),
            room.CreatedAt
        );
    }
}
