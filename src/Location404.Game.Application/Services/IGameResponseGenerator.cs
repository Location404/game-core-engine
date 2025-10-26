using Location404.Game.Domain.Entities;

namespace Location404.Game.Application.Services;

public interface IGameResponseGenerator
{
    Task<Coordinate> GenerateRandomCoordinateAsync();
}