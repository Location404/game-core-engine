namespace GameCoreEngine.Infrastructure.Matchmaking;

using GameCoreEngine.Application.Services;
using GameCoreEngine.Domain.Entities;

using StackExchange.Redis;

public class RedisMatchmakingService(IConnectionMultiplexer redis, IGameMatchManager matchManager) : IMatchmakingService
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly IGameMatchManager _matchManager = matchManager;
    private readonly SemaphoreSlim _matchmakingLock = new(1, 1);
    
    private const string QueueKey = "matchmaking:queue";
    private const string PlayerQueueKey = "matchmaking:players";

    public async Task<Guid> JoinQueueAsync(Guid playerId)
    {
        if (await _matchManager.IsPlayerInMatchAsync(playerId))
            throw new InvalidOperationException("Player is already in a match.");

        if (await IsPlayerInQueueAsync(playerId))
            throw new InvalidOperationException("Player is already in queue.");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        await _db.SortedSetAddAsync(QueueKey, playerId.ToString(), timestamp);
        await _db.SetAddAsync(PlayerQueueKey, playerId.ToString());

        return playerId;
    }

    public async Task LeaveQueueAsync(Guid playerId)
    {
        await _db.SortedSetRemoveAsync(QueueKey, playerId.ToString());
        await _db.SetRemoveAsync(PlayerQueueKey, playerId.ToString());
    }

    public async Task<GameMatch?> TryFindMatchAsync()
    {
        await _matchmakingLock.WaitAsync();
        
        
        try
        {
            var players = await _db.SortedSetRangeByRankAsync(QueueKey, 0, 1);
            
            if (players.Length < 2)
                return null;

            var playerAId = Guid.Parse(players[0]!);
            var playerBId = Guid.Parse(players[1]!);

            await LeaveQueueAsync(playerAId);
            await LeaveQueueAsync(playerBId);

            var match = await _matchManager.CreateMatchAsync(playerAId, playerBId);
            
            return match;
        }
        finally
        {
            _matchmakingLock.Release();
        }
    }

    public async Task<int> GetQueueSizeAsync()
    {
        return (int)await _db.SortedSetLengthAsync(QueueKey);
    }

    public async Task<bool> IsPlayerInQueueAsync(Guid playerId)
    {
        return await _db.SetContainsAsync(PlayerQueueKey, playerId.ToString());
    }
}