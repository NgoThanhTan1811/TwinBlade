namespace TwinBlade.Infrastructure.Options;

public sealed class S3Options
{
    public const string SectionName = "S3";

    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string AvatarPathPrefix { get; set; } = string.Empty;
    public string DefaultAvatarFileName { get; set; } = string.Empty;
    public string ItemPathPrefix { get; set; } = string.Empty;
}
