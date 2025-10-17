namespace GameCoreEngine.Infrastructure.HttpClients;

using GameCoreEngine.Application.Services;
using GameCoreEngine.Application.DTOs.Integration;
using System.Net.Http.Json;

public class GameDataHttpClient : IGameDataClient
{
    private readonly HttpClient _httpClient;

    public GameDataHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/users/{userId}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> IsUserAvailableAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/users/{userId}/availability");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task UpdateUserStatsAsync(UpdateUserStatsRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/users/stats/update",
                request
            );

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to update user stats: {ex.Message}", 
                ex
            );
        }
    }
}