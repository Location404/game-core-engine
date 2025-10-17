namespace GameCoreEngime.Application.DTOs.Responses;

public record MatchFoundResponse(
    Guid MatchId,
    Guid PlayerAId,
    Guid PlayerBId,
    DateTime StartTime
);