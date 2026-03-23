using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Commands.Game;

public sealed class ActivateBossMapCommandHandler(
    IRoomStateService roomStateService,
    IPlayerRepository playerRepository) : IRequestHandler<ActivateBossMapCommand, RoomStateResponse>
{
    private const int RequiredBossKeys = 3;

    public async Task<RoomStateResponse> Handle(ActivateBossMapCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"room:{request.RoomId}:lock";
        var acquired = await roomStateService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(5), cancellationToken);

        if (!acquired)
            throw new InvalidOperationException("Could not acquire room lock");

        try
        {
            var roomState = await roomStateService.GetRoomStateAsync(request.RoomId, cancellationToken)
                            ?? throw new InvalidOperationException("Room runtime state not found");

            if (roomState.Status != RoomStatus.InGame)
                throw new InvalidOperationException("Room is not in game state.");

            if (!roomState.Players.Any(x => x.PlayerId == request.PlayerId))
                throw new InvalidOperationException("Player is not in this room.");

            if (roomState.BossMapActivated)
                throw new InvalidOperationException("Boss map is already activated.");

            // Validate persistent boss keys
            var player = await playerRepository.GetByIdAsync(request.PlayerId, cancellationToken)
                         ?? throw new InvalidOperationException("Player not found.");

            if (player.Progress.HasBossCrard < RequiredBossKeys)
                throw new InvalidOperationException($"Need at least {RequiredBossKeys} boss keys to activate boss map.");

            // Consume boss keys (persistent)
            player.Progress.HasBossCrard -= RequiredBossKeys;

            // Update session state
            roomState.BossMapActivated = true;
            roomState.LastActivityAt = DateTime.UtcNow;
            roomState.Version++;

            await playerRepository.SaveChangesAsync(cancellationToken);
            await roomStateService.SetRoomStateAsync(roomState, cancellationToken);

            // Mirror will handle the actual boss map activation in gameplay
            // Backend just validates and sets the flag

            return new RoomStateResponse(
                roomState.RoomId,
                roomState.RoomCode,
                roomState.Status,
                roomState.BossMapActivated,
                roomState.BossDefeated,
                roomState.Players.Select(p => new PlayerStateResponse(
                    p.PlayerId,
                    p.DisplayName
                )).ToList(),
                roomState.GameStartedAt,
                roomState.LastActivityAt
            );
        }
        finally
        {
            await roomStateService.ReleaseLockAsync(lockKey, cancellationToken);
        }
    }
}