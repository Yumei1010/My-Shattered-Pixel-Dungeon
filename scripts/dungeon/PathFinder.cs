namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     寻路工具类，对应原版 com.watabou.utils.PathFinder
///     使用 BFS 距离地图算法：从目标反向构建距离场，然后沿距离下降方向移动
///     比 A* 更快，且天然支持多目标（goals 数组）
/// </summary>
public static class PathFinder
{
    /// <summary>
    ///     每个格子的距离场（由 BuildDistanceMap 填充）
    /// </summary>
    public static int[] Distance = Array.Empty<int>();

    private static int[] _maxVal = Array.Empty<int>();
    private static bool[] _goals = Array.Empty<bool>();
    private static int[] _queue = Array.Empty<int>();
    private static bool[] _queued = Array.Empty<bool>();

    private static int _size;
    private static int _width;

    /// <summary>
    ///     8 方向偏移（数组访问顺序优化）
    /// </summary>
    private static int[] _dir = Array.Empty<int>();

    /// <summary>
    ///     8 方向偏移（顺时针顺序，用于边界安全遍历）
    /// </summary>
    private static int[] _dirLr = Array.Empty<int>();

    /// <summary>
    ///     4 邻域偏移（上下左右）
    /// </summary>
    public static int[] Neighbours4 = Array.Empty<int>();

    /// <summary>
    ///     8 邻域偏移
    /// </summary>
    public static int[] Neighbours8 = Array.Empty<int>();

    /// <summary>
    ///     9 邻域偏移（含自身）
    /// </summary>
    public static int[] Neighbours9 = Array.Empty<int>();

    /// <summary>
    ///     4 邻域偏移（顺时针顺序）
    /// </summary>
    public static int[] Circle4 = Array.Empty<int>();

    /// <summary>
    ///     8 邻域偏移（顺时针顺序）
    /// </summary>
    public static int[] Circle8 = Array.Empty<int>();

    /// <summary>
    ///     设置地图尺寸，分配内部数组并计算邻域偏移
    /// </summary>
    public static void SetMapSize(int width, int height)
    {
        _width = width;
        _size = width * height;

        Distance = new int[_size];
        _goals = new bool[_size];
        _queue = new int[_size + 8]; // +8 容差，应对 n == from 重复入队
        _queued = new bool[_size];

        _maxVal = new int[_size];
        Array.Fill(_maxVal, int.MaxValue);

        _dir = new[] { -1, +1, -width, +width, -width - 1, -width + 1, +width - 1, +width + 1 };
        _dirLr = new[] { -1 - width, -1, -1 + width, -width, +width, +1 - width, +1, +1 + width };

        Neighbours4 = new[] { -width, -1, +1, +width };
        Neighbours8 = new[] { -width - 1, -width, -width + 1, -1, +1, +width - 1, +width, +width + 1 };
        Neighbours9 = new[] { -width - 1, -width, -width + 1, -1, 0, +1, +width - 1, +width, +width + 1 };

        Circle4 = new[] { -width, +1, +width, -1 };
        Circle8 = new[] { -width - 1, -width, -width + 1, +1, +width + 1, +width, +width - 1, -1 };
    }

    /// <summary>
    ///     寻找从 from 到 to 的完整路径
    /// </summary>
    /// <returns>路径单元格列表（不含起点），不可达返回 null</returns>
    public static List<int>? Find(int from, int to, bool[] passable)
    {
        if (!BuildDistanceMap(from, to, passable))
        {
            return null;
        }

        var result = new List<int>();
        int s = from;

        // 从起点沿距离下降方向移动，直到到达终点
        do
        {
            int minD = Distance[s];
            int mins = s;

            for (int i = 0; i < _dir.Length; i++)
            {
                int n = s + _dir[i];
                if (n >= 0 && n < _size)
                {
                    int thisD = Distance[n];
                    if (thisD < minD)
                    {
                        minD = thisD;
                        mins = n;
                    }
                }
            }

            s = mins;
            result.Add(s);
        } while (s != to);

        return result;
    }

