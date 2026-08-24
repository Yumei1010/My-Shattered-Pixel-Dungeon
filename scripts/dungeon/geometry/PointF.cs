namespace MyShatteredPixelDungeon.scripts.dungeon.geometry;

/// <summary>
///     浮点坐标点，对应原版 com.watabou.utils.PointF
///     用于房间中心计算和角度计算
/// </summary>
public struct PointF : IEquatable<PointF>
{
    public float X { get; set; }
    public float Y { get; set; }

    public PointF(float x, float y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(PointF other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is PointF p && Equals(p);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";

    public static PointF operator +(PointF a, PointF b) => new(a.X + b.X, a.Y + b.Y);

    public static bool operator ==(PointF a, PointF b) => a.Equals(b);

    public static bool operator !=(PointF a, PointF b) => !a.Equals(b);
}