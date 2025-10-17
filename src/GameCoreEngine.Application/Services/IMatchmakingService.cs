using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.Services;

public interface IMatchmakingService
{
    Task<Guid> JoinQueueAsync(Guid playerId);
    Task LeaveQueueAsync(Guid playerId);
    Task<GameMatch?> TryFindMatchAsync();
    Task<int> GetQueueSizeAsync();
    Task<bool> IsPlayerInQueueAsync(Guid playerId);
}