namespace TwinBlade.Application.Dtos.Request;

public sealed record RefreshTokenRequest(string Email, string RefreshToken);
