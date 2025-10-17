using GameCoreEngime.Domain.Entities;

namespace GameCoreEngime.Application.DTOs;

public record CoordinateDto(int X, int Y)
{
    public static CoordinateDto FromEntity(Coordinate coordinate)
        => new(coordinate.X, coordinate.Y);

    public Coordinate ToEntity() => new(X, Y);
}
