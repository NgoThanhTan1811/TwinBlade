using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace TwinBlade.Infrastructure.Auth.Cognito;

public sealed class CognitoAuthService(
    IAmazonCognitoIdentityProvider cognito,
    IOptions<CognitoOptions> options) : ICognitoAuthService
{
    private readonly CognitoOptions _options = options.Value;

    public async Task<string> SignUpAsync(string email, string password, string username, CancellationToken ct = default)
    {
        var request = new SignUpRequest
        {
            ClientId = _options.ClientId,
            Username = email,
            Password = password,
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = email },
                new() { Name = "preferred_username", Value = username }
            }
        };

        var response = await cognito.SignUpAsync(request, ct);

        // Auto-confirm user (for development/testing)
        var confirmRequest = new AdminConfirmSignUpRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = email
        };
        await cognito.AdminConfirmSignUpAsync(confirmRequest, ct);

        return response.UserSub;
    }

    public async Task<AuthResult> SignInAsync(string email, string password, CancellationToken ct = default)
    {
        var request = new AdminInitiateAuthRequest
        {
            UserPoolId = _options.UserPoolId,
            ClientId = _options.ClientId,
            AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                ["USERNAME"] = email,
                ["PASSWORD"] = password
            }
        };

        var response = await cognito.AdminInitiateAuthAsync(request, ct);

        return new AuthResult
        {
            AccessToken = response.AuthenticationResult.AccessToken,
            IdToken = response.AuthenticationResult.IdToken,
            RefreshToken = response.AuthenticationResult.RefreshToken
        };
    }
}
