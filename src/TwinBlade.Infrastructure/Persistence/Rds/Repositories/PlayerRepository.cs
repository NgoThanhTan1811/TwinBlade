using Microsoft.EntityFrameworkCore;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Persistence.Rds.Repositories;

public sealed class PlayerRepository(AppDbContext dbContext) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Players
            .Include(x => x.Progress)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Player?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Players
            .Include(x => x.Progress)
            .Include(x => x.InventoryItems)
                .ThenInclude(pi => pi.Item)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Player?> GetByIdWithEquipmentAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Players
            .Include(x => x.Progress)
            .Include(x => x.EquippedItems)
                .ThenInclude(e => e.Item)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Player?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await dbContext.Players
            .Include(x => x.Progress)
            .FirstOrDefaultAsync(x => x.Username == username, ct);

    public async Task<Player?> GetByCognitoIdAsync(string cognitoId, CancellationToken ct = default)
        => await dbContext.Players
            .Include(x => x.Progress)
            .FirstOrDefaultAsync(x => x.CognitoId == cognitoId, ct);

    public async Task AddAsync(Player player, CancellationToken ct = default)
        => await dbContext.Players.AddAsync(player, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
