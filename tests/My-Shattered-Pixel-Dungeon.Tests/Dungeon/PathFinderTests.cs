using MyShatteredPixelDungeon.scripts.dungeon;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

/// <summary>
///     PathFinder BFS 距离地图寻路测试
/// </summary>
public class PathFinderTests
{
    private static bool[] BuildOpenMap(int width, int height)
    {
        var passable = new bool[width * height];
        Array.Fill(passable, true);
        return passable;
    }

    [Fact]
    public void SetMapSize_AllocatesArrays()
    {
        PathFinder.SetMapSize(10, 10);
        Assert.Equal(100, PathFinder.Distance.Length);
        Assert.Equal(8, PathFinder.Neighbours8.Length);
        Assert.Equal(4, PathFinder.Neighbours4.Length);
        Assert.Equal(9, PathFinder.Neighbours9.Length);
        Assert.Equal(4, PathFinder.Circle4.Length);
        Assert.Equal(8, PathFinder.Circle8.Length);
    }

    [Fact]
    public void Find_StraightLine_Path()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        var path = PathFinder.Find(0, 3, passable);
        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.Equal(1, path[0]);
        Assert.Equal(2, path[1]);
        Assert.Equal(3, path[2]);
    }

    [Fact]
    public void Find_WithObstacle_GoesAround()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        // 在 (5,5) 周围设置障碍物形成一个 3×3 的障碍区域
        int w = 10;
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                passable[(5 + dx) + (5 + dy) * w] = false;

        // 从 (0,5) 到 (9,5)，必须绕过障碍
        int from = 0 + 5 * w;  // (0,5)
        int to = 9 + 5 * w;    // (9,5)
        var path = PathFinder.Find(from, to, passable);
        Assert.NotNull(path);
        Assert.Equal(to, path![^1]);  // 到达目标
        // 路径不能经过障碍物
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                Assert.DoesNotContain((5 + dx) + (5 + dy) * w, path);
    }

    [Fact]
    public void Find_Unreachable_ReturnsNull()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        // 中间一堵完整墙
        for (int y = 0; y < 10; y++) passable[5 + y * 10] = false;

        var path = PathFinder.Find(0, 9, passable);
        Assert.Null(path);
    }

    [Fact]
    public void GetStep_ReturnsNextCell()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        int step = PathFinder.GetStep(0, 5, passable);
        Assert.Equal(1, step);  // 向右
    }

    [Fact]
    public void GetStep_Unreachable_ReturnsMinusOne()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);
        for (int y = 0; y < 10; y++) passable[5 + y * 10] = false;

        int step = PathFinder.GetStep(0, 9, passable);
        Assert.Equal(-1, step);
    }

    [Fact]
    public void GetStepBack_FleesFromTarget()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        // 在 cell 0，敌人从 cell 20 追来，后退一步
        int step = PathFinder.GetStepBack(0, 20, 8, passable, true);
        // 远离敌人方向
        Assert.NotEqual(-1, step);
        Assert.NotEqual(0, step);
    }

    [Fact]
    public void BuildDistanceMap_MeasuresDistances()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        PathFinder.BuildDistanceMap(0, passable);
        Assert.Equal(0, PathFinder.Distance[0]);
        Assert.Equal(1, PathFinder.Distance[1]);
        Assert.Equal(1, PathFinder.Distance[10]);  // 正下方
        Assert.Equal(2, PathFinder.Distance[2]);
        Assert.Equal(9, PathFinder.Distance[99]);  // 最远距离
    }

    [Fact]
    public void BuildDistanceMap_WithLimit_StopsAtLimit()
    {
        PathFinder.SetMapSize(10, 10);
        var passable = BuildOpenMap(10, 10);

        PathFinder.BuildDistanceMap(0, passable, 2);
        Assert.Equal(0, PathFinder.Distance[0]);
        // (2,1) 距离 2 应可达
        int cell = 2 + 1 * 10;  // (2,1)
        Assert.Equal(2, PathFinder.Distance[cell]);
        // 超出限制的应保持 int.MaxValue
        Assert.Equal(int.MaxValue, PathFinder.Distance[99]);  // (9,9) 距离 9
    }

    [Fact]
    public void Neighbours_Are_Symmetric()
    {
        PathFinder.SetMapSize(10, 10);

        // Neighbours8 的偏移和应为 0（对称性）
        int sum = 0;
        foreach (int n in PathFinder.Neighbours8) sum += n;
        Assert.Equal(0, sum);

        // Neighbours4 的偏移和应为 0
        sum = 0;
        foreach (int n in PathFinder.Neighbours4) sum += n;
        Assert.Equal(0, sum);
    }
}