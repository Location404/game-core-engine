namespace GameCoreEngine.Infrastructure.Configuration;

public class GameDataClientSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5001";
    public int TimeoutSeconds { get; set; } = 30;
}