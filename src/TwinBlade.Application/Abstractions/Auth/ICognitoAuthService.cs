using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Abstractions.Auth;

public interface ICognitoAuthService
{
    Task<string> SignUpAsync(string email, string password, string username, CancellationToken ct = default);
    Task<AuthResult> SignInAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(string email, string refreshToken, CancellationToken ct = default);
}