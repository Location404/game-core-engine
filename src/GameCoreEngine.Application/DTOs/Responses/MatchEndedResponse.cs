using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.DTOs.Responses;

public record MatchEndedResponse(
    Guid MatchId,
    Guid? WinnerId,
    Guid? LoserId,
    int? PlayerATotalPoints,
    int? PlayerBTotalPoints,
    int? PointsEarned,
    int? PointsLost,
    DateTime EndTime,
    List<GameRoundDto> Rounds
)
{
    public static MatchEndedResponse FromGameMatch(GameMatch match)
    {
        return match.GameRounds == null
            ? throw new InvalidOperationException("Match must have rounds before creating response.")
            : new MatchEndedResponse(
            match.Id,
            match.PlayerWinnerId,
            match.PlayerLoserId,
            match.PlayerATotalPoints,
            match.PlayerBTotalPoints,
            match.PointsEarned,
            match.PointsLost,
            match.EndTime,
            match.GameRounds.Select(GameRoundDto.FromEntity).ToList()
        );
    }
}