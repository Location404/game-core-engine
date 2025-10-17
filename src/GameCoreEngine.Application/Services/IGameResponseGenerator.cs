using GameCoreEngine.Domain.Entities;

namespace GameCoreEngine.Application.Services;

public interface IGameResponseGenerator
{
    Task<Coordinate> GenerateRandomCoordinateAsync();
}