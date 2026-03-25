using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Item;

public sealed class GetItemsByTypeQueryHandler(IItemRepository itemRepository)
    : IRequestHandler<GetItemsByTypeQuery, List<ItemResponse>>
{
    public async Task<List<ItemResponse>> Handle(GetItemsByTypeQuery request, CancellationToken cancellationToken)
    {
        var items = await itemRepository.GetByTypeCodeAsync(request.TypeCode, cancellationToken);
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
