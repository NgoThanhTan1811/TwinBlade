namespace TwinBlade.Application.Abstractions.Storage;

public interface IAssetUrlService
{
    string GetDefaultAvatarUrl();
    string GetItemImageUrl(Guid itemId);
}