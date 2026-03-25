using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon.S3;
using Amazon;
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
using TwinBlade.Infrastructure.Cache.InMemory;
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
        services.Configure<CognitoOptions>(options =>
        {
            var section = configuration.GetSection(CognitoOptions.SectionName);
            section.Bind(options);

            options.UserPoolId = Environment.GetEnvironmentVariable("Cognito_UserPoolId")
                                ?? options.UserPoolId;
            options.ClientId = Environment.GetEnvironmentVariable("Cognito_ClientId")
                                ?? options.ClientId;
            options.Region = Environment.GetEnvironmentVariable("Region")
                                ?? Environment.GetEnvironmentVariable("AWS_REGION")
                                ?? options.Region;
            options.ClientSecret = Environment.GetEnvironmentVariable("Cognito_Secret")
                                ?? options.ClientSecret;
        });
        services.Configure<S3Options>(options =>
        {
            var section = configuration.GetSection(S3Options.SectionName);
            section.Bind(options);

            options.Region = Environment.GetEnvironmentVariable("Region")
                                ?? options.Region;
            options.S3_BucketName = Environment.GetEnvironmentVariable("S3_BucketName")
                                ?? options.S3_BucketName;
            options.S3_BaseUrl = Environment.GetEnvironmentVariable("S3_BaseUrl")
                                ?? options.S3_BaseUrl;
            options.S3_AvatarPathPrefix = Environment.GetEnvironmentVariable("S3_AvatarPathPrefix")
                                ?? options.S3_AvatarPathPrefix;
            options.ItemPathPrefix = Environment.GetEnvironmentVariable("S3_ItemPathPrefix")
                                ?? options.ItemPathPrefix;
        });

        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<CacheProviderOptions>(configuration.GetSection(CacheProviderOptions.SectionName));

        // AWS
        var accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var sessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        var region = Environment.GetEnvironmentVariable("AWS_REGION")
                     ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
                     ?? Environment.GetEnvironmentVariable("Region")
                     ?? configuration["AWS:Region"]
                     ?? configuration["S3:Region"];

        if (!string.IsNullOrWhiteSpace(accessKeyId) ^ !string.IsNullOrWhiteSpace(secretAccessKey))
        {
            throw new InvalidOperationException("Both AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY must be set together.");
        }

        var awsOptions = configuration.GetAWSOptions();

        if (!string.IsNullOrWhiteSpace(region))
        {
            awsOptions.Region = RegionEndpoint.GetBySystemName(region);
        }

        if (!string.IsNullOrWhiteSpace(accessKeyId) && !string.IsNullOrWhiteSpace(secretAccessKey))
        {
            awsOptions.Credentials = string.IsNullOrWhiteSpace(sessionToken)
                ? new BasicAWSCredentials(accessKeyId, secretAccessKey)
                : new SessionAWSCredentials(accessKeyId, secretAccessKey, sessionToken);
        }

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonCognitoIdentityProvider>();

        // Database (RDS PostgreSQL)
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var certPath = Path.Combine(AppContext.BaseDirectory, "global-bundle.pem");

        var connectionString =
            @$"Host={host};Port=5432;Database=postgres;Username=postgres;Password={password};SSL Mode=VerifyFull;Root Certificate={certPath}";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

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

        // Storage (S3)
        services.AddScoped<IAvatarStorageService, S3AvatarStorageService>();
        services.AddScoped<IAvatarService, AvatarService>();
        services.AddScoped<IAssetUrlService, S3AssetUrlService>();

        // Cache - Support both Redis and In-Memory
        var cacheProviderOptions = configuration.GetSection(CacheProviderOptions.SectionName).Get<CacheProviderOptions>();
        if (cacheProviderOptions?.UseInMemoryDebug == true || cacheProviderOptions?.Provider == "InMemory")
        {
            services.AddScoped<IRoomCacheService, InMemoryRoomCacheService>();
        }
        else
        {
            services.AddScoped<IRoomCacheService, RoomCacheService>();
        }

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
