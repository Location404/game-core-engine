using GameCoreEngime.Domain.Entities;

namespace GameCoreEngime.Application.DTOs.Requests;

public record EndRoundRequest(
    Guid MatchId,
    int ResponseX,
    int ResponseY,
    Guid PlayerAId,
    int PlayerAGuessX,
    int PlayerAGuessY,
    Guid PlayerBId,
    int PlayerBGuessX,
    int PlayerBGuessY
)
{
    public Coordinate GetGameResponse() => new(ResponseX, ResponseY);
    public Coordinate GetPlayerAGuess() => new(PlayerAGuessX, PlayerAGuessY);
    public Coordinate GetPlayerBGuess() => new(PlayerBGuessX, PlayerBGuessY);
}