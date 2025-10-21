using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.DTOs.Requests;

public record SubmitGuessRequest(Guid MatchId, Guid PlayerId, double X, double Y)
{
    public Coordinate ToCoordinate() => new(X, Y);
}