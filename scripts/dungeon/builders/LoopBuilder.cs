using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.connection;

namespace MyShatteredPixelDungeon.scripts.dungeon.builders;

/// <summary>
///     LoopBuilder，对应原版 LoopBuilder
///     以单一主环为核心元素的 Builder
/// </summary>
public sealed class LoopBuilder : RegularBuilder
{
    // 曲线指数：增大使环更椭圆
    private int _curveExponent = 0;

    // 曲线强度（0-1）：0=完美圆，1=完全由曲线指数决定
    private float _curveIntensity = 1;

    // 沿环的起始点偏移
    private float _curveOffset = 0;

    public LoopBuilder SetLoopShape(int exponent, float intensity, float offset)
    {
        _curveExponent = Math.Abs(exponent);
        _curveIntensity = intensity % 1f;
        _curveOffset = offset % 0.5f;
        return this;
    }

    private float TargetAngle(float percentAlong)
    {
        percentAlong += _curveOffset;
        return 360f * (float)(
            _curveIntensity * CurveEquation(percentAlong)
            + (1 - _curveIntensity) * percentAlong
            - _curveOffset);
    }

    private static double CurveEquation(double x)
    {
        // 环曲线方程：4 的 2*exponent 次幂 × 多项式 + 偏移
        int exp = 1; // 简化的曲线方程（对应 exponent=1 的常见形态）
        return Math.Pow(4, 2 * exp)
               * Math.Pow((x % 0.5f) - 0.25, 2 * exp + 1)
               + 0.25 + 0.5 * Math.Floor(2 * x);
    }

    private PointF _loopCenter;

    public override List<Room>? Build(List<Room> rooms)
    {
        SetupRooms(rooms);

        if (Entrance == null) return null;

        Entrance.SetSize();
        Entrance.SetPos(0, 0);

        float startAngle = (DeterministicRng.Float() * 360f);

        _mainPathRooms.Insert(0, Entrance);
        if (Exit != null)
        {
            _mainPathRooms.Insert((_mainPathRooms.Count + 1) / 2, Exit);
        }

        var loop = new List<Room>();
        var pathTunnels = (float[])_pathTunnelChances.Clone();
        foreach (var r in _mainPathRooms)
        {
            loop.Add(r);

            int tunnels = RandomChances(pathTunnels);
            if (tunnels == -1)
            {
                pathTunnels = (float[])_pathTunnelChances.Clone();
                tunnels = RandomChances(pathTunnels);
            }
            pathTunnels[tunnels]--;

            for (int j = 0; j < tunnels; j++)
            {
                loop.Add(ConnectionRoom.Create());
            }
        }

        Room prev = Entrance;
        float targetAngle;
        for (int i = 1; i < loop.Count; i++)
        {
            var r = loop[i];
            targetAngle = startAngle + TargetAngle(i / (float)loop.Count);
            float placed = PlaceRoom(rooms, prev, r, targetAngle);
            Console.WriteLine($"Loop {i}: {r.GetType().Name} angle={targetAngle:F1} placed={placed} prev=[{prev.Left},{prev.Top}→{prev.Right},{prev.Bottom}]");
            if (placed != -1)
            {
                prev = r;
                if (!rooms.Contains(prev)) rooms.Add(prev);
            }
            else
            {
                    return null;
            }
        }

        // 连接环的首尾
        while (!prev.Connect(Entrance))
        {
            var c = ConnectionRoom.Create();
            if (PlaceRoom(loop, prev, c, AngleBetweenRooms(prev, Entrance)) == -1)
            {
                return null;
            }
            loop.Add(c);
            rooms.Add(c);
            prev = c;
        }

        if (Shop != null)
        {
            float angle;
            int tries = 10;
            do
            {
                angle = PlaceRoom(loop, Entrance, Shop, (DeterministicRng.Float() * 360f));
                tries--;
            } while (angle == -1 && tries >= 0);
            if (angle == -1) return null;
        }

        _loopCenter = new PointF();
        foreach (var r in loop)
        {
            _loopCenter.X += (r.Left + r.Right) / 2f;
            _loopCenter.Y += (r.Top + r.Bottom) / 2f;
        }
        _loopCenter.X /= loop.Count;
        _loopCenter.Y /= loop.Count;

        var branchable = new List<Room>(loop);
        var roomsToBranch = new List<Room>();
        roomsToBranch.AddRange(_multiConnections);
        roomsToBranch.AddRange(_singleConnections);
        WeightRooms(branchable);
        if (!CreateBranches(rooms, branchable, roomsToBranch, _branchTunnelChances))
        {
            return null;
        }

        FindNeighbours(rooms);

        foreach (var r in rooms)
        {
            foreach (var n in r.Neighbours.ToList())
            {
                if (!n.Connected.ContainsKey(r)
                    && DeterministicRng.Float() < _extraConnectionChance)
                {
                    r.Connect(n);
                }
            }
        }

        return rooms;
    }

    protected override float RandomBranchAngle(Room r)
    {
        if (_loopCenter == default) return base.RandomBranchAngle(r);

        // 生成 4 个随机角度，返回指向环中心最接近的一个
        float toCenter = AngleBetweenPoints(
            new PointF((r.Left + r.Right) / 2f, (r.Top + r.Bottom) / 2f), _loopCenter);
        if (toCenter < 0) toCenter += 360f;

        float currAngle = (DeterministicRng.Float() * 360f);
        for (int i = 0; i < 4; i++)
        {
            float newAngle = (DeterministicRng.Float() * 360f);
            if (Math.Abs(toCenter - newAngle) < Math.Abs(toCenter - currAngle))
            {
                currAngle = newAngle;
            }
        }
        return currAngle;
    }
}