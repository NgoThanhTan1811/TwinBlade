using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed class GetPlayerProgressQueryHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerProgressQuery, PlayerProgressResponse?>
{
    public async Task<PlayerProgressResponse?> Handle(GetPlayerProgressQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdWithItemsAsync(request.PlayerId, cancellationToken);
        if (player is null) return null;

        var progress = player.Progress;
        var items = player.InventoryItems.Select(pi => new PlayerItemResponse(
            pi.ItemId,
            pi.Item.Code,
            pi.Item.Name,
            pi.Quantity
        )).ToList();

        return new PlayerProgressResponse(
            progress.Gold,
            progress.HasBossCard,
            progress.UpdatedAt,
            items
        );
    }
}
