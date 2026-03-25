using TwinBlade.Domain.Entities;
using TwinBlade.Domain.Items;

namespace TwinBlade.Application.Abstractions.Persistence;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Item>> GetAllActiveAsync(CancellationToken ct = default);
    Task<List<Item>> GetByTypeCodeAsync(string typeCode, CancellationToken ct = default);
    Task AddAsync(Item item, CancellationToken ct = default);
    Task<ItemType> GetOrCreateItemTypeByCodeAsync(string code, CancellationToken ct = default);
    Task<ItemMeterials> GetOrCreateItemMaterialByCodeAsync(string code, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
