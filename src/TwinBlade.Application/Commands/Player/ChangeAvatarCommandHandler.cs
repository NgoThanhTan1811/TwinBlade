using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Abstractions.Storage;

namespace TwinBlade.Application.Commands.Player;

public sealed class ChangeAvatarCommandHandler(
    IPlayerRepository playerRepository,
    IAvatarService avatarService) : IRequestHandler<ChangeAvatarCommand>
{
    public async Task Handle(ChangeAvatarCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdAsync(request.PlayerId, cancellationToken);
        if (player is null)
            throw new InvalidOperationException($"Player with ID '{request.PlayerId}' not found.");

        // Validate avatar exists in S3
        var availableAvatars = await avatarService.GetAvailableAvatarsAsync(cancellationToken);
        if (!availableAvatars.Contains(request.AvatarFileName))
            throw new InvalidOperationException($"Avatar '{request.AvatarFileName}' does not exist.");

        // Update player avatar URL
        var avatarUrl = await avatarService.GetAvatarUrlAsync(request.AvatarFileName, cancellationToken);
        player.AvatarUrl = avatarUrl;

        await playerRepository.SaveChangesAsync(cancellationToken);
    }
}
