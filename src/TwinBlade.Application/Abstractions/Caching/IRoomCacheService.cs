namespace TwinBlade.Application.Abstractions.Caching
{
    public interface IRoomCacheService
    {
        Task SetRoomCodeAsync(string roomCode, Guid roomId, CancellationToken ct = default);
        Task<Guid?> GetRoomIdByCodeAsync(string roomCode, CancellationToken ct = default);
    }
}