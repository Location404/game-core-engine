using GameCoreEngime.Domain.Entities;

namespace GameCoreEngime.Application.DTOs;

public record GameMatchDto(
    Guid Id,
    DateTime StartTime,
    DateTime EndTime,
    Guid PlayerAId,
    Guid PlayerBId,
    Guid? PlayerWinnerId,
    Guid? PlayerLoserId,
    int? PlayerATotalPoints,
    int? PlayerBTotalPoints,
    int? PointsEarned,
    int? PointsLost,
    List<GameRoundDto>? GameRounds,
    GameRoundDto? CurrentGameRound,
    int TotalRounds
)
{
    public static GameMatchDto FromEntity(GameMatch match)
        => new(
            match.Id,
            match.StartTime,
            match.EndTime,
            match.PlayerAId,
            match.PlayerBId,
            match.PlayerWinnerId,
            match.PlayerLoserId,
            match.PlayerATotalPoints,
            match.PlayerBTotalPoints,
            match.PointsEarned,
            match.PointsLost,
            match.GameRounds?.Select(GameRoundDto.FromEntity).ToList(),
            match.CurrentGameRound != null ? GameRoundDto.FromEntity(match.CurrentGameRound) : null,
            match.TotalRounds
        );
}
