namespace TwinBlade.Application.Dtos.Response;

public sealed record PlayerResponse(
    Guid Id,
    string Username,
    string DisplayName,
    string AvatarUrl,
    int Gold,
    DateTime CreatedAt
);
