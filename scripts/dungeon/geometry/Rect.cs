namespace MyShatteredPixelDungeon.scripts.dungeon.geometry;

/// <summary>
///     整数矩形，对应原版 com.watabou.utils.Rect
///     right/bottom 为独占边界（就像 C++ 的迭代器 end），width = right - left
///     Room 子类覆盖 Width/Height 为包含语义（+1）
/// </summary>
public class Rect : IEquatable<Rect>
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public Rect() { }

    public Rect(int left, int top, int right, int bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }

    public Rect(Rect other) : this(other.Left, other.Top, other.Right, other.Bottom) { }

    /// <summary>宽度（独占）</summary>
    public virtual int Width => Right - Left;

    /// <summary>高度（独占）</summary>
    public virtual int Height => Bottom - Top;

    /// <summary>面积</summary>
    public int Area => Width * Height;

    /// <summary>是否为空（right ≤ left 或 bottom ≤ top）</summary>
    public bool IsEmpty => Right <= Left || Bottom <= Top;

    public Rect Set(int left, int top, int right, int bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
        return this;
    }

    public Rect Set(Rect other) => Set(other.Left, other.Top, other.Right, other.Bottom);

    /// <summary>设置位置（保持宽高不变）</summary>
    public Rect SetPos(int x, int y)
    {
        int w = Right - Left, h = Bottom - Top;
        return Set(x, y, x + w, y + h);
    }

    /// <summary>偏移</summary>
    public Rect Shift(int dx, int dy) => Set(Left + dx, Top + dy, Right + dx, Bottom + dy);

    /// <summary>调整尺寸（w/h 为独占宽度）</summary>
    public Rect Resize(int w, int h) => Set(Left, Top, Left + w, Top + h);

    /// <summary>点是否在矩形内（独占 right/bottom）</summary>
    public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;

    public bool Contains(Point p) => Contains(p.X, p.Y);

    /// <summary>交集</summary>
    public Rect Intersect(Rect other) => new(
        Math.Max(Left, other.Left), Math.Max(Top, other.Top),
        Math.Min(Right, other.Right), Math.Min(Bottom, other.Bottom));

    public bool Intersects(Rect other) => !Intersect(other).IsEmpty;

    /// <summary>扩大包含点</summary>
    public Rect Union(int x, int y)
    {
        if (IsEmpty) return Set(x, y, x + 1, y + 1);
        if (x < Left) Left = x;
        else if (x >= Right) Right = x + 1;
        if (y < Top) Top = y;
        else if (y >= Bottom) Bottom = y + 1;
        return this;
    }

    public Rect Union(Point p) => Union(p.X, p.Y);

    /// <summary>内部缩进</summary>
    public Rect Shrink(int d) => new(Left + d, Top + d, Right - d, Bottom - d);

    public Rect Shrink() => Shrink(1);

    /// <summary>枚举所有点（含 right/bottom 边界，供 Room 连接逻辑使用）</summary>
    public IEnumerable<Point> GetPoints()
    {
        for (int y = Top; y <= Bottom; y++)
            for (int x = Left; x <= Right; x++)
                yield return new Point(x, y);
    }

    public override bool Equals(object? obj) => obj is Rect r && Equals(r);
    public bool Equals(Rect? other) =>
        other is not null && Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);
    public override string ToString() => $"[{Left},{Top} → {Right},{Bottom}]";
}