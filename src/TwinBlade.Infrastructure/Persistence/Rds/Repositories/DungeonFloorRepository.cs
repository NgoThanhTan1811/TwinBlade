using Microsoft.EntityFrameworkCore;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Persistence.Rds.Repositories;

public sealed class DungeonFloorRepository(AppDbContext dbContext) : IDungeonFloorRepository
{
    public async Task<DungeonFloor?> GetByFloorNumberAsync(int floorNumber, CancellationToken ct = default)
        => await dbContext.DungeonFloors.FirstOrDefaultAsync(f => f.FloorNumber == floorNumber, ct);

    public async Task<List<DungeonFloor>> GetAllFloorsAsync(CancellationToken ct = default)
        => await dbContext.DungeonFloors.OrderBy(f => f.FloorNumber).ToListAsync(ct);
}
