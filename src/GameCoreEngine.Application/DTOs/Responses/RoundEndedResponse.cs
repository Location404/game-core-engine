using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.DTOs.Responses;

public record RoundEndedResponse(
    Guid MatchId,
    Guid RoundId,
    int RoundNumber,
    CoordinateDto CorrectAnswer,
    CoordinateDto PlayerAGuess,
    CoordinateDto PlayerBGuess,
    int PlayerAPoints,
    int PlayerBPoints,
    int PlayerATotalPoints,
    int PlayerBTotalPoints,
    Guid? RoundWinnerId
)
{
    public static RoundEndedResponse FromGameRound(GameRound round, int playerATotalPoints, int playerBTotalPoints)
    {
        if (round.GameResponse == null || round.PlayerAGuess == null || round.PlayerBGuess == null)
            throw new InvalidOperationException("Round must be ended before creating response.");

        return new RoundEndedResponse(
            round.GameMatchId,
            round.Id,
            round.RoundNumber,
            CoordinateDto.FromEntity(round.GameResponse),
            CoordinateDto.FromEntity(round.PlayerAGuess),
            CoordinateDto.FromEntity(round.PlayerBGuess),
            round.PlayerAPoints ?? 0,
            round.PlayerBPoints ?? 0,
            playerATotalPoints,
            playerBTotalPoints,
            round.PlayerWinner()
        );
    }
}