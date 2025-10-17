using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application;

public interface IGameResponseGenerator
{
    Task<Coordinate> GenerateRandomCoordinateAsync();
}