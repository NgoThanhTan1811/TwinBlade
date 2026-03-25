namespace TwinBlade.Application.Dtos.Response;

public sealed record AuthResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed record SignUpResponse
{
    public Guid PlayerId { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool RequiresEmailVerification { get; init; }
    public string Message { get; init; } = string.Empty;
}