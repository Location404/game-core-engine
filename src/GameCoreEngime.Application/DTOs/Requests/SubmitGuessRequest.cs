using GameCoreEngime.Domain.Entities;

namespace GameCoreEngime.Application.DTOs.Requests;

public record SubmitGuessRequest(Guid MatchId, Guid PlayerId, int X, int Y)
{
    public Coordinate ToCoordinate() => new(X, Y);
}