using GameCoreEngine.Application.DTOs;
using GameCoreEngine.Application.Events;

namespace GameCoreEngine.Application.Services;

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
