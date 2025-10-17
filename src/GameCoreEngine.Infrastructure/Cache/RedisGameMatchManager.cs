using GameCoreEngine.Application.Services;
using GameCoreEngine.Domain.Entities;

using StackExchange.Redis;

namespace GameCoreEngine.Infrastructure.Cache;

public class RedisGameMatchManager(IConnectionMultiplexer redis) : IGameMatchManager
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly TimeSpan _matchExpiration = TimeSpan.FromHours(2);
    
    private const string MatchKeyPrefix = "match:";
    private const string PlayerMatchKeyPrefix = "player:match:";
    private const string ActiveMatchesKey = "matches:active";

    public async Task<GameMatch> CreateMatchAsync(Guid playerAId, Guid playerBId)
    {
        var match = GameMatch.StartGameMatch(playerAId, playerBId);
        
        await SaveMatchAsync(match);
        
        await _db.StringSetAsync(
            GetPlayerMatchKey(playerAId), 
            match.Id.ToString(), 
            _matchExpiration
        );
        
        await _db.StringSetAsync(
            GetPlayerMatchKey(playerBId), 
            match.Id.ToString(), 
            _matchExpiration
        );
        
        await _db.SetAddAsync(ActiveMatchesKey, match.Id.ToString());
        
        return match;
    }

    public async Task<GameMatch?> GetMatchAsync(Guid matchId)
    {
        var key = GetMatchKey(matchId);
        var json = await _db.StringGetAsync(key);
        
        if (json.IsNullOrEmpty)
            return null;
        
        return GameMatchSerializer.Deserialize(json!);
    }

    public async Task<GameMatch?> GetPlayerCurrentMatchAsync(Guid playerId)
    {
        var matchIdStr = await _db.StringGetAsync(GetPlayerMatchKey(playerId));
        
        if (matchIdStr.IsNullOrEmpty)
            return null;
        
        if (!Guid.TryParse(matchIdStr, out var matchId))
            return null;
        
        return await GetMatchAsync(matchId);
    }

    public async Task UpdateMatchAsync(GameMatch match)
    {
        await SaveMatchAsync(match);
    }

    public async Task RemoveMatchAsync(Guid matchId)
    {
        var match = await GetMatchAsync(matchId);
        if (match == null)
            return;
        
        await _db.KeyDeleteAsync(GetPlayerMatchKey(match.PlayerAId));
        await _db.KeyDeleteAsync(GetPlayerMatchKey(match.PlayerBId));
        await _db.KeyDeleteAsync(GetMatchKey(matchId));
        await _db.SetRemoveAsync(ActiveMatchesKey, matchId.ToString());
    }

    public async Task<bool> IsPlayerInMatchAsync(Guid playerId)
    {
        var matchId = await _db.StringGetAsync(GetPlayerMatchKey(playerId));
        return !matchId.IsNullOrEmpty;
    }

    public async Task<IEnumerable<Guid>> GetAllActiveMatchIdsAsync()
    {
        var matchIds = await _db.SetMembersAsync(ActiveMatchesKey);
        return matchIds
            .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value);
    }

    private async Task SaveMatchAsync(GameMatch match)
    {
        var key = GetMatchKey(match.Id);
        var json = GameMatchSerializer.Serialize(match);
        await _db.StringSetAsync(key, json, _matchExpiration);
    }

    private static string GetMatchKey(Guid matchId) 
        => $"{MatchKeyPrefix}{matchId}";

    private static string GetPlayerMatchKey(Guid playerId) 
        => $"{PlayerMatchKeyPrefix}{playerId}";
}