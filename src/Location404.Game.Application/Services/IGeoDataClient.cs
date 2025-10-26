using Location404.Game.Application.DTOs;
using Location404.Game.Application.Events;

namespace Location404.Game.Application.Services;

public interface IGeoDataClient
{
    Task<LocationDto?> GetRandomLocationAsync(CancellationToken cancellationToken = default);
    Task<bool> SendMatchEndedAsync(GameMatchEndedEvent matchEvent, CancellationToken cancellationToken = default);
}

public record LocationDto(
    Guid Id,
    CoordinateDto Coordinate,
    string Name,
    string Country,
    string Region,
    int? Heading,
    int? Pitch
);
