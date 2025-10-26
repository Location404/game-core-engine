namespace Location404.Game.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public byte[]? ProfileImage { get; private set; }

    public List<GameMatch> HistoryGameMatch { get; private set; } = [];
    public GameMatch? CurrentGameMatch { get; private set; }
    public int TotalPoints { get; private set; }

    public bool[] HistoryGameResults => SearchHistoryGameResults();
    public bool IsInGameMatch() => CurrentGameMatch != null;

    public User(Guid id, string username, string email, byte[]? profileImage = null, List<GameMatch>? historyGameMatch = null, GameMatch? currentGameMatch = null, int totalPoints = 0)
    {
        Id = id;
        Username = username;
        Email = email;
        ProfileImage = profileImage;
        HistoryGameMatch = historyGameMatch ?? [];
        CurrentGameMatch = currentGameMatch;
        TotalPoints = totalPoints;
    }

    public void StartGameMatch(GameMatch gameMatch)
    {
        if (IsInGameMatch())
            throw new InvalidOperationException("User is already in a game match.");

        CurrentGameMatch = gameMatch;
    }
    
    public bool[] SearchHistoryGameResults()
    {
        return [.. HistoryGameMatch
            .OrderByDescending(gm => gm.EndTime)
            .Take(3)
            .Select(gm => gm.PlayerWinnerId == Id)];
    }
 }