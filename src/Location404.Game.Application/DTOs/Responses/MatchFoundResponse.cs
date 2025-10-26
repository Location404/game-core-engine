namespace Location404.Game.Application.DTOs.Responses;

public record MatchFoundResponse(
    Guid MatchId,
    Guid PlayerAId,
    Guid PlayerBId,
    DateTime StartTime
);