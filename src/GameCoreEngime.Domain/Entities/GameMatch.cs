namespace GameCoreEngime.Domain.Entities;

public class GameMatch
{
    public Guid Id { get; private set; }
    public List<Guid> PlayersIds { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public Guid PlayerWinnerId { get; private set; }
    public Guid PlayerLoserId { get; private set; }

    public int PointsEarned { get; private set; }
    public int PointsLost { get; private set; }

    public List<GameRound>? GameRounds { get; private set; }

    private GameMatch() { }

    public GameMatch(Guid id, List<Guid> playersIds, DateTime startTime, DateTime endTime, Guid playerWinnerId, Guid playerLoserId, int pointsEarned, int pointsLost)
    {
        Id = id;
        PlayersIds = playersIds;
        StartTime = startTime;
        EndTime = endTime;
        PlayerWinnerId = playerWinnerId;
        PlayerLoserId = playerLoserId;
        PointsEarned = pointsEarned;
        PointsLost = pointsLost;
    }

    public static GameMatch StartGameMatch(List<Guid> playersIds)
    {
        var gameMatch = new GameMatch()
        {
            Id = Guid.NewGuid(),
            PlayersIds = playersIds,
            StartTime = DateTime.UtcNow
        };

        return gameMatch;
    }
    
    public void EndGameMatch(Guid playerWinnerId, Guid playerLoserId, int pointsEarned, int pointsLost)
    {
        PlayerWinnerId = playerWinnerId;
        PlayerLoserId = playerLoserId;
        PointsEarned = pointsEarned;
        PointsLost = pointsLost;
        EndTime = DateTime.UtcNow;
    }
}