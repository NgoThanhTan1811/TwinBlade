namespace TwinBlade.Application.Dtos.Response;

public sealed record PlayerResponse(
    Guid Id,
    string CognitoUserId,
    string Username,
    string Email,
    string AvatarUrl,
    int Gold,
    DateTime CreatedAt
);
