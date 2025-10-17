namespace GameCoreEngine.Application.DTOs.Integration;

public record UpdateUserStatsRequest(
    Guid UserId,
    int PointsChange,
    bool IsWinner
);