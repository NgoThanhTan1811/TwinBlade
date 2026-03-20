using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed class GetPlayerProgressQueryHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerProgressQuery, PlayerProgressResponse?>
{
    public async Task<PlayerProgressResponse?> Handle(GetPlayerProgressQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdAsync(request.PlayerId, cancellationToken);
        if (player is null) return null;

        var progress = player.Progress;
        return new PlayerProgressResponse(
            progress.Gold,
            progress.Inventory.Select(i => new PlayerItemResponse(i.Id, i.ItemId, i.Quantity)).ToList()
        );
    }
}
