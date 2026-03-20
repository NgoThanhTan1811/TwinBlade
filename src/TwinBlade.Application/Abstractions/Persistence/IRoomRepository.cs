using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Room?> GetByCodeAsync(string roomCode, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
