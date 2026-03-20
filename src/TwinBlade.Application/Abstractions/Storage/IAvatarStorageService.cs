namespace TwinBlade.Application.Abstractions.Storage
{
    public interface IAvatarStorageService
    {
        Task<string> UploadAsync(Guid playerId, Stream stream, string contentType, CancellationToken ct = default);
    }
}