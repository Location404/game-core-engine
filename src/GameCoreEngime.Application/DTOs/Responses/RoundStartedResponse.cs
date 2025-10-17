namespace GameCoreEngime.Application.DTOs.Responses;

public record RoundStartedResponse(
    Guid MatchId,
    Guid RoundId,
    int RoundNumber,
    DateTime StartTime
);