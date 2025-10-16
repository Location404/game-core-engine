namespace GameCoreEngime.Domain.Entities;

public class Coordinate(int x, int y)
{
    public int X { get; private set; } = x;
    public int Y { get; private set; } = y;

    public double CalculateDistance(Coordinate other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        
        return distance;
    }
}