namespace GameCoreEngine.Infrastructure.Cache;

using GameCoreEngine.Application.Services;
using System.Collections.Concurrent;

/// <summary>
/// In-memory implementation of IPlayerConnectionManager for development without Redis
/// </summary>
public class InMemoryPlayerConnectionManager : IPlayerConnectionManager
{
    private readonly ConcurrentDictionary<Guid, string> _connections = new();

    public Task MapPlayerToConnectionAsync(Guid playerId, string connectionId)
    {
        _connections[playerId] = connectionId;
        return Task.CompletedTask;
    }

    public Task<string?> GetConnectionIdAsync(Guid playerId)
    {
        _connections.TryGetValue(playerId, out var connectionId);
        return Task.FromResult(connectionId);
    }

    public Task RemoveMappingAsync(Guid playerId)
    {
        _connections.TryRemove(playerId, out _);
        return Task.CompletedTask;
    }
}
