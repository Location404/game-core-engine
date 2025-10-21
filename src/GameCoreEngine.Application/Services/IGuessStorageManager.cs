using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.Services;

public interface IGuessStorageManager
{
    Task StoreGuessAsync(Guid matchId, Guid roundId, Guid playerId, Coordinate guess);
    Task<(Coordinate? playerA, Coordinate? playerB)> GetBothGuessesAsync(Guid matchId, Guid roundId, Guid playerAId, Guid playerBId);
    Task ClearGuessesAsync(Guid matchId, Guid roundId);

    // Store and retrieve the correct answer for a round
    Task StoreCorrectAnswerAsync(Guid matchId, Guid roundId, Coordinate correctAnswer);
    Task<Coordinate?> GetCorrectAnswerAsync(Guid matchId, Guid roundId);
}