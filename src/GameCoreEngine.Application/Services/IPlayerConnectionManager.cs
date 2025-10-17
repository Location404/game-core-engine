namespace GameCoreEngine.Application.Services;

public interface IPlayerConnectionManager
{
    Task MapPlayerToConnectionAsync(Guid playerId, string connectionId);
    Task<string?> GetConnectionIdAsync(Guid playerId);
    Task RemoveMappingAsync(Guid playerId);
}