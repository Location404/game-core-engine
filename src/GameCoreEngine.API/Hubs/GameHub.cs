namespace GameCoreEngine.API.Hubs;

using Microsoft.AspNetCore.SignalR;
using GameCoreEngine.Application.Services;
using GameCoreEngine.Application.DTOs.Requests;
using GameCoreEngine.Application.DTOs.Responses;
using GameCoreEngine.Application.Events;

public class GameHub : Hub
{
    private readonly IGameMatchManager _matchManager;
    private readonly IMatchmakingService _matchmaking;
    private readonly IGameEventPublisher _eventPublisher;

    public GameHub(
        IGameMatchManager matchManager,
        IMatchmakingService matchmaking,
        IGameEventPublisher eventPublisher)
    {
        _matchManager = matchManager;
        _matchmaking = matchmaking;
        _eventPublisher = eventPublisher;
    }

    public async Task<string> JoinMatchmaking(JoinMatchmakingRequest request)
    {
        try
        {
            await _matchmaking.JoinQueueAsync(request.PlayerId);
            
            var match = await _matchmaking.TryFindMatchAsync();
            
            if (match != null)
            {
                var response = new MatchFoundResponse(
                    match.Id,
                    match.PlayerAId,
                    match.PlayerBId,
                    match.StartTime
                );

                await Clients.Group(match.Id.ToString())
                    .SendAsync("MatchFound", response);

                return "Match found!";
            }

            return "Added to queue. Waiting for opponent...";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task LeaveMatchmaking(Guid playerId)
    {
        await _matchmaking.LeaveQueueAsync(playerId);
        await Clients.Caller.SendAsync("LeftQueue", "You have left the matchmaking queue.");
    }

    public async Task StartRound(StartRoundRequest request)
    {
        var match = await _matchManager.GetMatchAsync(request.MatchId);
        
        if (match == null)
        {
            await Clients.Caller.SendAsync("Error", "Match not found.");
            return;
        }

        if (!match.CanStartNewRound())
        {
            await Clients.Caller.SendAsync("Error", "Cannot start new round.");
            return;
        }

        match.StartNewGameRound();
        await _matchManager.UpdateMatchAsync(match);

        var response = new RoundStartedResponse(
            match.Id,
            match.CurrentGameRound!.Id,
            match.CurrentGameRound.RoundNumber,
            DateTime.UtcNow
        );

        await Clients.Group(match.Id.ToString())
            .SendAsync("RoundStarted", response);
    }

    public async Task SubmitGuess(SubmitGuessRequest request)
    {
        var match = await _matchManager.GetMatchAsync(request.MatchId);
        
        if (match == null)
        {
            await Clients.Caller.SendAsync("Error", "Match not found.");
            return;
        }

        if (match.CurrentGameRound == null)
        {
            await Clients.Caller.SendAsync("Error", "No active round.");
            return;
        }
        
        await Clients.Caller.SendAsync("GuessSubmitted", "Guess submitted successfully.");
    }

    public async Task EndRound(EndRoundRequest request)
    {
        var match = await _matchManager.GetMatchAsync(request.MatchId);
        
        if (match == null)
        {
            await Clients.Caller.SendAsync("Error", "Match not found.");
            return;
        }

        var gameResponse = request.GetGameResponse();
        var playerAGuess = request.GetPlayerAGuess();
        var playerBGuess = request.GetPlayerBGuess();

        match.EndCurrentGameRound(gameResponse, playerAGuess, playerBGuess);
        await _matchManager.UpdateMatchAsync(match);

        var roundEndedResponse = RoundEndedResponse.FromGameRound(
            match.GameRounds!.Last(),
            match.PlayerATotalPoints,
            match.PlayerBTotalPoints
        );

        await Clients.Group(match.Id.ToString()).SendAsync("RoundEnded", roundEndedResponse);

        if (match.GameRounds == null)
            throw new InvalidOperationException("Match must have rounds after ending a round.");

        var roundEvent = GameRoundEndedEvent.FromGameRound(match.GameRounds.Last());
        await _eventPublisher.PublishRoundEndedAsync(roundEvent);

        if (!match.CanStartNewRound())
        {
            match.EndGameMatch();
            await _matchManager.UpdateMatchAsync(match);

            var matchEndedResponse = MatchEndedResponse.FromGameMatch(match);
            
            await Clients.Group(match.Id.ToString())
                .SendAsync("MatchEnded", matchEndedResponse);

            var matchEvent = GameMatchEndedEvent.FromGameMatch(match);
            await _eventPublisher.PublishMatchEndedAsync(matchEvent);

            await _matchManager.RemoveMatchAsync(match.Id);
        }
    }

    public async Task GetMatchStatus(Guid matchId)
    {
        var match = await _matchManager.GetMatchAsync(matchId);
        
        if (match == null)
        {
            await Clients.Caller.SendAsync("Error", "Match not found.");
            return;
        }

        await Clients.Caller.SendAsync("MatchStatus", match);
    }

    public override async Task OnConnectedAsync()
    {
        var playerId = GetPlayerIdFromContext();
        
        if (playerId != Guid.Empty)
        {
            var match = await _matchManager.GetPlayerCurrentMatchAsync(playerId);
            
            if (match != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, match.Id.ToString());
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = GetPlayerIdFromContext();
        
        if (playerId != Guid.Empty)
        {
            await _matchmaking.LeaveQueueAsync(playerId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private Guid GetPlayerIdFromContext()
    {
        var playerIdClaim = Context.User?.FindFirst("PlayerId")?.Value;
        
        if (string.IsNullOrEmpty(playerIdClaim))
            return Guid.Empty;

        return Guid.TryParse(playerIdClaim, out var playerId) ? playerId : Guid.Empty;
    }
}