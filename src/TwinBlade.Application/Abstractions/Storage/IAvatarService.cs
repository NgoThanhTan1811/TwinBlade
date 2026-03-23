namespace TwinBlade.Application.Abstractions.Storage;

public interface IAvatarService
{
    Task<List<string>> GetAvailableAvatarsAsync(CancellationToken ct = default);
    Task<string> GetAvatarUrlAsync(string avatarFileName, CancellationToken ct = default);
}
