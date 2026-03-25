using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Items;

namespace TwinBlade.Application.Commands.Item;

public sealed class CreateItemCommandHandler(IItemRepository itemRepository)
    : IRequestHandler<CreateItemCommand, ItemResponse>
{
    public async Task<ItemResponse> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // Parse code: {type}_{material} (e.g., "kiem_go" -> type="kiem", material="go")
        var parts = request.Code.Split('_');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid code format. Expected format: {{type}}_{{material}}, got: {request.Code}");
        }

        var typeCode = parts[0];
        var materialCode = parts[1];

        // Find or create ItemType
        var itemType = await itemRepository.GetOrCreateItemTypeByCodeAsync(typeCode, cancellationToken);

        // Find or create ItemMaterial
        var itemMaterial = await itemRepository.GetOrCreateItemMaterialByCodeAsync(materialCode, cancellationToken);

        // Create the Item
        var item = new Domain.Items.Item
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            ItemTypeId = itemType.Id,
            ItemType = itemType,
            ItemMaterialId = itemMaterial.Id,
            ItemMaterial = itemMaterial,
            Name = request.Name,
            IsActive = true,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? $"{itemType.Name} made of {itemMaterial.Name}"
                : request.Description,
            ImageUrl = string.Empty
        };

        await itemRepository.AddAsync(item, cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return new ItemResponse(
            item.Id,
            item.Code,
            itemType.Code,
            itemMaterial.Code,
            item.Name,
            item.Description,
            item.ImageUrl
        );
    }
}
