using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Room;

public sealed class GetRoomQueryHandler(IRoomRepository roomRepository)
    : IRequestHandler<GetRoomQuery, RoomResponse?>
{
    public async Task<RoomResponse?> Handle(GetRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken);
        if (room is null) return null;

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
