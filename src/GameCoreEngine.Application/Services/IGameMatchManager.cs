using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.Services;

public interface IGameMatchManager
{
    Task<GameMatch> CreateMatchAsync(Guid playerAId, Guid playerBId);
    Task<GameMatch?> GetMatchAsync(Guid matchId);
    Task<GameMatch?> GetPlayerCurrentMatchAsync(Guid playerId);
    Task UpdateMatchAsync(GameMatch match);
    Task RemoveMatchAsync(Guid matchId);
    Task<bool> IsPlayerInMatchAsync(Guid playerId);
    Task<IEnumerable<Guid>> GetAllActiveMatchIdsAsync();
}