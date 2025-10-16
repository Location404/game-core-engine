using Shared.Observability.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddOpenTelemetryObservability(builder.Configuration, options =>
{
    options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
});


var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();


app.UseCors();

app.Run();