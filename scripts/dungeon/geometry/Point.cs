namespace MyShatteredPixelDungeon.scripts.dungeon.geometry;

/// <summary>
///     整数坐标点，对应原版 com.watabou.utils.Point
///     用于房间、门、地图位置等离散坐标表示
/// </summary>
public sealed class Point : IEquatable<Point>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point(Point other) : this(other.X, other.Y)
    {
    }

    /// <summary>
    ///     偏移坐标
    /// </summary>
    public void Offset(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }

    public override bool Equals(object? obj) => obj is Point p && Equals(p);

    public bool Equals(Point? other) => other is not null && X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";

    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

    public static bool operator ==(Point? a, Point? b) => Equals(a, b);

    public static bool operator !=(Point? a, Point? b) => !Equals(a, b);
}