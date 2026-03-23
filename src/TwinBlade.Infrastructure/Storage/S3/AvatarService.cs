using Amazon.S3;
using Amazon.S3.Model;
using TwinBlade.Application.Abstractions.Storage;
using TwinBlade.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace TwinBlade.Infrastructure.Storage.S3;

public sealed class AvatarService(
    IAmazonS3 s3Client,
    IOptions<S3Options> options) : IAvatarService
{
    private readonly S3Options _options = options.Value;

    public async Task<List<string>> GetAvailableAvatarsAsync(CancellationToken ct = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _options.BucketName,
            Prefix = $"{_options.AvatarPathPrefix}/",
            MaxKeys = 100
        };

        var response = await s3Client.ListObjectsV2Async(request, ct);

        return response.S3Objects
            .Where(obj => !obj.Key.EndsWith("/")) // Exclude folders
            .Select(obj => obj.Key.Replace($"{_options.AvatarPathPrefix}/", ""))
            .ToList();
    }

    public Task<string> GetAvatarUrlAsync(string avatarFileName, CancellationToken ct = default)
    {
        var url = $"{_options.BaseUrl}/{_options.AvatarPathPrefix}/{avatarFileName}";
        return Task.FromResult(url);
    }
}
