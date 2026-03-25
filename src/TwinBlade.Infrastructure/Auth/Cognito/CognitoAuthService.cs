using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Infrastructure.Options;

namespace TwinBlade.Infrastructure.Auth.Cognito;

public sealed class CognitoAuthService(
    IAmazonCognitoIdentityProvider cognito,
    IOptions<CognitoOptions> options) : ICognitoAuthService
{
    private readonly CognitoOptions _options = options.Value;

    private static string ComputeSecretHash(string username, string clientId, string clientSecret)
    {
        var key = Encoding.UTF8.GetBytes(clientSecret);
        var message = Encoding.UTF8.GetBytes(username + clientId);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(message);
        return Convert.ToBase64String(hash);
    }

    public async Task<string> SignUpAsync(
        string email,
        string password,
        string username,
        CancellationToken ct = default)
    {
        var request = new SignUpRequest
        {
            ClientId = _options.ClientId,
            Username = email,
            Password = password,
            SecretHash = ComputeSecretHash(email, _options.ClientId, _options.ClientSecret),
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = email },
                new() { Name = "preferred_username", Value = username }
            }
        };
        
        await cognito.AdminAddUserToGroupAsync(new AdminAddUserToGroupRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = email,
            GroupName = "User"
        }, ct);

        var response = await cognito.SignUpAsync(request, ct);

        return response.UserSub;
    }

    public async Task ConfirmSignUpAsync(
        string email,
        string confirmationCode,
        CancellationToken ct = default)
    {
        var request = new ConfirmSignUpRequest
        {
            ClientId = _options.ClientId,
            Username = email,
            ConfirmationCode = confirmationCode,
            SecretHash = ComputeSecretHash(email, _options.ClientId, _options.ClientSecret)
        };

        await cognito.ConfirmSignUpAsync(request, ct);
    }

    public async Task ResendConfirmationCodeAsync(
        string email,
        CancellationToken ct = default)
    {
        var request = new ResendConfirmationCodeRequest
        {
            ClientId = _options.ClientId,
            Username = email,
            SecretHash = ComputeSecretHash(email, _options.ClientId, _options.ClientSecret)
        };

        await cognito.ResendConfirmationCodeAsync(request, ct);
    }

    public async Task<AuthResult> SignInAsync(
    string email,
    string password,
    CancellationToken ct = default)
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                ClientId = _options.ClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    ["USERNAME"] = email,
                    ["PASSWORD"] = password,
                    ["SECRET_HASH"] = ComputeSecretHash(email, _options.ClientId, _options.ClientSecret)
                }
            };

            var response = await cognito.InitiateAuthAsync(request, ct);
            var result = response.AuthenticationResult;

            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
            {
                throw new InvalidOperationException("Cognito did not return a valid access token.");
            }

            return new AuthResult
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            };
        }
        catch (UserNotConfirmedException ex)
        {
            throw new InvalidOperationException("User has not verified their email yet.", ex);
        }
    }
    
    public async Task<AuthResult> RefreshTokenAsync(
        string email,
        string refreshToken,
        CancellationToken ct = default)
    {
        var request = new InitiateAuthRequest
        {
            ClientId = _options.ClientId,
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                ["REFRESH_TOKEN"] = refreshToken,
                ["SECRET_HASH"] = ComputeSecretHash(email, _options.ClientId, _options.ClientSecret)
            }
        };

        var response = await cognito.InitiateAuthAsync(request, ct);
        var result = response.AuthenticationResult;

        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Cognito did not return a valid access token during refresh.");
        }

        return new AuthResult
        {
            AccessToken = result.AccessToken,
            RefreshToken = string.IsNullOrWhiteSpace(result.RefreshToken)
                ? refreshToken
                : result.RefreshToken
        };
    }
}