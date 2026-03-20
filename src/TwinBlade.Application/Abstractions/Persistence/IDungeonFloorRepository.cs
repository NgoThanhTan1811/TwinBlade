using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Persistence;

public interface IDungeonFloorRepository
{
    Task<DungeonFloor?> GetByFloorNumberAsync(int floorNumber, CancellationToken ct = default);
    Task<List<DungeonFloor>> GetAllFloorsAsync(CancellationToken ct = default);
}
