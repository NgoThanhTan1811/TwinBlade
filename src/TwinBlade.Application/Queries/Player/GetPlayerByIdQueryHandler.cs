using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed class GetPlayerByIdQueryHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerByIdQuery, PlayerResponse?>
{
    public async Task<PlayerResponse?> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdAsync(request.PlayerId, cancellationToken);
        if (player is null) return null;

        return new PlayerResponse(
            player.Id,
            player.CognitoId,
            player.Username,
            player.Email,
            player.AvatarUrl,
            player.Progress.Gold,
            player.CreatedAt
        );
    }
}
