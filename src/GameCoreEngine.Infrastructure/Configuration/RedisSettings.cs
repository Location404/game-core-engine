namespace GameCoreEngine.Infrastructure.Configuration;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "GameCoreEngine:";
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(2);
}