namespace GameCoreEngine.Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using GameCoreEngine.Infrastructure.Configuration;
using GameCoreEngine.Application.Services;
using GameCoreEngine.Infrastructure.Cache;
using GameCoreEngine.Infrastructure.Matchmaking;
using GameCoreEngine.Infrastructure.Messaging;
using GameCoreEngine.Infrastructure.HttpClients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRedisServices(configuration);
        services.AddMatchmakingServices(configuration);
        services.AddMessagingServices(configuration);
        services.AddHttpClients(configuration);
        // services.AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));

        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>()
            ?? new RedisSettings();

        if (redisSettings.Enabled)
        {
            // Use real Redis implementations
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configurationOptions = ConfigurationOptions.Parse(redisSettings.ConnectionString);

                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectTimeout = 5000;
                configurationOptions.SyncTimeout = 5000;
                configurationOptions.AsyncTimeout = 5000;
                configurationOptions.ConnectRetry = 3;
                configurationOptions.KeepAlive = 60;

                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            services.AddSingleton<IGameMatchManager, RedisGameMatchManager>();
            services.AddSingleton<IPlayerConnectionManager, PlayerConnectionManager>();
            services.AddSingleton<IGuessStorageManager, GuessStorageManager>();
        }
        else
        {
            // Use in-memory implementations for development
            services.AddSingleton<IGameMatchManager, InMemoryGameMatchManager>();
            services.AddSingleton<IPlayerConnectionManager, InMemoryPlayerConnectionManager>();
            services.AddSingleton<IGuessStorageManager, InMemoryGuessStorageManager>();
        }

        return services;
    }

    private static IServiceCollection AddMatchmakingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>()
            ?? new RedisSettings();

        if (redisSettings.Enabled)
        {
            services.AddSingleton<IMatchmakingService, RedisMatchmakingService>();
        }
        else
        {
            services.AddSingleton<IMatchmakingService, InMemoryMatchmakingService>();
        }

        return services;
    }

    private static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

        var rabbitSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
            ?? new RabbitMQSettings();

        // Use mock if RabbitMQ is disabled or in development without RabbitMQ
        if (rabbitSettings.Enabled)
        {
            services.AddSingleton<IGameEventPublisher, RabbitMQEventPublisher>();
        }
        else
        {
            services.AddSingleton<IGameEventPublisher, MockGameEventPublisher>();
        }

        return services;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GameDataClientSettings>(configuration.GetSection("GameDataClient"));

        var gameDataSettings = configuration.GetSection("GameDataClient").Get<GameDataClientSettings>()
            ?? new GameDataClientSettings();

        services.AddHttpClient<IGameDataClient, GameDataHttpClient>(client =>
        {
            client.BaseAddress = new Uri(gameDataSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(gameDataSettings.TimeoutSeconds);
        });

        return services;
    }
}