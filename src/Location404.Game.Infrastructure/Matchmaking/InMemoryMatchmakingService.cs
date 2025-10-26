namespace Location404.Game.Infrastructure.Matchmaking;

using Location404.Game.Application.Services;
using Location404.Game.Domain.Entities;
using System.Collections.Concurrent;

/// <summary>
/// In-memory implementation of IMatchmakingService for development without Redis
/// </summary>
public class InMemoryMatchmakingService : IMatchmakingService
{
    private readonly IGameMatchManager _matchManager;
    private readonly SemaphoreSlim _matchmakingLock = new(1, 1);

    // Queue sorted by timestamp (playerId, timestamp)
    private readonly SortedSet<(Guid PlayerId, long Timestamp)> _queue = new(
        Comparer<(Guid, long)>.Create((a, b) => a.Item2.CompareTo(b.Item2))
    );

    // Set for fast lookup
    private readonly ConcurrentDictionary<Guid, long> _playerTimestamps = new();

    public InMemoryMatchmakingService(IGameMatchManager matchManager)
    {
        _matchManager = matchManager ?? throw new ArgumentNullException(nameof(matchManager));
    }

    public async Task<Guid> JoinQueueAsync(Guid playerId)
    {
        if (await _matchManager.IsPlayerInMatchAsync(playerId))
            throw new InvalidOperationException("Player is already in a match.");

        if (await IsPlayerInQueueAsync(playerId))
            throw new InvalidOperationException("Player is already in queue.");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _matchmakingLock.WaitAsync();
        try
        {
            _queue.Add((playerId, timestamp));
            _playerTimestamps[playerId] = timestamp;
        }
        finally
        {
            _matchmakingLock.Release();
        }

        return playerId;
    }

    public async Task LeaveQueueAsync(Guid playerId)
    {
        await _matchmakingLock.WaitAsync();
        try
        {
            if (_playerTimestamps.TryRemove(playerId, out var timestamp))
            {
                _queue.Remove((playerId, timestamp));
            }
        }
        finally
        {
            _matchmakingLock.Release();
        }
    }

    public async Task<GameMatch?> TryFindMatchAsync()
    {
        await _matchmakingLock.WaitAsync();

        try
        {
            if (_queue.Count < 2)
                return null;

            var players = _queue.Take(2).ToList();
            var playerAId = players[0].PlayerId;
            var playerBId = players[1].PlayerId;

            // Remove from queue
            _queue.Remove(players[0]);
            _queue.Remove(players[1]);
            _playerTimestamps.TryRemove(playerAId, out _);
            _playerTimestamps.TryRemove(playerBId, out _);

            var match = await _matchManager.CreateMatchAsync(playerAId, playerBId);

            return match;
        }
        finally
        {
            _matchmakingLock.Release();
        }
    }

    public Task<int> GetQueueSizeAsync()
    {
        return Task.FromResult(_queue.Count);
    }

    public Task<bool> IsPlayerInQueueAsync(Guid playerId)
    {
        return Task.FromResult(_playerTimestamps.ContainsKey(playerId));
    }
}
