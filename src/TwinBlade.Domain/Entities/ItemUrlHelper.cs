namespace TwinBlade.Domain.Entities;

public static class ItemUrlHelper
{
    public static string GetItemImageUrl(Guid itemId, string baseUrl, string itemPathPrefix)
        => $"{baseUrl.TrimEnd('/')}/{itemPathPrefix.Trim('/')}/{itemId}.png";
}
