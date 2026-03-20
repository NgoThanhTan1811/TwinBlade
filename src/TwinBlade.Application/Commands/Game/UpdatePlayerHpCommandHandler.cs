using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Realtime;

namespace TwinBlade.Application.Commands.Game;

public sealed class UpdatePlayerHpCommandHandler(
    IRoomStateService roomStateService,
    IGameHubService gameHubService) : IRequestHandler<UpdatePlayerHpCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePlayerHpCommand request, CancellationToken cancellationToken)
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
                throw new InvalidOperationException("Player is already dead");

            playerState.CurrentHp = Math.Max(0, playerState.CurrentHp - request.Damage);
            
            if (playerState.CurrentHp == 0)
                playerState.IsAlive = false;

            playerState.LastUpdateAt = DateTime.UtcNow;

            await roomStateService.UpdatePlayerStateAsync(request.RoomId, playerState, cancellationToken);

            // Notify all clients in room
            await gameHubService.NotifyPlayerHpChangedAsync(
                request.RoomId, 
                request.PlayerId, 
                playerState.CurrentHp, 
                playerState.MaxHp, 
                cancellationToken);

            return Unit.Value;
        }
        finally
        {
            await roomStateService.ReleaseLockAsync(lockKey, cancellationToken);
        }
    }
}
