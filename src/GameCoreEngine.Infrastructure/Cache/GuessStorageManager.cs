namespace GameCoreEngine.Infrastructure.Cache;

using GameCoreEngine.Application.Services;
using GameCoreEngine.Domain.Entities;
using StackExchange.Redis;

public class GuessStorageManager : IGuessStorageManager
{
    private readonly IDatabase _db;

    public GuessStorageManager(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task StoreGuessAsync(Guid matchId, Guid roundId, Guid playerId, Coordinate guess)
    {
        var key = $"guess:{matchId}:{roundId}:{playerId}";
        var value = $"{guess.X},{guess.Y}";
        await _db.StringSetAsync(key, value, TimeSpan.FromMinutes(5));
    }

    public async Task<(Coordinate? playerA, Coordinate? playerB)> GetBothGuessesAsync(
        Guid matchId, Guid roundId, Guid playerAId, Guid playerBId)
    {
        var guessA = await _db.StringGetAsync($"guess:{matchId}:{roundId}:{playerAId}");
        var guessB = await _db.StringGetAsync($"guess:{matchId}:{roundId}:{playerBId}");

        Coordinate? coordA = null;
        Coordinate? coordB = null;

        if (guessA.HasValue)
        {
            var parts = guessA.ToString().Split(',');
            coordA = new Coordinate(double.Parse(parts[0]), double.Parse(parts[1]));
        }

        if (guessB.HasValue)
        {
            var parts = guessB.ToString().Split(',');
            coordB = new Coordinate(double.Parse(parts[0]), double.Parse(parts[1]));
        }

        return (coordA, coordB);
    }

    public async Task ClearGuessesAsync(Guid matchId, Guid roundId)
    {
        var pattern = $"guess:{matchId}:{roundId}:*";

        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();

        if (keys.Length > 0)
        {
            await _db.KeyDeleteAsync(keys);
        }

        // Also remove correct answer
        var answerKey = $"answer:{matchId}:{roundId}";
        await _db.KeyDeleteAsync(answerKey);
    }

    public async Task StoreCorrectAnswerAsync(Guid matchId, Guid roundId, Coordinate correctAnswer)
    {
        var key = $"answer:{matchId}:{roundId}";
        var value = $"{correctAnswer.X},{correctAnswer.Y}";
        await _db.StringSetAsync(key, value, TimeSpan.FromMinutes(5));
    }

    public async Task<Coordinate?> GetCorrectAnswerAsync(Guid matchId, Guid roundId)
    {
        var key = $"answer:{matchId}:{roundId}";
        var answer = await _db.StringGetAsync(key);

        if (!answer.HasValue)
            return null;

        var parts = answer.ToString().Split(',');
        return new Coordinate(double.Parse(parts[0]), double.Parse(parts[1]));
    }
}