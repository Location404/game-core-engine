namespace GameCoreEngine.Infrastructure.Configuration;

public class GeoDataServiceSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public int TimeoutSeconds { get; set; } = 10;
}
