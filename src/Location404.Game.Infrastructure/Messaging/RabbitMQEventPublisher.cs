namespace Location404.Game.Infrastructure.Messaging;

using Location404.Game.Application.Events;
using Location404.Game.Application.Services;
using Location404.Game.Infrastructure.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

public class RabbitMQEventPublisher : IGameEventPublisher, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();
    private bool _isDisposed;

    public RabbitMQEventPublisher(IOptions<RabbitMQSettings> options, ILogger<RabbitMQEventPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _settings = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Don't connect in constructor - lazy connection on first publish
        _logger.LogInformation("RabbitMQEventPublisher initialized. Connection will be established on first event publish.");
    }

    private void EnsureConnection()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        lock (_lock)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            try
            {
                _logger.LogInformation("Connecting to RabbitMQ at {HostName}:{Port}", _settings.HostName, _settings.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
                };

                // Disable SSL for non-SSL RabbitMQ server
                factory.Ssl.Enabled = false;

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                );

                _logger.LogInformation("Successfully connected to RabbitMQ");
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "RabbitMQ broker is unreachable at {HostName}:{Port}. Check if RabbitMQ is running and accessible.",
                    _settings.HostName, _settings.Port);
                throw new InvalidOperationException($"Cannot connect to RabbitMQ at {_settings.HostName}:{_settings.Port}. Ensure RabbitMQ is running.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                throw;
            }
        }

    }

    public Task PublishMatchEndedAsync(GameMatchEndedEvent @event)
    {
        return PublishEventAsync("match.ended", @event);
    }

    public Task PublishRoundEndedAsync(GameRoundEndedEvent @event)
    {
        return PublishEventAsync("round.ended", @event);
    }

    private Task PublishEventAsync<T>(string routingKey, T @event)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                EnsureConnection();

                if (_channel == null || !_channel.IsOpen)
                {
                    throw new InvalidOperationException("RabbitMQ channel is not available");
                }

                var json = JsonSerializer.Serialize(@event, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.MessageId = Guid.NewGuid().ToString();

                _channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("Event published to RabbitMQ: {RoutingKey}, MessageId: {MessageId}",
                    routingKey, properties.MessageId);

                return Task.CompletedTask;
            }
            catch (AlreadyClosedException ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "RabbitMQ connection closed. Retry {RetryCount}/{MaxRetries}", retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to publish event after {MaxRetries} retries", maxRetries);
                    throw new InvalidOperationException($"Failed to publish event to RabbitMQ after {maxRetries} retries", ex);
                }

                Thread.Sleep(TimeSpan.FromSeconds(retryCount * 2));
                EnsureConnection();
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "RabbitMQ broker is unreachable. Event will be lost: {RoutingKey}", routingKey);
                throw new InvalidOperationException($"Cannot publish event - RabbitMQ is unreachable at {_settings.HostName}:{_settings.Port}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error publishing event to RabbitMQ: {RoutingKey}", routingKey);
                throw new InvalidOperationException($"Failed to publish event to RabbitMQ: {ex.Message}", ex);
            }
        }

        throw new InvalidOperationException($"Failed to publish event after {maxRetries} retries");
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        lock (_lock)
        {
            if (_isDisposed)
                return;

            try
            {
                _logger.LogInformation("Disposing RabbitMQ connection");

                if (_channel?.IsOpen == true)
                {
                    _channel.Close();
                }
                _channel?.Dispose();

                if (_connection?.IsOpen == true)
                {
                    _connection.Close();
                }
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ connection disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
            finally
            {
                _isDisposed = true;
            }
        }

        GC.SuppressFinalize(this);
    }
}