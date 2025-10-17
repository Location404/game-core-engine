using GameCoreEngine.Application.Events;

namespace GameCoreEngine.Application.Services;

public interface IGameEventPublisher
{
    Task PublishMatchEndedAsync(GameMatchEndedEvent @event);
    Task PublishRoundEndedAsync(GameRoundEndedEvent @event);
}