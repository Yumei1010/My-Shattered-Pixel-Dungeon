using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;

namespace MyShatteredPixelDungeon.scripts.dungeon.builders;

/// <summary>
///     Builder 抽象基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.levels.builders.Builder
///     接收房间列表，返回连接好的房间图（失败返回 null）
/// </summary>
public abstract class Builder
{
    /// <summary>
    ///     构建房间连接图
    /// </summary>
    public abstract List<Room>? Build(List<Room> rooms);

    /// <summary>
    ///     计算所有房间的邻居关系（两两检查相邻）
    /// </summary>
    protected static void FindNeighbours(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                rooms[i].AddNeighbour(rooms[j]);
            }
        }
    }

    /// <summary>
    ///     返回从指定起点出发的最大自由空间矩形
    /// </summary>
    protected static Rect FindFreeSpace(Point start, List<Room> collision, int maxSize)
    {
        var space = new Rect(start.X - maxSize, start.Y - maxSize, start.X + maxSize, start.Y + maxSize);
        var colliding = new List<Room>(collision);

        do
        {
            // 移除空房间和不重叠的房间
            for (int i = colliding.Count - 1; i >= 0; i--)
            {
                var room = colliding[i];
                if (room.IsEmpty
                    || Math.Max(space.Left, room.Left) >= Math.Min(space.Right, room.Right)
                    || Math.Max(space.Top, room.Top) >= Math.Min(space.Bottom, room.Bottom))
                {
                    colliding.RemoveAt(i);
                }
            }

            // 找到最接近的房间
            Room? closestRoom = null;
            int closestDiff = int.MaxValue;
            foreach (var curRoom in colliding)
            {
                bool inside = true;
                int curDiff = 0;

                if (start.X <= curRoom.Left) { inside = false; curDiff += curRoom.Left - start.X; }
                else if (start.X >= curRoom.Right) { inside = false; curDiff += start.X - curRoom.Right; }
                if (start.Y <= curRoom.Top) { inside = false; curDiff += curRoom.Top - start.Y; }
                else if (start.Y >= curRoom.Bottom) { inside = false; curDiff += start.Y - curRoom.Bottom; }

                if (inside)
                {
                    space.Set(start.X, start.Y, start.X, start.Y);
                    return space;
                }

                if (curDiff < closestDiff)
                {
                    closestDiff = curDiff;
                    closestRoom = curRoom;
                }
            }

            int wDiff, hDiff;
            if (closestRoom != null)
            {
                wDiff = int.MaxValue;
                if (closestRoom.Left >= start.X) wDiff = (space.Right - closestRoom.Left) * (space.Height + 1);
                else if (closestRoom.Right <= start.X) wDiff = (closestRoom.Right - space.Left) * (space.Height + 1);

                hDiff = int.MaxValue;
                if (closestRoom.Top >= start.Y) hDiff = (space.Bottom - closestRoom.Top) * (space.Width + 1);
                else if (closestRoom.Bottom <= start.Y) hDiff = (closestRoom.Bottom - space.Top) * (space.Width + 1);

                // 尽可能减少空间以解决碰撞
                if (wDiff < hDiff || (wDiff == hDiff && DeterministicRng.Range(2) == 0))
                {
                    if (closestRoom.Left >= start.X && closestRoom.Left < space.Right) space.Right = closestRoom.Left;
                    if (closestRoom.Right <= start.X && closestRoom.Right > space.Left) space.Left = closestRoom.Right;
                }
                else
                {
                    if (closestRoom.Top >= start.Y && closestRoom.Top < space.Bottom) space.Bottom = closestRoom.Top;
                    if (closestRoom.Bottom <= start.Y && closestRoom.Bottom > space.Top) space.Top = closestRoom.Bottom;
                }
                colliding.Remove(closestRoom);
            }
            else
            {
                colliding.Clear();
            }
        } while (colliding.Count > 0);

        return space;
    }

    private const double A = 180 / Math.PI;

    /// <summary>
    ///     两个房间中心点之间的角度（0 = 正上方，顺时针）
    /// </summary>
    protected static float AngleBetweenRooms(Room from, Room to)
    {
        var fromCenter = new PointF((from.Left + from.Right) / 2f, (from.Top + from.Bottom) / 2f);
        var toCenter = new PointF((to.Left + to.Right) / 2f, (to.Top + to.Bottom) / 2f);
        return AngleBetweenPoints(fromCenter, toCenter);
    }

    protected static float AngleBetweenPoints(PointF from, PointF to)
    {
        double m = (to.Y - from.Y) / (to.X - from.X);
        float angle = (float)(A * (Math.Atan(m) + Math.PI / 2.0));
        if (from.X > to.X) angle -= 180f;
        return angle;
    }

    /// <summary>
    ///     尝试放置房间，使 prev 中心到 next 的角度尽量接近指定角度。
    ///     返回两房间中心间的实际角度，失败返回 -1
    /// </summary>
    protected static float PlaceRoom(List<Room> collision, Room prev, Room next, float angle)
    {
        // 角度规范化到 [0, 360)
        angle %= 360f;
        if (angle < 0) angle += 360f;

        var prevCenter = new PointF((prev.Left + prev.Right) / 2f, (prev.Top + prev.Bottom) / 2f);

        // y = mx + b 直线公式
        double m = Math.Tan(angle / A + Math.PI / 2.0);
        double b = prevCenter.Y - m * prevCenter.X;

        // 找到 prev 房间上直线经过的点
        Point start;
        int direction;
        if (Math.Abs(m) >= 1)
        {
            if (angle < 90 || angle > 270)
            {
                direction = Room.DirTop;
                start = new Point((int)Math.Round((prev.Top - b) / m), prev.Top);
            }
            else
            {
                direction = Room.DirBottom;
                start = new Point((int)Math.Round((prev.Bottom - b) / m), prev.Bottom);
            }
        }
        else
        {
            if (angle < 180)
            {
                direction = Room.DirRight;
                start = new Point(prev.Right, (int)Math.Round(m * prev.Right + b));
            }
            else
            {
                direction = Room.DirLeft;
                start = new Point(prev.Left, (int)Math.Round(m * prev.Left + b));
            }
        }

        // 限制到有效的连接点范围
        if (direction == Room.DirTop || direction == Room.DirBottom)
        {
            start.X = (int)GameMath.Gate(prev.Left + 1, start.X, prev.Right - 1);
        }
        else
        {
            start.Y = (int)GameMath.Gate(prev.Top + 1, start.Y, prev.Bottom - 1);
        }

        // 空间检查
        var space = FindFreeSpace(start, collision, Math.Max(next.MaxWidth(), next.MaxHeight()));
        if (!next.SetSizeWithLimit(space.Width + 1, space.Height + 1))
        {
            return -1;
        }

        // 用直线公式和已知尺寸计算理想中心
        PointF targetCenter = new();
        if (direction == Room.DirTop)
        {
            targetCenter.Y = prev.Top - (next.Height - 1) / 2f;
            targetCenter.X = (float)((targetCenter.Y - b) / m);
            next.SetPos((int)MathF.Round(targetCenter.X - (next.Width - 1) / 2f), prev.Top - (next.Height - 1));
        }
        else if (direction == Room.DirBottom)
        {
            targetCenter.Y = prev.Bottom + (next.Height - 1) / 2f;
            targetCenter.X = (float)((targetCenter.Y - b) / m);
            next.SetPos((int)MathF.Round(targetCenter.X - (next.Width - 1) / 2f), prev.Bottom);
        }
        else if (direction == Room.DirRight)
        {
            targetCenter.X = prev.Right + (next.Width - 1) / 2f;
            targetCenter.Y = (float)(m * targetCenter.X + b);
            next.SetPos(prev.Right, (int)MathF.Round(targetCenter.Y - (next.Height - 1) / 2f));
        }
        else // DirLeft
        {
            targetCenter.X = prev.Left - (next.Width - 1) / 2f;
            targetCenter.Y = (float)(m * targetCenter.X + b);
            next.SetPos(prev.Left - (next.Width - 1), (int)MathF.Round(targetCenter.Y - (next.Height - 1) / 2f));
        }

        // 连接边界和目标检查，必要时移动房间
        if (direction == Room.DirTop || direction == Room.DirBottom)
        {
            if (next.Right < prev.Left + 2) next.Shift(prev.Left + 2 - next.Right, 0);
            else if (next.Left > prev.Right - 2) next.Shift(prev.Right - 2 - next.Left, 0);
            if (next.Right > space.Right) next.Shift(space.Right - next.Right, 0);
            else if (next.Left < space.Left) next.Shift(space.Left - next.Left, 0);
        }
        else
        {
            if (next.Bottom < prev.Top + 2) next.Shift(0, prev.Top + 2 - next.Bottom);
            else if (next.Top > prev.Bottom - 2) next.Shift(0, prev.Bottom - 2 - next.Top);
            if (next.Bottom > space.Bottom) next.Shift(0, space.Bottom - next.Bottom);
            else if (next.Top < space.Top) next.Shift(0, space.Top - next.Top);
        }

        // 尝试连接，成功返回角度
        if (next.Connect(prev))
        {
            return AngleBetweenRooms(prev, next);
        }
        return -1;
    }
}