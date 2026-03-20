using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Persistence;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Item>> GetAllActiveAsync(CancellationToken ct = default);
}