    /// <summary>
    ///     获取从 from 走向 to 的下一步
    /// </summary>
    /// <returns>下一步单元格，不可达返回 -1</returns>
    public static int GetStep(int from, int to, bool[] passable)
    {
        if (!BuildDistanceMap(from, to, passable))
        {
            return -1;
        }

        // 从起始位置沿距离下降方向移动一步
        int minD = Distance[from];
        int best = from;

        for (int i = 0; i < _dir.Length; i++)
        {
            int step = from + _dir[i];
            if (step >= 0 && step < _size)
            {
                int stepD = Distance[step];
                if (stepD < minD)
                {
                    minD = stepD;
                    best = step;
                }
            }
        }

        return best;
    }

    /// <summary>
    ///     获取远离 from 的撤退一步（用于逃跑 AI）
    /// </summary>
    /// <param name="lookahead">撤退的目标距离</param>
    public static int GetStepBack(int cur, int from, int lookahead, bool[] passable, bool canApproachFromPos)
    {
        int d = BuildEscapeDistanceMap(cur, from, lookahead, passable);
        if (d == 0) return -1;

        if (!canApproachFromPos)
        {
            // 不能靠近撤退源位置：重新计算并限制目标距离
            int head = 0;
            int tail = 0;

            int newD = Distance[cur];
            BArray.SetFalse(_queued);

            _queue[tail++] = cur;
            _queued[cur] = true;

            while (head < tail)
            {
                int step = _queue[head++];

                if (Distance[step] > newD)
                {
                    newD = Distance[step];
                }

                int start = (step % _width == 0 ? 3 : 0);
                int end = ((step + 1) % _width == 0 ? 3 : 0);
                for (int i = start; i < _dirLr.Length - end; i++)
                {
                    int n = step + _dirLr[i];
                    if (n >= 0 && n < _size && passable[n])
                    {
                        if (Distance[n] < Distance[cur])
                        {
                            passable[n] = false;
                        }
                        else if (Distance[n] >= Distance[step] && !_queued[n])
                        {
                            _queue[tail++] = n;
                            _queued[n] = true;
                        }
                    }
                }
            }

            d = Math.Min(newD, d);
        }

        for (int i = 0; i < _size; i++)
        {
            _goals[i] = Distance[i] == d;
        }

        if (!BuildDistanceMap(cur, _goals, passable))
        {
            return -1;
        }

        int s = cur;
        int minD2 = Distance[s];
        int mins = s;

        for (int i = 0; i < _dir.Length; i++)
        {
            int n = s + _dir[i];
            if (n >= 0 && n < _size)
            {
                int thisD = Distance[n];
                if (thisD < minD2)
                {
                    minD2 = thisD;
                    mins = n;
                }
            }
        }

        return mins;
    }

    /// <summary>
    ///     从目标反向构建距离地图（单目标版本）
    /// </summary>
    private static bool BuildDistanceMap(int from, int to, bool[] passable)
    {
        if (from == to)
        {
            return false;
        }

        Array.Copy(_maxVal, 0, Distance, 0, _maxVal.Length);

        bool pathFound = false;

        int head = 0;
        int tail = 0;

        // 从目标开始 BFS
        _queue[tail++] = to;
        Distance[to] = 0;

        while (head < tail)
        {
            int step = _queue[head++];
            if (step == from)
            {
                pathFound = true;
                break;
            }

            int nextDistance = Distance[step] + 1;

            int start = (step % _width == 0 ? 3 : 0);
            int end = ((step + 1) % _width == 0 ? 3 : 0);
            for (int i = start; i < _dirLr.Length - end; i++)
            {
                int n = step + _dirLr[i];
                if (n == from || (n >= 0 && n < _size && passable[n] && Distance[n] > nextDistance))
                {
                    _queue[tail++] = n;
                    Distance[n] = nextDistance;
                }
            }
        }

        return pathFound;
    }

