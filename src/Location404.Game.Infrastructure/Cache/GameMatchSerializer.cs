namespace Location404.Game.Infrastructure.Cache;

using Location404.Game.Domain.Entities;
using Location404.Game.Infrastructure.Cache.Models;
using System.Text.Json;

internal static class GameMatchSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize(GameMatch match)
    {
    var cache = new GameMatchCache
        {
            Id = match.Id,
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            PlayerAId = match.PlayerAId,
            PlayerBId = match.PlayerBId,
            PlayerWinnerId = match.PlayerWinnerId,
            PlayerLoserId = match.PlayerLoserId,
            PlayerATotalPoints = match.PlayerATotalPoints,
            PlayerBTotalPoints = match.PlayerBTotalPoints,
            PointsEarned = match.PointsEarned,
            PointsLost = match.PointsLost,
            GameRounds = match.GameRounds?.Select(SerializeRound).ToList(),
            CurrentGameRound = match.CurrentGameRound != null ? SerializeRound(match.CurrentGameRound) : null
        };

        return JsonSerializer.Serialize(cache, JsonOptions);
    }

    public static GameMatch Deserialize(string json)
    {
        var cache = JsonSerializer.Deserialize<GameMatchCache>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize match");

        var match = GameMatch.StartGameMatch(cache.PlayerAId, cache.PlayerBId);
        
        SetProperty(match, nameof(GameMatch.Id), cache.Id);
        SetProperty(match, nameof(GameMatch.StartTime), cache.StartTime);
        SetProperty(match, nameof(GameMatch.EndTime), cache.EndTime);
        SetProperty(match, nameof(GameMatch.PlayerWinnerId), cache.PlayerWinnerId);
        SetProperty(match, nameof(GameMatch.PlayerLoserId), cache.PlayerLoserId);
        SetProperty(match, nameof(GameMatch.PlayerATotalPoints), cache.PlayerATotalPoints);
        SetProperty(match, nameof(GameMatch.PlayerBTotalPoints), cache.PlayerBTotalPoints);
        SetProperty(match, nameof(GameMatch.PointsEarned), cache.PointsEarned);
        SetProperty(match, nameof(GameMatch.PointsLost), cache.PointsLost);
        
        if (cache.GameRounds != null)
        {
            var rounds = cache.GameRounds.Select(DeserializeRound).ToList();
            SetProperty(match, nameof(GameMatch.GameRounds), rounds);
        }
        
        if (cache.CurrentGameRound != null)
        {
            var currentRound = DeserializeRound(cache.CurrentGameRound);
            SetProperty(match, nameof(GameMatch.CurrentGameRound), currentRound);
        }
        
        return match;
    }

    private static GameRoundCache SerializeRound(GameRound round)
    {
        return new GameRoundCache
        {
            Id = round.Id,
            GameMatchId = round.GameMatchId,
            RoundNumber = round.RoundNumber,
            PlayerAId = round.PlayerAId,
            PlayerBId = round.PlayerBId,
            PlayerAPoints = round.PlayerAPoints,
            PlayerBPoints = round.PlayerBPoints,
            GameResponse = round.GameResponse != null 
                ? new CoordinateCache { X = round.GameResponse.X, Y = round.GameResponse.Y } 
                : null,
            PlayerAGuess = round.PlayerAGuess != null 
                ? new CoordinateCache { X = round.PlayerAGuess.X, Y = round.PlayerAGuess.Y } 
                : null,
            PlayerBGuess = round.PlayerBGuess != null 
                ? new CoordinateCache { X = round.PlayerBGuess.X, Y = round.PlayerBGuess.Y } 
                : null,
            GameRoundEnded = round.GameRoundEnded
        };
    }

    private static GameRound DeserializeRound(GameRoundCache cache)
    {
        var round = GameRound.StartGameRound(
            cache.GameMatchId, 
            cache.RoundNumber, 
            cache.PlayerAId, 
            cache.PlayerBId
        );
        
        SetProperty(round, nameof(GameRound.Id), cache.Id);
        SetProperty(round, nameof(GameRound.PlayerAPoints), cache.PlayerAPoints);
        SetProperty(round, nameof(GameRound.PlayerBPoints), cache.PlayerBPoints);
        SetProperty(round, nameof(GameRound.GameRoundEnded), cache.GameRoundEnded);
        
        if (cache.GameResponse != null)
            SetProperty(round, nameof(GameRound.GameResponse), 
                new Coordinate(cache.GameResponse.X, cache.GameResponse.Y));
        
        if (cache.PlayerAGuess != null)
            SetProperty(round, nameof(GameRound.PlayerAGuess), 
                new Coordinate(cache.PlayerAGuess.X, cache.PlayerAGuess.Y));
        
        if (cache.PlayerBGuess != null)
            SetProperty(round, nameof(GameRound.PlayerBGuess), 
                new Coordinate(cache.PlayerBGuess.X, cache.PlayerBGuess.Y));
        
        return round;
    }

    private static void SetProperty<T>(object obj, string propertyName, T value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}
