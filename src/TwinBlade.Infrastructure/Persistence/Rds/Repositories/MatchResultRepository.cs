using Microsoft.EntityFrameworkCore;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Infrastructure.Persistence.Rds.Repositories;

public sealed class MatchResultRepository(AppDbContext dbContext) : IMatchResultRepository
{
    public async Task<MatchResult?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.MatchResults.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<List<MatchResult>> GetByRoomIdAsync(Guid roomId, CancellationToken ct = default)
        => await dbContext.MatchResults.Where(m => m.RoomId == roomId).ToListAsync(ct);

    public async Task AddAsync(MatchResult matchResult, CancellationToken ct = default)
        => await dbContext.MatchResults.AddAsync(matchResult, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
