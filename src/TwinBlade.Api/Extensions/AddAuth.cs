using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TwinBlade.Infrastructure.Options;

namespace TwinBlade.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddCognitoAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cognitoOptions = configuration
            .GetSection(CognitoOptions.SectionName)
            .Get<CognitoOptions>();

        var region = Environment.GetEnvironmentVariable("Region") ?? cognitoOptions?.Region;
        var userPoolId = Environment.GetEnvironmentVariable("Cognito_UserPoolId") ?? cognitoOptions?.UserPoolId;
        var clientId = Environment.GetEnvironmentVariable("Cognito_ClientId") ?? cognitoOptions?.ClientId;

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

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,

                    ValidateAudience = true,
                    ValidAudience = clientId,

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    NameClaimType = "cognito:username",
                    RoleClaimType = "cognito:groups"
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Auth failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var sub = context.Principal?.FindFirst("sub")?.Value;
                        var username = context.Principal?.FindFirst("cognito:username")?.Value;
                        var email = context.Principal?.FindFirst("email")?.Value;

                        Console.WriteLine($"Token valid. sub={sub}, username={username}, email={email}");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}