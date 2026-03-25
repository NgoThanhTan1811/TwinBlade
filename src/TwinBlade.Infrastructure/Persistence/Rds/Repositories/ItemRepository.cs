using Microsoft.EntityFrameworkCore;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Domain.Entities;
using TwinBlade.Domain.Items;

namespace TwinBlade.Infrastructure.Persistence.Rds.Repositories;

public sealed class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Items
            .Include(i => i.ItemType)
            .Include(i => i.ItemMaterial)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<List<Item>> GetAllActiveAsync(CancellationToken ct = default)
        => await dbContext.Items
            .Include(i => i.ItemType)
            .Include(i => i.ItemMaterial)
            .Where(i => i.IsActive)
            .ToListAsync(ct);

    public async Task<List<Item>> GetByTypeCodeAsync(string typeCode, CancellationToken ct = default)
        => await dbContext.Items
            .Include(i => i.ItemType)
            .Include(i => i.ItemMaterial)
            .Where(i => i.IsActive && i.ItemType.Code == typeCode)
            .ToListAsync(ct);

    public async Task AddAsync(Item item, CancellationToken ct = default)
        => await dbContext.Items.AddAsync(item, ct);

    public async Task<ItemType> GetOrCreateItemTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        var itemType = await dbContext.ItemTypes.FirstOrDefaultAsync(t => t.Code == code, ct);

        if (itemType is null)
        {
            itemType = new ItemType
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = CapitalizeFirst(code),
                CreatedAt = DateTime.UtcNow
            };
            await dbContext.ItemTypes.AddAsync(itemType, ct);
        }

        return itemType;
    }

    public async Task<ItemMeterials> GetOrCreateItemMaterialByCodeAsync(string code, CancellationToken ct = default)
    {
        var material = await dbContext.ItemMaterials.FirstOrDefaultAsync(m => m.Code == code, ct);

        if (material is null)
        {
            material = new ItemMeterials
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = CapitalizeFirst(code),
                CreatedAt = DateTime.UtcNow
            };
            await dbContext.ItemMaterials.AddAsync(material, ct);
        }

        return material;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);

    private static string CapitalizeFirst(string str)
        => string.IsNullOrEmpty(str) ? str : char.ToUpper(str[0]) + str[1..];
}
