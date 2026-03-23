using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Abstractions.Persistence;

public interface IMatchResultRepository
{
    Task<MatchResult?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<MatchResult>> GetAllAsync(CancellationToken ct = default);
    Task<List<MatchResult>> GetByRoomIdAsync(Guid roomId, CancellationToken ct = default);
    Task AddAsync(MatchResult matchResult, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
