using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using DomainEntities = TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Commands.Room;

public sealed class CreateRoomCommandHandler(
    IRoomRepository roomRepository,
    IRoomCacheService roomCacheService)
    : IRequestHandler<CreateRoomCommand, RoomResponse>
{
    public async Task<RoomResponse> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var roomCode = GenerateRoomCode();

        var room = new DomainEntities.Room
        {
            Id = Guid.NewGuid(),
            RoomCode = roomCode,
            HostPlayerId = request.HostPlayerId,
            Status = DomainEntities.RoomStatus.Waiting,
            MaxPlayers = request.MaxPlayers,
            CreatedAt = DateTime.UtcNow,
            Players = new List<DomainEntities.RoomPlayer>
            {
                new() { PlayerId = request.HostPlayerId, IsReady = false }
            }
        };

        await roomRepository.AddAsync(room, cancellationToken);
        await roomRepository.SaveChangesAsync(cancellationToken);
        await roomCacheService.SetRoomCodeAsync(roomCode, room.Id, cancellationToken);

        return MapToResponse(room);
    }

    private static string GenerateRoomCode()
        => Guid.NewGuid().ToString("N")[..6].ToUpper();

    private static RoomResponse MapToResponse(DomainEntities.Room room) => new(
        room.Id,
        room.RoomCode,
        room.HostPlayerId,
        room.Status,
        room.MaxPlayers,
        room.Players.Select(p => new RoomPlayerResponse(p.PlayerId, p.DisplayName, p.IsReady)).ToList(),
        room.CreatedAt
    );
}
