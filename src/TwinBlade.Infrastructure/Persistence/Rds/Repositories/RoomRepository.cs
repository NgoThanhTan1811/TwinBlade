using Microsoft.EntityFrameworkCore;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Persistence.Rds.Repositories;

public sealed class RoomRepository(AppDbContext dbContext) : IRoomRepository
{
    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Room?> GetByCodeAsync(string roomCode, CancellationToken ct = default)
        => await dbContext.Rooms.FirstOrDefaultAsync(r => r.RoomCode == roomCode, ct);

    public async Task AddAsync(Room room, CancellationToken ct = default)
        => await dbContext.Rooms.AddAsync(room, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
