namespace GameCoreEngine.Infrastructure.Cache.Models;


internal class GameMatchCache
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid PlayerAId { get; set; }
    public Guid PlayerBId { get; set; }
    public Guid? PlayerWinnerId { get; set; }
    public Guid? PlayerLoserId { get; set; }
    public int? PlayerATotalPoints { get; set; }
    public int? PlayerBTotalPoints { get; set; }
    public int? PointsEarned { get; set; }
    public int? PointsLost { get; set; }
    public List<GameRoundCache>? GameRounds { get; set; }
    public GameRoundCache? CurrentGameRound { get; set; }
}