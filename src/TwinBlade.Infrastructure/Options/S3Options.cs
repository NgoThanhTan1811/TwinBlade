namespace TwinBlade.Infrastructure.Options;

public sealed class S3Options
{
    public const string SectionName = "S3";

    public string AvatarBucket { get; set; } = string.Empty;
    public string SaveBucket { get; set; } = string.Empty;
    public string Region { get; set; } = "ap-southeast-1";
}
