using Microsoft.Extensions.Options;
using TwinBlade.Application.Abstractions.Storage;
using TwinBlade.Domain.Entities;
using TwinBlade.Infrastructure.Options;

namespace TwinBlade.Infrastructure.Storage.S3;

public sealed class S3AssetUrlService(IOptions<S3Options> options) : IAssetUrlService
{
    private readonly S3Options _options = options.Value;

    public string GetDefaultAvatarUrl()
        => $"{_options.S3_BaseUrl.TrimEnd('/')}/{_options.S3_AvatarPathPrefix.Trim('/')}/{_options.DefaultAvatarFileName}";

    public string GetItemImageUrl(Guid itemId)
        => GetItemImageUrl(itemId, _options.S3_BaseUrl, _options.ItemPathPrefix);

    private static string GetItemImageUrl(Guid itemId, string S3_BaseUrl, string itemPathPrefix)
        => $"{S3_BaseUrl.TrimEnd('/')}/{itemPathPrefix.Trim('/')}/{itemId}.png";
}