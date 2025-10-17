namespace GameCoreEngime.Domain.Entities;

public class Coordinate(int x, int y)
{
    public int X { get; private set; } = x;
    public int Y { get; private set; } = y;

    public double CalculateDistance(Coordinate other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    public override bool Equals(object? obj) => obj is Coordinate other && X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}