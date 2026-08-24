using MyShatteredPixelDungeon.scripts.dungeon.geometry;

namespace MyShatteredPixelDungeon.scripts.dungeon.painters;

/// <summary>
///     地图绘制工具，对应原版 com.shatteredpixel.shatteredpixeldungeon.levels.painters.Painter
///     提供填充矩形/线/圆等批量地形操作
/// </summary>
public static class Painter
{
    /// <summary>
    ///     设置单个格子的地形
    /// </summary>
    public static void Set(DungeonData data, int cell, int terrain)
    {
        if (cell >= 0 && cell < data.Length)
        {
            data.Map[cell] = terrain;
        }
    }

    /// <summary>
    ///     设置单个格子的地形（坐标版）
    /// </summary>
    public static void Set(DungeonData data, int x, int y, int terrain)
    {
        if (data.InsideMap(x, y))
        {
            data.Map[x + y * data.Width] = terrain;
        }
    }

    /// <summary>
    ///     设置单个格子的地形（点版）
    /// </summary>
    public static void Set(DungeonData data, Point p, int terrain) => Set(data, p.X, p.Y, terrain);

    /// <summary>
    ///     填充矩形区域
    /// </summary>
    public static void Fill(DungeonData data, Rect rect, int terrain)
    {
        for (int y = rect.Top; y <= rect.Bottom; y++)
        {
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                Set(data, x, y, terrain);
            }
        }
    }

    /// <summary>
    ///     填充矩形区域（向内缩 margin 格）
    /// </summary>
    public static void Fill(DungeonData data, Rect rect, int margin, int terrain)
    {
        Fill(data, new Rect(rect.Left + margin, rect.Top + margin, rect.Right - margin, rect.Bottom - margin), terrain);
    }

    /// <summary>
    ///     填充矩形内部区域（不含边框）
    /// </summary>
    public static void FillInside(DungeonData data, Rect rect, int terrain)
    {
        Fill(data, rect, 1, terrain);
    }

    /// <summary>
    ///     填充矩形边框（不含内部）
    /// </summary>
    public static void FillBorder(DungeonData data, Rect rect, int terrain)
    {
        // 上边和下边
        for (int x = rect.Left; x <= rect.Right; x++)
        {
            Set(data, x, rect.Top, terrain);
            Set(data, x, rect.Bottom, terrain);
        }
        // 左边和右边（跳过已绘的角）
        for (int y = rect.Top + 1; y < rect.Bottom; y++)
        {
            Set(data, rect.Left, y, terrain);
            Set(data, rect.Right, y, terrain);
        }
    }

    /// <summary>
    ///     在两点间绘制直线（用 terrain 填充）
    ///     from/to 为地图 cell 索引
    /// </summary>
    public static void DrawLine(DungeonData data, int from, int to, int terrain)
    {
        var p0 = data.CellToPoint(from);
        var p1 = data.CellToPoint(to);
        DrawLine(data, p0, p1, terrain);
    }

    /// <summary>
    ///     在两点间绘制直线（点版）
    /// </summary>
    public static void DrawLine(DungeonData data, Point from, Point to, int terrain)
    {
        // Bresenham 直线算法
        int x0 = from.X, y0 = from.Y;
        int x1 = to.X, y1 = to.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = -Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            Set(data, x0, y0, terrain);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    /// <summary>
    ///     填充矩形（从房间中心向内绘制 value 值，从外向内第 m 层）
    /// </summary>
    public static void DrawInside(DungeonData data, Rect rect, int margin, int terrain, int value)
    {
        // 内部矩形（缩 margin）
        var inner = new Rect(rect.Left + margin, rect.Top + margin, rect.Right - margin, rect.Bottom - margin);

        // 从外向内逐层填充（value 层）
        for (int i = 0; i < value; i++)
        {
            Fill(data, inner, i, terrain);
        }
    }

    /// <summary>
    ///     填充椭圆
    /// </summary>
    public static void FillEllipse(DungeonData data, Rect rect, int terrain)
    {
        // 使用中点椭圆算法
        int cx = rect.Left + rect.Width / 2;
        int cy = rect.Top + rect.Height / 2;
        int rx = rect.Width / 2;
        int ry = rect.Height / 2;

        if (rx < 1 || ry < 1)
        {
            Fill(data, rect, terrain);
            return;
        }

        for (int y = rect.Top; y <= rect.Bottom; y++)
        {
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                // 椭圆方程判断
                double dx = (double)(x - cx) / rx;
                double dy = (double)(y - cy) / ry;
                if (dx * dx + dy * dy <= 1.0)
                {
                    Set(data, x, y, terrain);
                }
            }
        }
    }

    /// <summary>
    ///     绘制圆环（空心圆）
    /// </summary>
    public static void DrawCircle(DungeonData data, Rect rect, int terrain)
    {
        int cx = rect.Left + rect.Width / 2;
        int cy = rect.Top + rect.Height / 2;
        int radius = Math.Max(rect.Width, rect.Height) / 2;

        if (radius < 1) return;

        for (int y = rect.Top; y <= rect.Bottom; y++)
        {
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                double dist = Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2));
                if (Math.Abs(dist - radius) < 0.6)
                {
                    Set(data, x, y, terrain);
                }
            }
        }
    }
}