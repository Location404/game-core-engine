namespace GameCoreEngine.Application.DTOs.Responses;

public record MatchFoundResponse(
    Guid MatchId,
    Guid PlayerAId,
    Guid PlayerBId,
    DateTime StartTime
);