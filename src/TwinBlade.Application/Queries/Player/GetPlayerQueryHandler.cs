using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed class GetPlayerQueryHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerQuery, PlayerResponse?>
{
    public async Task<PlayerResponse?> Handle(GetPlayerQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdAsync(request.PlayerId, cancellationToken);
        if (player is null) return null;

        return new PlayerResponse(
            player.Id,
            player.Username,
            player.DisplayName,
            player.AvatarUrl,
            player.Progress.Gold,
            player.CreatedAt
        );
    }
}
