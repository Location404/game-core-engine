namespace GameCoreEngine.Infrastructure.Cache.Models;

internal class GameRoundCache
{
    public Guid Id { get; set; }
    public Guid GameMatchId { get; set; }
    public int RoundNumber { get; set; }
    public Guid PlayerAId { get; set; }
    public Guid PlayerBId { get; set; }
    public int? PlayerAPoints { get; set; }
    public int? PlayerBPoints { get; set; }
    public CoordinateCache? GameResponse { get; set; }
    public CoordinateCache? PlayerAGuess { get; set; }
    public CoordinateCache? PlayerBGuess { get; set; }
    public bool GameRoundEnded { get; set; }
}