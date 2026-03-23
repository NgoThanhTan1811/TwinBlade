using Amazon.CognitoIdentityProvider;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Abstractions.Storage;
using TwinBlade.Infrastructure.Auth.Cognito;
using TwinBlade.Infrastructure.Cache.Redis;
using TwinBlade.Infrastructure.Options;
using TwinBlade.Infrastructure.Persistence.Rds;
using TwinBlade.Infrastructure.Persistence.Rds.Repositories;
using TwinBlade.Infrastructure.Storage.S3;


namespace TwinBlade.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<CognitoOptions>(configuration.GetSection(CognitoOptions.SectionName));
        services.Configure<S3Options>(configuration.GetSection(S3Options.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        // AWS
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonCognitoIdentityProvider>();

        // Database (RDS PostgreSQL)
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Redis (Elastic Cache)
        var cacheSection = configuration.GetSection("Cache");
        var redisEndpoint = Environment.GetEnvironmentVariable("CacheEndpoint");
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { redisEndpoint! },
            Ssl = bool.TryParse(cacheSection["Ssl"], out var ssl) && ssl,
            AbortOnConnectFail = false,
            ConnectTimeout = int.TryParse(cacheSection["ConnectTimeoutMs"], out var ct) ? ct : 5000,
            SyncTimeout = int.TryParse(cacheSection["SyncTimeoutMs"], out var st) ? st : 5000,
        };

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configOptions));

        // Auth
        services.AddScoped<ICognitoAuthService, CognitoAuthService>();

        // Storage (S3 — uploads done manually via AWS Console)
        services.AddScoped<IAvatarStorageService, S3AvatarStorageService>();
        services.AddScoped<IAvatarService, AvatarService>();
        services.AddScoped<IAssetUrlService, S3AssetUrlService>();

        // Cache
        services.AddScoped<IRoomCacheService, RoomCacheService>();

        // Repositories
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IMatchResultRepository, MatchResultRepository>();

        // Realtime state storage
        services.AddScoped<IRoomStateService, RoomStateService>();

        return services;
    }
}
