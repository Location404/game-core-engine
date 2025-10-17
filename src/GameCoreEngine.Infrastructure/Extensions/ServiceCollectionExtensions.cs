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
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRedisServices(configuration);
        services.AddMatchmakingServices();
        services.AddMessagingServices(configuration);
        services.AddHttpClients(configuration);
        // services.AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddRedisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>()
            ?? new RedisSettings();

        services.AddSingleton(redisSettings);

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

        return services;
    }

    private static IServiceCollection AddMatchmakingServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IMatchmakingService, RedisMatchmakingService>();
        return services;
    }

    private static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
            ?? new RabbitMQSettings();

        services.AddSingleton(rabbitSettings);
        services.AddSingleton<IGameEventPublisher, RabbitMQEventPublisher>();

        return services;
    }

    private static IServiceCollection AddHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var gameDataSettings = configuration.GetSection("GameDataClient").Get<GameDataClientSettings>()
            ?? new GameDataClientSettings();

        services.AddSingleton(gameDataSettings);

        services.AddHttpClient<IGameDataClient, GameDataHttpClient>(client =>
        {
            client.BaseAddress = new Uri(gameDataSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(gameDataSettings.TimeoutSeconds);
        });

        return services;
    }

    // private static IServiceCollection AddApplicationServices(
    //     this IServiceCollection services)
    // {
    //     services.AddSingleton<IGameResponseGenerator, RandomGameResponseGenerator>();
    //     return services;
    // }
}