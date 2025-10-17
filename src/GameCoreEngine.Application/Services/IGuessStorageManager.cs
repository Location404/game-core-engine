using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.Services;

public interface IGuessStorageManager
{
    Task StoreGuessAsync(Guid matchId, Guid roundId, Guid playerId, Coordinate guess);
    Task<(Coordinate? playerA, Coordinate? playerB)> GetBothGuessesAsync(Guid matchId, Guid roundId, Guid playerAId, Guid playerBId);
    Task ClearGuessesAsync(Guid matchId, Guid roundId);
}