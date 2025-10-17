namespace GameCoreEngine.Domain.Entities;

public class GameRound
{
    public Guid Id { get; private set; }
    public Guid GameMatchId { get; private set; }
    public int RoundNumber { get; private set; }

    public Guid PlayerAId { get; private set; }
    public Guid PlayerBId { get; private set; }

    public int? PlayerAPoints { get; private set; }
    public int? PlayerBPoints { get; private set; }

    public Coordinate? GameResponse { get; private set; }

    public Coordinate? PlayerAGuess { get; private set; }
    public Coordinate? PlayerBGuess { get; private set; }

    public bool GameRoundEnded { get; private set; } = false;

    private GameRound() { }

    public static GameRound StartGameRound(Guid gameMatchId, int roundNumber, Guid playerAId, Guid playerBId)
    {
        var gameRound = new GameRound()
        {
            Id = Guid.NewGuid(),
            GameMatchId = gameMatchId,
            RoundNumber = roundNumber,
            PlayerAId = playerAId,
            PlayerBId = playerBId
        };

        return gameRound;
    }

    public void EndGameRound(Coordinate gameResponse, Coordinate playerAGuess, Coordinate playerBGuess)
    {
        GameResponse = gameResponse;
        PlayerAGuess = playerAGuess;
        PlayerBGuess = playerBGuess;

        PlayerAPoints = CalculatePoints(playerAGuess);
        PlayerBPoints = CalculatePoints(playerBGuess);
        GameRoundEnded = true;
    }

    private int CalculatePoints(Coordinate playerGuess)
    {
        if (GameResponse == null)
            throw new InvalidOperationException("Game response is not set.");

        var distance = GameResponse.CalculateDistance(playerGuess);
        if (distance == 0)
            return 10;
        else if (distance <= 1)
            return 5;
        else if (distance <= 2)
            return 2;
        else
            return 0;
    }

    public Guid? PlayerWinner()
    {
        if (PlayerAPoints == null || PlayerBPoints == null)
            throw new InvalidOperationException("Round has not ended or points are not calculated.");

        if (PlayerAPoints > PlayerBPoints)
            return PlayerAId;
        else if (PlayerBPoints > PlayerAPoints)
            return PlayerBId;
        else
            return null;
    }
}