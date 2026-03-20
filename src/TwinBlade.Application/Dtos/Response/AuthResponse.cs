namespace TwinBlade.Application.Dtos.Response;

public sealed record AuthResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string IdToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}