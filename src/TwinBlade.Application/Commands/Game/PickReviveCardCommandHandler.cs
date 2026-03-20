using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Realtime;

namespace TwinBlade.Application.Commands.Game;

public sealed class PickReviveCardCommandHandler(
    IRoomStateService roomStateService,
    IGameHubService gameHubService) : IRequestHandler<PickReviveCardCommand, Unit>
{
    public async Task<Unit> Handle(PickReviveCardCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"room:{request.RoomId}:lock";
        var acquired = await roomStateService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(5), cancellationToken);
        
        if (!acquired)
            throw new InvalidOperationException("Could not acquire room lock");

        try
        {
            var playerState = await roomStateService.GetPlayerStateAsync(request.RoomId, request.PlayerId, cancellationToken)
                              ?? throw new InvalidOperationException("Player state not found");

            if (!playerState.IsAlive)
                throw new InvalidOperationException("Dead players cannot pick up items");

            playerState.ReviveCardsCount++;
            playerState.HasReviveCard = true;
            playerState.LastUpdateAt = DateTime.UtcNow;

            await roomStateService.UpdatePlayerStateAsync(request.RoomId, playerState, cancellationToken);

            await gameHubService.NotifyPlayerPickedReviveCardAsync(
                request.RoomId,
                request.PlayerId,
                playerState.ReviveCardsCount,
                cancellationToken);

            return Unit.Value;
        }
        finally
        {
            await roomStateService.ReleaseLockAsync(lockKey, cancellationToken);
        }
    }
}
