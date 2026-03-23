using Microsoft.Extensions.Options;
using TwinBlade.Application.Abstractions.Storage;
using TwinBlade.Domain.Entities;
using TwinBlade.Infrastructure.Options;

namespace TwinBlade.Infrastructure.Storage.S3;

public sealed class S3AssetUrlService(IOptions<S3Options> options) : IAssetUrlService
{
    private readonly S3Options _options = options.Value;

    public string GetDefaultAvatarUrl()
        => $"{_options.BaseUrl.TrimEnd('/')}/{_options.AvatarPathPrefix.Trim('/')}/{_options.DefaultAvatarFileName}";

    public string GetItemImageUrl(Guid itemId)
        => ItemUrlHelper.GetItemImageUrl(itemId, _options.BaseUrl, _options.ItemPathPrefix);
}