namespace GameCoreEngine.Infrastructure.Messaging;

using GameCoreEngine.Application.Events;
using GameCoreEngine.Application.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Mock implementation of IGameEventPublisher for development without RabbitMQ
/// Logs events instead of publishing to message broker
/// </summary>
public class MockGameEventPublisher : IGameEventPublisher
{
    private readonly ILogger<MockGameEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MockGameEventPublisher(ILogger<MockGameEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public Task PublishMatchEndedAsync(GameMatchEndedEvent @event)
    {
        var json = JsonSerializer.Serialize(@event, _jsonOptions);

        _logger.LogInformation(
            "[MOCK] Match Ended Event - MatchId: {MatchId}\n{EventJson}",
            @event.MatchId,
            json
        );

        return Task.CompletedTask;
    }

    public Task PublishRoundEndedAsync(GameRoundEndedEvent @event)
    {
        var json = JsonSerializer.Serialize(@event, _jsonOptions);

        _logger.LogInformation(
            "[MOCK] Round Ended Event - MatchId: {MatchId}, Round: {RoundNumber}\n{EventJson}",
            @event.MatchId,
            @event.RoundNumber,
            json
        );

        return Task.CompletedTask;
    }
}
