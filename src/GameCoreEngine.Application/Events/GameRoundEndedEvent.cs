namespace GameCoreEngine.Application.Events;

using GameCoreEngine.Application.DTOs;
using GameCoreEngine.Domain.Entities;

public record GameRoundEndedEvent(
    Guid MatchId,
    Guid RoundId,
    int RoundNumber,
    Guid PlayerAId,
    Guid PlayerBId,
    CoordinateDto GameResponse,
    CoordinateDto PlayerAGuess,
    CoordinateDto PlayerBGuess,
    int PlayerAPoints,
    int PlayerBPoints,
    Guid? WinnerId,
    DateTime EndTime
)
{
    public static GameRoundEndedEvent FromGameRound(GameRound round)
    {
        return round.GameResponse == null || round.PlayerAGuess == null || round.PlayerBGuess == null
            ? throw new InvalidOperationException("Round must be ended before creating event.")
            : new GameRoundEndedEvent(
            round.GameMatchId,
            round.Id,
            round.RoundNumber,
            round.PlayerAId,
            round.PlayerBId,
            CoordinateDto.FromEntity(round.GameResponse),
            CoordinateDto.FromEntity(round.PlayerAGuess),
            CoordinateDto.FromEntity(round.PlayerBGuess),
            round.PlayerAPoints ?? 0,
            round.PlayerBPoints ?? 0,
            round.PlayerWinner(),
            DateTime.UtcNow
        );
    }
}