    /// <summary>
    ///     从目标反向构建距离地图，限制最大距离
    /// </summary>
    public static void BuildDistanceMap(int to, bool[] passable, int limit)
    {
        Array.Copy(_maxVal, 0, Distance, 0, _maxVal.Length);

        int head = 0;
        int tail = 0;

        _queue[tail++] = to;
        Distance[to] = 0;

        while (head < tail)
        {
            int step = _queue[head++];

            int nextDistance = Distance[step] + 1;
            if (nextDistance > limit)
            {
                return;
            }

            int start = (step % _width == 0 ? 3 : 0);
            int end = ((step + 1) % _width == 0 ? 3 : 0);
            for (int i = start; i < _dirLr.Length - end; i++)
            {
                int n = step + _dirLr[i];
                if (n >= 0 && n < _size && passable[n] && Distance[n] > nextDistance)
                {
                    _queue[tail++] = n;
                    Distance[n] = nextDistance;
                }
            }
        }
    }

    /// <summary>
    ///     从多目标集合反向构建距离地图
    /// </summary>
    private static bool BuildDistanceMap(int from, bool[] to, bool[] passable)
    {
        if (to[from])
        {
            return false;
        }

        Array.Copy(_maxVal, 0, Distance, 0, _maxVal.Length);

        bool pathFound = false;

        int head = 0;
        int tail = 0;

        // 所有目标格入队
        for (int i = 0; i < _size; i++)
        {
            if (to[i])
            {
                _queue[tail++] = i;
                Distance[i] = 0;
            }
        }

        while (head < tail)
        {
            int step = _queue[head++];
            if (step == from)
            {
                pathFound = true;
                break;
            }

            int nextDistance = Distance[step] + 1;

            int start = (step % _width == 0 ? 3 : 0);
            int end = ((step + 1) % _width == 0 ? 3 : 0);
            for (int i = start; i < _dirLr.Length - end; i++)
            {
                int n = step + _dirLr[i];
                if (n == from || (n >= 0 && n < _size && passable[n] && Distance[n] > nextDistance))
                {
                    _queue[tail++] = n;
                    Distance[n] = nextDistance;
                }
            }
        }

        return pathFound;
    }

    /// <summary>
    ///     构建逃生距离地图：从 from 开始扩散，记录 cur 到 from 的距离，
    ///     返回达到 lookAhead 深度时扩散到的最大距离
    /// </summary>
    private static int BuildEscapeDistanceMap(int cur, int from, int lookAhead, bool[] passable)
    {
        Array.Copy(_maxVal, 0, Distance, 0, _maxVal.Length);

        int destDist = int.MaxValue;

        int head = 0;
        int tail = 0;

        _queue[tail++] = from;
        Distance[from] = 0;

        int dist = 0;

        while (head < tail)
        {
            int step = _queue[head++];
            dist = Distance[step];

            if (dist > destDist)
            {
                return destDist;
            }

            if (step == cur)
            {
                destDist = dist + lookAhead;
            }

            int nextDistance = dist + 1;

            int start = (step % _width == 0 ? 3 : 0);
            int end = ((step + 1) % _width == 0 ? 3 : 0);
            for (int i = start; i < _dirLr.Length - end; i++)
            {
                int n = step + _dirLr[i];
                if (n >= 0 && n < _size && passable[n] && Distance[n] > nextDistance)
                {
                    _queue[tail++] = n;
                    Distance[n] = nextDistance;
                }
            }
        }

        return dist;
    }

    /// <summary>
    ///     从目标反向构建完整距离地图（无限制）
    /// </summary>
    public static void BuildDistanceMap(int to, bool[] passable)
    {
        Array.Copy(_maxVal, 0, Distance, 0, _maxVal.Length);

        int head = 0;
        int tail = 0;

        _queue[tail++] = to;
        Distance[to] = 0;

        while (head < tail)
        {
            int step = _queue[head++];
            int nextDistance = Distance[step] + 1;

            int start = (step % _width == 0 ? 3 : 0);
            int end = ((step + 1) % _width == 0 ? 3 : 0);
            for (int i = start; i < _dirLr.Length - end; i++)
            {
                int n = step + _dirLr[i];
                if (n >= 0 && n < _size && passable[n] && Distance[n] > nextDistance)
                {
                    _queue[tail++] = n;
                    Distance[n] = nextDistance;
                }
            }
        }
    }
}