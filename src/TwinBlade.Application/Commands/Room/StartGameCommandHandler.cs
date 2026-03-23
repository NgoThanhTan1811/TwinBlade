using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Abstractions.Realtime;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Commands.Room;

public sealed class StartGameCommandHandler(
    IRoomRepository roomRepository,
    IRoomStateService roomStateService,
    IGameHubService gameHubService) : IRequestHandler<StartGameCommand, RoomResponse>
{
    private const int DefaultMaxHp = 100;
    private const int DefaultAttack = 15;
    private const int DefaultDefense = 5;
    private const int TotalFloors = 10;

    public async Task<RoomResponse> Handle(StartGameCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
                   ?? throw new InvalidOperationException("Room not found.");

        if (room.HostPlayerId != request.HostPlayerId)
            throw new InvalidOperationException("Only the host can start the game.");

        if (room.Status != RoomStatus.Waiting)
            throw new InvalidOperationException("Room is not in waiting state.");

        if (room.Players.Count < 2)
            throw new InvalidOperationException("Need at least 2 players to start.");

        if (!room.Players.Where(p => p.PlayerId != room.HostPlayerId).All(p => p.IsReady))
            throw new InvalidOperationException("Not all players are ready.");

        room.Status = RoomStatus.InGame;
        await roomRepository.SaveChangesAsync(cancellationToken);

        // Khởi tạo runtime state trong Redis
        var runtimeState = new RoomRuntimeState
        {
            RoomId = room.Id,
            RoomCode = room.RoomCode,
            Status = RoomStatus.InGame,
            BossMapActivated = false,
            BossDefeated = false,
            GameStartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            Version = 1,
            Players = [.. room.Players.Select(p => new RoomPlayerState
            {
                PlayerId = p.PlayerId,
                DisplayName = p.DisplayName,
                LastUpdateAt = DateTime.UtcNow
            })]
        };

        await roomStateService.SetRoomStateAsync(runtimeState, cancellationToken);
        await gameHubService.NotifyGameStartedAsync(room.Id, cancellationToken);

        return new RoomResponse(
            room.Id, room.RoomCode, room.HostPlayerId, room.Status, room.MaxPlayers,
            room.Players.Select(p => new RoomPlayerResponse(p.PlayerId, p.DisplayName, p.IsReady)).ToList(),
            room.CreatedAt
        );
    }
}
