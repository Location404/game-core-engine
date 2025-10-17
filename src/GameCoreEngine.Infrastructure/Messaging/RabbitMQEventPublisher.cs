namespace GameCoreEngine.Infrastructure.Messaging;

using GameCoreEngine.Application.Events;
using GameCoreEngine.Application.Services;
using GameCoreEngine.Infrastructure.Configuration;

using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMQEventPublisher : IGameEventPublisher, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQEventPublisher(RabbitMQSettings settings)
    {
        _settings = settings;

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        DeclareQueue(settings.MatchEndedQueue, "match.ended");
        DeclareQueue(settings.RoundEndedQueue, "round.ended");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
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
        try
        {
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

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to publish event to RabbitMQ: {ex.Message}", ex);
        }
    }

    private void DeclareQueue(string queueName, string routingKey)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        _channel.QueueBind(
            queue: queueName,
            exchange: _settings.ExchangeName,
            routingKey: routingKey
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}