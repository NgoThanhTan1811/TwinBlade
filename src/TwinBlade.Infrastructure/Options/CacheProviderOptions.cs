namespace TwinBlade.Infrastructure.Options;

public class CacheProviderOptions
{
    public const string SectionName = "CacheProvider";

    /// <summary>
    /// Cache provider type: "Redis" or "InMemory"
    /// </summary>
    public string Provider { get; set; } = "Redis";

    /// <summary>
    /// Use InMemory cache for debugging/temporary use
    /// </summary>
    public bool UseInMemoryDebug { get; set; } = false;
}
