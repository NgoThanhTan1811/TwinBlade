using Amazon.S3;
using Amazon.S3.Model;
using TwinBlade.Application.Abstractions.Storage;
using TwinBlade.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace TwinBlade.Infrastructure.Storage.S3;

public sealed class S3AvatarStorageService(
    IAmazonS3 s3Client,
    IOptions<S3Options> options) : IAvatarStorageService
{
    private readonly S3Options _options = options.Value;

    // S3 objects are uploaded manually via AWS Console — this returns the pre-signed URL for reading
    public async Task<string> GetAvatarUrlAsync(Guid playerId, CancellationToken ct = default)
    {
        var key = $"avatars/{playerId}";
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.AvatarBucket,
            Key = key,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        return await Task.FromResult(s3Client.GetPreSignedURL(request));
    }

    // Returns the S3 key path where the avatar should be uploaded manually
    public Task<string> UploadAsync(Guid playerId, Stream stream, string contentType, CancellationToken ct = default)
    {
        // S3 uploads are handled manually via AWS Console
        // This returns the expected S3 URL for the avatar
        var key = $"avatars/{playerId}";
        var url = $"https://{_options.AvatarBucket}.s3.{_options.Region}.amazonaws.com/{key}";
        return Task.FromResult(url);
    }
}
