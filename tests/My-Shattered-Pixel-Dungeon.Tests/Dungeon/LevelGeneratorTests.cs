using MyShatteredPixelDungeon.scripts.dungeon;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

/// <summary>
///     LevelGenerator 完整生成管线测试
/// </summary>
public class LevelGeneratorTests
{
    [Fact]
    public void Generate_Level1_ReturnsValidData()
    {
        var gen = new LevelGenerator(1, 12345);
        var data = gen.Generate();

        Assert.NotNull(data);
        Assert.Equal(1, data.Depth);
        Assert.Equal(64, data.Width);
        Assert.Equal(64, data.Height);
        Assert.Equal(4096, data.Length);
        Assert.NotNull(data.Map);
        Assert.Equal(4096, data.Map.Length);
    }

    [Fact]
    public void Generate_Entrance_Exists()
    {
        var data = new LevelGenerator(1, 12345).Generate();
        int entrance = data.Entrance;
        Assert.NotEqual(0, entrance);
        Assert.Equal(Terrain.Entrance, data.Map[entrance]);
    }

    [Fact]
    public void Generate_Exit_Exists()
    {
        var data = new LevelGenerator(1, 12345).Generate();
        int exit = data.Exit;
        Assert.NotEqual(0, exit);
        Assert.Equal(Terrain.Exit, data.Map[exit]);
    }

    [Fact]
    public void Generate_FlagMaps_ArePopulated()
    {
        var data = new LevelGenerator(1, 12345).Generate();

        Assert.Equal(4096, data.Passable.Length);
        Assert.Equal(4096, data.LosBlocking.Length);
        Assert.Equal(4096, data.Solid.Length);

        // 墙壁应是固体
        Assert.True(data.Solid[0]);
        Assert.False(data.Passable[0]);
    }

    [Fact]
    public void Generate_SameSeed_ProducesIdenticalMap()
    {
        var data1 = new LevelGenerator(5, 99999).Generate();
        var data2 = new LevelGenerator(5, 99999).Generate();

        Assert.Equal(data1.Map.Length, data2.Map.Length);
        Assert.True(data1.Map.SequenceEqual(data2.Map),
            $"Same seed should produce identical map. Diff at first index: {FirstDiff(data1.Map, data2.Map)}");
    }

    [Fact]
    public void Generate_DifferentSeed_ProducesDifferentMap()
    {
        var data1 = new LevelGenerator(5, 10000).Generate();
        var data2 = new LevelGenerator(5, 20000).Generate();

        // 不同种子很可能产生不同地图
        bool identical = data1.Map.SequenceEqual(data2.Map);
        Assert.False(identical, "Different seeds should produce different maps");
    }

    [Fact]
    public void Generate_DifferentDepth_ProducesDifferentMap()
    {
        var data1 = new LevelGenerator(1, 55555).Generate();
        var data2 = new LevelGenerator(2, 55555).Generate();

        bool identical = data1.Map.SequenceEqual(data2.Map);
        Assert.False(identical, "Different depths should produce different maps");
    }

    [Fact]
    public void Generate_AllDepths_Succeed()
    {
        // 所有 26 层都应能成功生成
        for (int depth = 1; depth <= 26; depth++)
        {
            var data = new LevelGenerator(depth, 12345 + depth).Generate();
            Assert.NotNull(data);
            Assert.Equal(depth, data.Depth);
            if (data.Entrance == 0)
                Assert.Fail($"Depth {depth}: entrance not set");
            if (data.Exit == 0)
                Assert.Fail($"Depth {depth}: exit not set");
        }
    }

    [Fact]
    public void Generate_Map_HasPassableArea()
    {
        var data = new LevelGenerator(1, 12345).Generate();

        int passableCount = data.Passable.Count(p => p);
        int total = data.Passable.Length;
        Assert.True(passableCount > total * 0.05,
            $"Passable area too small: {passableCount}/{total} = {passableCount * 100f / total:F1}%");
    }

    [Fact]
    public void Generate_Map_HasDoors()
    {
        var data = new LevelGenerator(1, 12345).Generate();

        // 至少应有可通行区域
        Assert.True(data.Passable.Count(p => p) > 50, "Map should have passable area");
    }

    [Fact]
    public void DeterministicRng_ProducesSameSequence()
    {
        var rng1 = new DeterministicRng(42);
        var rng2 = new DeterministicRng(42);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.NextLong(), rng2.NextLong());
        }
    }

    [Fact]
    public void DeterministicRng_SameSeed_SameSequence()
    {
        using (DeterministicRng.WithSeed(42))
        {
            int a1 = DeterministicRng.Range(100);
            int a2 = DeterministicRng.Range(100);
            int a3 = DeterministicRng.Range(100);

            using (DeterministicRng.WithSeed(42))
            {
                int b1 = DeterministicRng.Range(100);
                int b2 = DeterministicRng.Range(100);
                int b3 = DeterministicRng.Range(100);

                Assert.Equal(a1, b1);
                Assert.Equal(a2, b2);
                Assert.Equal(a3, b3);
            }
        }
    }

    [Fact]
    public void DeterministicRng_DifferentSeed_DifferentSequence()
    {
        using (DeterministicRng.WithSeed(42))
        {
            int a = DeterministicRng.Range(10000);
            using (DeterministicRng.WithSeed(99))
            {
                int b = DeterministicRng.Range(10000);
                Assert.NotEqual(a, b);
            }
        }
    }

    private static int FirstDiff(int[] a, int[] b)
    {
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            if (a[i] != b[i]) return i;
        return -1;
    }
}