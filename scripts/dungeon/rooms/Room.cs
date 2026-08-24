using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.painters;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms;

/// <summary>
///     房间基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.levels.rooms.Room
///     继承自 Rect，带邻居/连接图，管理房间间的门和绘制
/// </summary>
public abstract class Room : Rect
{
    // 方向常量（避免与 Rect 的 Left/Top/Right/Bottom 属性冲突）
    public const int DirAll = 0;
    public const int DirLeft = 1;
    public const int DirTop = 2;
    public const int DirRight = 3;
    public const int DirBottom = 4;

    /// <summary>邻居房间列表（相邻但不一定连通）</summary>
    public List<Room> Neighbours { get; } = new();

    /// <summary>已连接房间 → 门类型</summary>
    public Dictionary<Room, Door> Connected { get; } = new();

    /// <summary>图距离（Builder 使用）</summary>
    public int GraphDistance { get; set; }

    /// <summary>连接代价（Builder 使用）</summary>
    public int Price { get; set; } = 1;

    protected Room() { }

    protected Room(Rect other) : base(other) { }

    /// <summary>宽度（包含 right 边界，= Rect.Width + 1）</summary>
    public override int Width => base.Width + 1;

    /// <summary>高度（包含 bottom 边界）</summary>
    public override int Height => base.Height + 1;

    // ---------- 尺寸约束 ----------

    public abstract int MinWidth();
    public abstract int MaxWidth();
    public abstract int MinHeight();
    public abstract int MaxHeight();

    public virtual bool SetSize()
    {
        return SetSize(MinWidth(), MaxWidth(), MinHeight(), MaxHeight());
    }

    public bool ForceSize(int w, int h)
    {
        return SetSize(w, w, h, h);
    }

    protected bool SetSize(int minW, int maxW, int minH, int maxH)
    {
        if (minW < MinWidth() || maxW > MaxWidth() || minH < MinHeight() || maxH > MaxHeight() || minW > maxW || minH > maxH)
            return false;
        // 减 1 因为房间的 right/bottom 是包含边界（Rect.Resize 是独占语义）
        Resize(System.Random.Shared.Next(minW, maxW + 1) - 1, System.Random.Shared.Next(minH, maxH + 1) - 1);
        return true;
    }

    // ---------- 空间逻辑 ----------

    public Point PointInside(Point from, int n)
    {
        var step = new Point(from);
        if (from.X == Left) step.Offset(n, 0);
        else if (from.X == Right) step.Offset(-n, 0);
        else if (from.Y == Top) step.Offset(0, n);
        else if (from.Y == Bottom) step.Offset(0, -n);
        return step;
    }

    public Point Random(int m = 1)
    {
        return new Point(System.Random.Shared.Next(Left + m, Right - m + 1), System.Random.Shared.Next(Top + m, Bottom - m + 1));
    }

    public new bool Contains(Point p) => p.X > Left && p.Y > Top && p.X < Right && p.Y < Bottom;

    public Point Center()
    {
        int cx = (Left + Right) / 2 + ((Right - Left) % 2 == 1 ? System.Random.Shared.Next(2) : 0);
        int cy = (Top + Bottom) / 2 + ((Bottom - Top) % 2 == 1 ? System.Random.Shared.Next(2) : 0);
        return new Point(cx, cy);
    }

    // ---------- 连接逻辑 ----------

    public virtual int MinConnections(int direction) => direction == DirAll ? 1 : 0;

    public virtual int MaxConnections(int direction) => direction == DirAll ? 16 : 4;

    public int CurConnections(int direction)
    {
        if (direction == DirAll) return Connected.Count;
        int total = 0;
        foreach (var (room, _) in Connected)
        {
            var i = Intersect(room);
            if (i is null) continue;
            if (direction == DirLeft && i.Width == 0 && i.Left == Left) total++;
            else if (direction == DirTop && i.Height == 0 && i.Top == Top) total++;
            else if (direction == DirRight && i.Width == 0 && i.Right == Right) total++;
            else if (direction == DirBottom && i.Height == 0 && i.Bottom == Bottom) total++;
        }
        return total;
    }

    public int RemConnections(int direction)
    {
        if (CurConnections(DirAll) >= MaxConnections(DirAll)) return 0;
        return MaxConnections(direction) - CurConnections(direction);
    }

    public virtual bool CanConnect(Point p) => (p.X == Left || p.X == Right) != (p.Y == Top || p.Y == Bottom);

    public virtual bool CanConnect(int direction) => RemConnections(direction) > 0;

    public virtual bool CanConnect(Room other)
    {
        if (IsExit() && other.IsEntrance() || IsEntrance() && other.IsExit())
            return false;

        var i = Intersect(other);
        if (i is null) return false;

        bool foundPoint = false;
        foreach (var p in i.GetPoints())
        {
            if (CanConnect(p) && other.CanConnect(p))
            {
                foundPoint = true;
                break;
            }
        }
        if (!foundPoint) return false;

        if (i.Width == 0 && i.Left == Left) return CanConnect(DirLeft) && other.CanConnect(DirRight);
        if (i.Height == 0 && i.Top == Top) return CanConnect(DirTop) && other.CanConnect(DirBottom);
        if (i.Width == 0 && i.Right == Right) return CanConnect(DirRight) && other.CanConnect(DirLeft);
        if (i.Height == 0 && i.Bottom == Bottom) return CanConnect(DirBottom) && other.CanConnect(DirTop);
        return false;
    }

    public bool AddNeighbour(Room other)
    {
        if (Neighbours.Contains(other)) return true;
        var i = Intersect(other);
        if ((i is not null && i.Width == 0 && i.Height >= 2) || (i is not null && i.Height == 0 && i.Width >= 2))
        {
            Neighbours.Add(other);
            other.Neighbours.Add(this);
            return true;
        }
        return false;
    }

    public bool Connect(Room other)
    {
        if ((Neighbours.Contains(other) || AddNeighbour(other)) && !Connected.ContainsKey(other) && CanConnect(other))
        {
            Connected[other] = new Door();
            other.Connected[this] = new Door();
            return true;
        }
        return false;
    }

    public void ClearConnections()
    {
        foreach (var r in Neighbours) r.Neighbours.Remove(this);
        Neighbours.Clear();
        foreach (var r in Connected.Keys) r.Connected.Remove(this);
        Connected.Clear();
    }

    public virtual bool IsEntrance() => false;
    public virtual bool IsExit() => false;

    // ---------- 绘制 ----------

    public abstract void Paint(DungeonData data);

    public virtual bool CanPlaceWater(Point p) => true;
    public virtual bool CanPlaceGrass(Point p) => true;
    public virtual bool CanPlaceTrap(Point p) => true;
    public virtual bool CanPlaceItem(Point p, DungeonData data) => Contains(p);
    public virtual bool CanPlaceCharacter(Point p, DungeonData data) => Contains(p);
}