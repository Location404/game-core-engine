using Location404.Game.Application.Events;

namespace Location404.Game.Application.Services;

public interface IGameEventPublisher
{
    Task PublishMatchEndedAsync(GameMatchEndedEvent @event);
    Task PublishRoundEndedAsync(GameRoundEndedEvent @event);
}