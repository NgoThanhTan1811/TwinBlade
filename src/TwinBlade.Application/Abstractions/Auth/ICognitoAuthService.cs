using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Abstractions.Auth;

public interface ICognitoAuthService
{
    Task<AuthResult> SignInAsync(string username, string password, CancellationToken ct = default);
}