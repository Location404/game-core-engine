using GameCoreEngine.Application.DTOs.Integration;

namespace GameCoreEngine.Application.Services;

public interface IGameDataClient
{
    Task<UserDto?> GetUserAsync(Guid userId);
    Task<bool> IsUserAvailableAsync(Guid userId);
    Task UpdateUserStatsAsync(UpdateUserStatsRequest request);
}