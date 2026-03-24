using MediatR;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Abstractions.Storage;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;
using PlayerEntity = TwinBlade.Domain.Entities.Player;

namespace TwinBlade.Application.Commands.Player;

public sealed class RegisterPlayerCommandHandler(
    IPlayerRepository playerRepository,
    ICognitoAuthService cognitoAuthService,
    IAssetUrlService assetUrlService)
    : IRequestHandler<RegisterPlayerCommand, PlayerResponse>
{
    public async Task<PlayerResponse> Handle(RegisterPlayerCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        var existing = await playerRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Player '{request.Username}' already exists.");

        // Sign up user in Cognito (will throw if email already exists)
        var cognitoUserId = await cognitoAuthService.SignUpAsync(
            request.Email,
            request.Password,
            request.Username,
            cancellationToken);

        var player = new PlayerEntity
        {
            Id = Guid.NewGuid(),
            CognitoId = cognitoUserId,
            Username = request.Username,
            Email = request.Email,
            AvatarUrl = assetUrlService.GetDefaultAvatarUrl(),
            CreatedAt = DateTime.UtcNow,
            Progress = new PlayerProgress { Gold = 0, Inventory = [] }
        };

        await playerRepository.AddAsync(player, cancellationToken);
        await playerRepository.SaveChangesAsync(cancellationToken);

        return new PlayerResponse(
            player.Id,
            player.CognitoId,
            player.Username,
            player.Email,
            player.AvatarUrl,
            0,
            player.CreatedAt
        );
    }
}
