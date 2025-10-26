namespace Location404.Game.Application.Events;

using Location404.Game.Application.DTOs;
using Location404.Game.Domain.Entities;

public record GameMatchEndedEvent(
    Guid MatchId,
    Guid PlayerAId,
    Guid PlayerBId,
    Guid? WinnerId,
    Guid? LoserId,
    int? PlayerATotalPoints,
    int? PlayerBTotalPoints,
    int? PointsEarned,
    int? PointsLost,
    DateTime StartTime,
    DateTime EndTime,
    List<GameRoundDto> Rounds
)
{
    public static GameMatchEndedEvent FromGameMatch(GameMatch match)
    {
        if (match.GameRounds == null)
            throw new InvalidOperationException("Match must have rounds before creating event.");

        return new GameMatchEndedEvent(
            match.Id,
            match.PlayerAId,
            match.PlayerBId,
            match.PlayerWinnerId,
            match.PlayerLoserId,
            match.PlayerATotalPoints,
            match.PlayerBTotalPoints,
            match.PointsEarned,
            match.PointsLost,
            match.StartTime,
            match.EndTime,
            match.GameRounds.Select(GameRoundDto.FromEntity).ToList()
        );
    }
}