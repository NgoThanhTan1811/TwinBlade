using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;
using PlayerEntity = TwinBlade.Domain.Entities.Player;

namespace TwinBlade.Application.Commands.Player;

public sealed class RegisterPlayerCommandHandler(IPlayerRepository playerRepository)
    : IRequestHandler<RegisterPlayerCommand, PlayerResponse>
{
    public async Task<PlayerResponse> Handle(RegisterPlayerCommand request, CancellationToken cancellationToken)
    {
        var existing = await playerRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Player '{request.Username}' already exists.");

        var player = new PlayerEntity
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            DisplayName = request.DisplayName,
            AvatarUrl = string.Empty,
            CreatedAt = DateTime.UtcNow,
            Progress = new PlayerProgress { Gold = 0, Inventory = new() }
        };

        await playerRepository.AddAsync(player, cancellationToken);
        await playerRepository.SaveChangesAsync(cancellationToken);

        return new PlayerResponse(player.Id, player.Username, player.DisplayName, player.AvatarUrl, 0, player.CreatedAt);
    }
}
