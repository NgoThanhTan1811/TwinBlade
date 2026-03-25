using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed class GetPlayerEquipmentQueryHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerEquipmentQuery, List<PlayerEquipmentResponse>?>
{
    public async Task<List<PlayerEquipmentResponse>?> Handle(GetPlayerEquipmentQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdWithEquipmentAsync(request.PlayerId, cancellationToken);
        if (player is null) return null;

        return player.EquippedItems.Select(e => new PlayerEquipmentResponse(
            e.Slot.ToString(),
            e.ItemId,
            e.Item?.Code,
            e.Item?.Name
        )).ToList();
    }
}
