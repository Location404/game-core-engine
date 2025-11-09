using Location404.Game.API.Hubs;
using Location404.Game.Infrastructure.Extensions;
using Shared.Observability.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var redisConnection = builder.Configuration["Redis:ConnectionString"];
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options =>
    {
        options.Configuration.ChannelPrefix = "SignalR";
    });

builder.Services.AddOpenTelemetryObservability(builder.Configuration, options =>
{
    options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
});

builder.Services.AddObservabilityHealthChecks(builder.Configuration, checks =>
{
    var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        checks.AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready", "db" }, timeout: TimeSpan.FromSeconds(3));
    }

    var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ");
    var rabbitHost = rabbitMqSettings["HostName"];
    var rabbitPort = rabbitMqSettings.GetValue<int>("Port");
    var rabbitUser = rabbitMqSettings["UserName"];
    var rabbitPass = rabbitMqSettings["Password"];
    var rabbitVHost = rabbitMqSettings["VirtualHost"];

    if (!string.IsNullOrEmpty(rabbitHost))
    {
        var connectionString = $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:{rabbitPort}/{rabbitVHost}";
        checks.AddRabbitMQ(sp =>
        {
            var factory = new RabbitMQ.Client.ConnectionFactory();
            factory.Uri = new Uri(connectionString);
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        }, name: "rabbitmq", tags: new[] { "ready", "messaging" }, timeout: TimeSpan.FromSeconds(3));
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200"]
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// CORS must be configured before endpoints
app.UseCors("AllowFrontend");

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapOpenApi();
app.MapHub<GameHub>("/gamehub");
app.MapObservabilityHealthChecks();

app.Run();
