using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Realtime;

namespace TwinBlade.Application.Commands.Game;

public sealed class RevivePlayerCommandHandler(
    IRoomStateService roomStateService,
    IGameHubService gameHubService) : IRequestHandler<RevivePlayerCommand, Unit>
{
    public async Task<Unit> Handle(RevivePlayerCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"room:{request.RoomId}:lock";
        var acquired = await roomStateService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(5), cancellationToken);
        
        if (!acquired)
            throw new InvalidOperationException("Could not acquire room lock");

        try
        {
            var reviverState = await roomStateService.GetPlayerStateAsync(request.RoomId, request.ReviverPlayerId, cancellationToken)
                               ?? throw new InvalidOperationException("Reviver not found");

            var targetState = await roomStateService.GetPlayerStateAsync(request.RoomId, request.TargetPlayerId, cancellationToken)
                              ?? throw new InvalidOperationException("Target player not found");

            if (!reviverState.IsAlive)
                throw new InvalidOperationException("Dead players cannot revive others");

            if (reviverState.ReviveCardsCount <= 0)
                throw new InvalidOperationException("No revive cards available");

            if (targetState.IsAlive)
                throw new InvalidOperationException("Target player is already alive");

            // Use revive card
            reviverState.ReviveCardsCount--;
            reviverState.HasReviveCard = reviverState.ReviveCardsCount > 0;
            reviverState.LastUpdateAt = DateTime.UtcNow;

            // Revive target with 50% HP
            var restoredHp = targetState.MaxHp / 2;
            targetState.CurrentHp = restoredHp;
            targetState.IsAlive = true;
            targetState.LastUpdateAt = DateTime.UtcNow;

            await roomStateService.UpdatePlayerStateAsync(request.RoomId, reviverState, cancellationToken);
            await roomStateService.UpdatePlayerStateAsync(request.RoomId, targetState, cancellationToken);

            await gameHubService.NotifyPlayerRevivedAsync(
                request.RoomId,
                request.TargetPlayerId,
                request.ReviverPlayerId,
                restoredHp,
                cancellationToken);

            return Unit.Value;
        }
        finally
        {
            await roomStateService.ReleaseLockAsync(lockKey, cancellationToken);
        }
    }
}
