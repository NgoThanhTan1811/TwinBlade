using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TwinBlade.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddCognitoAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var region = Environment.GetEnvironmentVariable("Region")
                     ?? Environment.GetEnvironmentVariable("AWS_REGION")
                     ?? configuration["Cognito:Region"];
        var userPoolId = Environment.GetEnvironmentVariable("Cognito_UserPoolId")
                         ?? configuration["Cognito:UserPoolId"];
        var clientId = Environment.GetEnvironmentVariable("Cognito_ClientId")
                       ?? configuration["Cognito:ClientId"];

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new InvalidOperationException("Missing Cognito region configuration.");
        }

        if (string.IsNullOrWhiteSpace(userPoolId))
        {
            throw new InvalidOperationException("Missing Cognito user pool ID configuration.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Missing Cognito client ID configuration.");
        }

        var authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,

                    ValidateAudience = false,

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    NameClaimType = "username",
                    RoleClaimType = "cognito:groups",

                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Auth failed: {context.Exception}");
                        return Task.CompletedTask;
                    },

                    OnTokenValidated = context =>
                    {
                        var principal = context.Principal;

                        var sub = principal?.FindFirst("sub")?.Value;
                        var username = principal?.FindFirst("username")?.Value
                                       ?? principal?.FindFirst("cognito:username")?.Value;
                        var email = principal?.FindFirst("email")?.Value;
                        var tokenUse = principal?.FindFirst("token_use")?.Value;
                        var tokenClientId = principal?.FindFirst("client_id")?.Value;

                        if (!string.Equals(tokenUse, "access", StringComparison.Ordinal))
                        {
                            context.Fail("Only access tokens are allowed.");
                            return Task.CompletedTask;
                        }

                        if (!string.Equals(tokenClientId, clientId, StringComparison.Ordinal))
                        {
                            context.Fail("Invalid client_id.");
                            return Task.CompletedTask;
                        }

                        Console.WriteLine(
                            $"Token valid. sub={sub}, username={username}, email={email}, token_use={tokenUse}, client_id={tokenClientId}");

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}