using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TwinBlade.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddCognitoAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var region = configuration["Cognito:Region"];
        var userPoolId = configuration["Cognito:UserPoolId"];
        var clientId = configuration["Cognito:ClientId"];

        if (string.IsNullOrWhiteSpace(region) ||
            string.IsNullOrWhiteSpace(userPoolId) ||
            string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Missing Cognito configuration.");
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