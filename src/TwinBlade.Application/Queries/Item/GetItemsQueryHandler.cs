using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Item;

public sealed class GetItemsQueryHandler(IItemRepository itemRepository)
    : IRequestHandler<GetItemsQuery, List<ItemResponse>>
{
    public async Task<List<ItemResponse>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await itemRepository.GetAllActiveAsync(cancellationToken);
        return items.Select(i => new ItemResponse(
            i.Id,
            i.Code,
            i.ItemType.Code,
            i.ItemMaterial.Code,
            i.Name,
            i.Description,
            i.ImageUrl
        )).ToList();
    }
}
