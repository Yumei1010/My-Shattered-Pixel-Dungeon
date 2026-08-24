using MyShatteredPixelDungeon.scripts.dungeon;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

/// <summary>
///     ShadowCaster 递归阴影投射 FOV 测试
/// </summary>
public class ShadowCasterTests
{
    /// <summary>
    ///     验证 rounding 表：每行最大列 ≥ 1 且 ≤ 行号
    /// </summary>
    [Fact]
    public void Rounding_Table_Is_Valid()
    {
        for (int d = 1; d <= ShadowCaster.MaxDistance; d++)
        {
            Assert.True(ShadowCaster.Rounding[d][d] >= 1);
            Assert.True(ShadowCaster.Rounding[d][d] <= d);
        }
    }

    [Fact]
    public void CastShadow_SourceCell_AlwaysVisible()
    {
        var (fov, _) = CastShadowAt(10, 10, 30, 8);
        Assert.True(fov[10 + 10 * 30]);
    }

    [Fact]
    public void CastShadow_SmallDistance_LimitsFov()
    {
        // 视野距离 2，地图 30×30，角落不应可见
        var (fov, _) = CastShadowAt(15, 15, 30, 2);
        Assert.True(fov[15 + 15 * 30]);  // 源
        Assert.False(fov[0]);             // 左上角太远
        Assert.False(fov[29 + 29 * 30]);  // 右下角太远
    }

    [Fact]
    public void CastShadow_BlockingWall_Occludes()
    {
        // 30×30 地图，源 (0,16)，墙在 x=8，视野距离 10
        // 墙在视野范围内，墙后的点不可见
        int w = 30;
        var blocking = new bool[w * w];
        for (int y = 0; y < 30; y++) blocking[8 + y * w] = true;

        CastShadow(0, 16, w, out var fov, blocking, 10);

        // 源可见
        Assert.True(fov[0 + 16 * w]);
        // 墙前点可见
        Assert.True(fov[7 + 16 * w]);
        // 墙（第一个阻挡物，标记为可见）
        Assert.True(fov[8 + 16 * w]);
        // 墙后不可见
        Assert.False(fov[9 + 16 * w]);
        Assert.False(fov[10 + 16 * w]);
    }

    [Fact]
    public void CastShadow_Distance8_NoWall_CoversNearby()
    {
        int w = 30;
        var blocking = new bool[w * w];
        CastShadow(15, 15, w, out var fov, blocking, 8);

        // 源可见
        Assert.True(fov[15 + 15 * w]);

        // 距离 8 内可见
        Assert.True(fov[15 + 10 * w]);   // 正上方 5 格
        Assert.True(fov[15 + 20 * w]);   // 正下方 5 格

        // 角落（距离 > 8）应不可见
        Assert.False(fov[0]);               // 左上角 (0,0) 距离 15
        Assert.False(fov[29 + 29 * w]);     // 右下角 (29,29) 距离 14
    }

    [Fact]
    public void CastShadow_MinimalMap_3x3()
    {
        // 3×3 最小可工作地图，源在中心 (1,1)
        var (fov, _) = CastShadowAt(1, 1, 3, 1);
        Assert.True(fov[1 + 1 * 3]);  // 源可见
        // 所有 8 邻域可见（距离 1 内）
        for (int i = 0; i < 9; i++)
            Assert.True(fov[i], $"Cell {i} should be visible");
    }

    [Fact]
    public void CastShadow_AllBlocking_ShowsAdjacent()
    {
        // 所有格子阻挡视线，但紧邻源的格子应可见（它们是被扫描到的第一个阻挡物）
        // 这是递归阴影投射算法的预期行为
        int w = 5;
        var blocking = new bool[w * w];
        for (int i = 0; i < blocking.Length; i++) blocking[i] = true;

        CastShadow(2, 2, w, out var fov, blocking, 1);

        // 源可见
        Assert.True(fov[2 + 2 * w]);
        // 距离 1 内（8 邻域）可见（它们是墙壁紧邻玩家）
        for (int i = 0; i < fov.Length; i++)
        {
            int dx = (i % w) - 2;
            int dy = (i / w) - 2;
            int dist = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (dist <= 1)
                Assert.True(fov[i], $"Cell {i} (dist={dist}) should be visible");
            else
                Assert.False(fov[i], $"Cell {i} (dist={dist}) should be hidden");
        }
    }

    [Fact]
    public void CastShadow_AllBlockingExceptSource_ShowsAdjacent()
    {
        // 源格子不阻挡，其余全部阻挡：结果同上
        int w = 5;
        var blocking = new bool[w * w];
        for (int i = 0; i < blocking.Length; i++) blocking[i] = true;
        blocking[2 + 2 * w] = false;

        CastShadow(2, 2, w, out var fov, blocking, 1);

        Assert.True(fov[2 + 2 * w]);
        for (int i = 0; i < fov.Length; i++)
        {
            int dx = (i % w) - 2;
            int dy = (i / w) - 2;
            int dist = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (dist <= 1)
                Assert.True(fov[i], $"Cell {i} (dist={dist}) should be visible");
            else
                Assert.False(fov[i], $"Cell {i} (dist={dist}) should be hidden");
        }
    }

    // ---------- 辅助方法 ----------

    private static (bool[] fov, bool[] blocking) CastShadowAt(int x, int y, int w, int distance = 8)
    {
        var blocking = new bool[w * w];
        var fov = new bool[w * w];
        ShadowCaster.CastShadow(x, y, w, fov, blocking, distance);
        return (fov, blocking);
    }

    private static void CastShadow(int x, int y, int w, out bool[] fov, bool[] blocking, int distance = 8)
    {
        fov = new bool[w * w];
        ShadowCaster.CastShadow(x, y, w, fov, blocking, distance);
    }
}