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

    public async Task<Player?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await dbContext.Players
            .Include(x => x.Progress)
            .FirstOrDefaultAsync(x => x.Username == username, ct);

    public async Task AddAsync(Player player, CancellationToken ct = default)
        => await dbContext.Players.AddAsync(player, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
