using Location404.Game.Domain.Entities;

namespace Location404.Game.Application.DTOs;

public record GameRoundDto(
    Guid Id,
    Guid GameMatchId,
    int RoundNumber,
    Guid PlayerAId,
    Guid PlayerBId,
    int? PlayerAPoints,
    int? PlayerBPoints,
    CoordinateDto? GameResponse,
    CoordinateDto? PlayerAGuess,
    CoordinateDto? PlayerBGuess,
    bool GameRoundEnded
)
{
    public static GameRoundDto FromEntity(GameRound round)
        => new(
            round.Id,
            round.GameMatchId,
            round.RoundNumber,
            round.PlayerAId,
            round.PlayerBId,
            round.PlayerAPoints,
            round.PlayerBPoints,
            round.GameResponse != null ? CoordinateDto.FromEntity(round.GameResponse) : null,
            round.PlayerAGuess != null ? CoordinateDto.FromEntity(round.PlayerAGuess) : null,
            round.PlayerBGuess != null ? CoordinateDto.FromEntity(round.PlayerBGuess) : null,
            round.GameRoundEnded
        );
}
