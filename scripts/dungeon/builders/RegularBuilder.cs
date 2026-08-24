using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.connection;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.special;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.builders;

/// <summary>
///     RegularBuilder 抽象基类，对应原版 RegularBuilder
///     引入主路径和分支的概念，隧道填充在其中
/// </summary>
public abstract class RegularBuilder : Builder
{
    // *** 参数值 ***
    protected float _pathVariance = 45f;

    public RegularBuilder SetPathVariance(float variance)
    {
        _pathVariance = variance;
        return this;
    }

    // 路径长度 = 主路径房间占可通行房间的百分比
    protected float _pathLength = 0.25f;
    protected float[] _pathLenJitterChances = new[] { 0f, 0f, 0f, 1f };

    public RegularBuilder SetPathLength(float len, float[] jitter)
    {
        _pathLength = len;
        _pathLenJitterChances = jitter;
        return this;
    }

    protected float[] _pathTunnelChances = new[] { 2f, 2f, 1f };
    protected float[] _branchTunnelChances = new[] { 1f, 1f, 0f };

    public RegularBuilder SetTunnelLength(float[] path, float[] branch)
    {
        _pathTunnelChances = path;
        _branchTunnelChances = branch;
        return this;
    }

    // 额外连接概率（相邻房间之间）
    protected float _extraConnectionChance = 0.30f;

    public RegularBuilder SetExtraConnectionChance(float chance)
    {
        _extraConnectionChance = chance;
        return this;
    }

    // *** 房间状态 ***
    protected Room? Entrance;
    protected Room? Exit;
    protected Room? Shop;

    protected List<Room> _mainPathRooms = new();
    protected List<Room> _multiConnections = new();
    protected List<Room> _singleConnections = new();

    /// <summary>
    ///     设置房间：识别入口/出口/商店，分配主路径房间
    /// </summary>
    protected void SetupRooms(List<Room> rooms)
    {
        foreach (var r in rooms) r.SetEmpty();

        Entrance = Exit = Shop = null;
        _mainPathRooms.Clear();
        _singleConnections.Clear();
        _multiConnections.Clear();

        foreach (var r in rooms)
        {
            if (r.IsEntrance()) Entrance = r;
            else if (r.IsExit()) Exit = r;
            else if (r is ShopRoom && r.MaxConnections(Room.DirAll) == 1) Shop = r;
            else if (r.MaxConnections(Room.DirAll) > 1) _multiConnections.Add(r);
            else if (r.MaxConnections(Room.DirAll) == 1) _singleConnections.Add(r);
        }

        // 大房间更可能出现在主环中（放在 multiconnections 前部）
        WeightRooms(_multiConnections);
        Shuffle(_multiConnections);
        // 按引用去重（Rect 重写了 Equals 值相等，Distinct 会错误折叠同边界的房间）
        var seen = new HashSet<Room>(ReferenceEqualityComparer.Instance);
        _multiConnections = _multiConnections.Where(seen.Add).ToList();
        Shuffle(_multiConnections);

        int roomsOnMainPath = (int)(_multiConnections.Count * _pathLength) + RandomChances(_pathLenJitterChances);
        while (roomsOnMainPath > 0 && _multiConnections.Count > 0)
        {
            var r = _multiConnections[0];
            _multiConnections.RemoveAt(0);
            roomsOnMainPath -= r is StandardRoom sr ? sr.SizeFactor() : 1;
            _mainPathRooms.Add(r);
        }
    }

    /// <summary>
    ///     权重：大房间在列表中多次出现以增加被选中的概率
    /// </summary>
    protected static void WeightRooms(List<Room> rooms)
    {
        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            if (rooms[i] is StandardRoom sr)
            {
                for (int j = 1; j < sr.ConnectionWeight(); j++)
                {
                    rooms.Add(sr);
                }
            }
        }
    }

    /// <summary>
    ///     将 roomsToBranch 中的房间放置到 branchable 房间的分支中
    /// </summary>
    protected bool CreateBranches(List<Room> rooms, List<Room> branchable, List<Room> roomsToBranch, float[] connChances)
    {
        int i = 0;
        float angle;
        int tries;
        Room curr;
        var connectingRoomsThisBranch = new List<Room>();
        int failedBranchAttempts = 0;
        var connectionChances = (float[])connChances.Clone();

        while (i < roomsToBranch.Count)
        {
            if (failedBranchAttempts > 100) return false;

            var r = roomsToBranch[i];
            connectingRoomsThisBranch.Clear();

            // 随机选择分支起点（秘密房间不能从连接房间分支）
            do { curr = branchable[System.Random.Shared.Next(branchable.Count)]; }
            while (r is SecretRoom && curr is ConnectionRoom);

            int connectingRooms = RandomChances(connectionChances);
            if (connectingRooms == -1)
            {
                connectionChances = (float[])connChances.Clone();
                connectingRooms = RandomChances(connectionChances);
            }
            connectionChances[connectingRooms]--;

            for (int j = 0; j < connectingRooms; j++)
            {
                ConnectionRoom t = r is SecretRoom ? new MazeConnectionRoom() : ConnectionRoom.Create();
                tries = 3;
                do
                {
                    angle = PlaceRoom(rooms, curr, t, RandomBranchAngle(curr));
                    tries--;
                } while (angle == -1 && tries > 0);

                if (angle == -1)
                {
                    t.ClearConnections();
                    foreach (var c in connectingRoomsThisBranch)
                    {
                        c.ClearConnections();
                        rooms.Remove(c);
                    }
                    connectingRoomsThisBranch.Clear();
                    break;
                }
                else
                {
                    connectingRoomsThisBranch.Add(t);
                    rooms.Add(t);
                }
                curr = t;
            }

            if (connectingRoomsThisBranch.Count != connectingRooms)
            {
                failedBranchAttempts++;
                continue;
            }

            tries = 10;
            do
            {
                angle = PlaceRoom(rooms, curr, r, RandomBranchAngle(curr));
                tries--;
            } while (angle == -1 && tries > 0);

            if (angle == -1)
            {
                r.ClearConnections();
                foreach (var t in connectingRoomsThisBranch)
                {
                    t.ClearConnections();
                    rooms.Remove(t);
                }
                connectingRoomsThisBranch.Clear();
                failedBranchAttempts++;
                continue;
            }

            for (int j = 0; j < connectingRoomsThisBranch.Count; j++)
            {
                if (System.Random.Shared.Next(3) <= 1) branchable.Add(connectingRoomsThisBranch[j]);
            }
            if (r.MaxConnections(Room.DirAll) > 1 && System.Random.Shared.Next(3) == 0)
            {
                if (r is StandardRoom sr)
                {
                    for (int j = 0; j < sr.ConnectionWeight(); j++) branchable.Add(r);
                }
                else
                {
                    branchable.Add(r);
                }
            }
            i++;
        }
        return true;
    }

    protected virtual float RandomBranchAngle(Room r) => (float)(System.Random.Shared.NextDouble() * 360f);

    private static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = System.Random.Shared.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    protected static int RandomChances(float[] probs)
    {
        float total = probs.Sum();
        if (total <= 0) return -1;
        float roll = (float)System.Random.Shared.NextDouble() * total;
        float cumulative = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            cumulative += probs[i];
            if (roll < cumulative) return i;
        }
        return -1;
    }
}