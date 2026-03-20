using Microsoft.EntityFrameworkCore;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Persistence.Rds.Repositories;

public sealed class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Items.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<List<Item>> GetAllActiveAsync(CancellationToken ct = default)
        => await dbContext.Items.Where(i => i.IsActive).ToListAsync(ct);
}
