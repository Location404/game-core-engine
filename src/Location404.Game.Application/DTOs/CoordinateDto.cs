using Location404.Game.Domain.Entities;


namespace Location404.Game.Application.DTOs;

public record CoordinateDto(double X, double Y)
{
    public static CoordinateDto FromEntity(Coordinate coordinate)
        => new(coordinate.X, coordinate.Y);

    public Coordinate ToEntity() => new(X, Y);
}
