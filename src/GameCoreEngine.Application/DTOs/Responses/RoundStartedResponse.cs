namespace GameCoreEngine.Application.DTOs.Responses;

public record RoundStartedResponse(
    Guid MatchId,
    Guid RoundId,
    int RoundNumber,
    DateTime StartTime,
    LocationData Location
);

/// <summary>
/// Location data for Street View
/// X = Latitude, Y = Longitude (consistent with Coordinate system)
/// </summary>
public record LocationData(
    double X,  // Latitude
    double Y,  // Longitude
    double Heading,
    double Pitch
)
{
    /// <summary>
    /// Converts LocationData to Coordinate (ignoring heading and pitch)
    /// </summary>
    public GameCoreEngine.Domain.Entities.Coordinate ToCoordinate()
    {
        return new GameCoreEngine.Domain.Entities.Coordinate(X, Y);
    }
};