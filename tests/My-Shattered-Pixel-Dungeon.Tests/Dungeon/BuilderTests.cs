using MyShatteredPixelDungeon.scripts.dungeon.builders;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

/// <summary>
///     Builder 房间连接算法测试
/// </summary>
public class BuilderTests
{
    private static List<Room> CreateRoomSet(int extraCount)
    {
        var rooms = new List<Room>
        {
            new EntranceRoom(),
            new ExitRoom()
        };
        for (int i = 0; i < extraCount; i++)
        {
            rooms.Add(new EmptyRoom());
        }
        return rooms;
    }

    /// <summary>
    ///     带重试的 Build（模拟 Level.create() 的 do-while 循环）
    /// </summary>
    private static List<Room>? BuildWithRetry(LoopBuilder builder, List<Room> rooms, int maxTries = 20)
    {
        for (int i = 0; i < maxTries; i++)
        {
            var freshRooms = rooms.Select(r => (Room)Activator.CreateInstance(r.GetType())!).ToList();
            var result = builder.Build(freshRooms);
            if (result != null) return result;
        }
        return null;
    }

    [Fact]
    public void LoopBuilder_Builds_ConnectedMap()
    {
        var result = BuildWithRetry(new LoopBuilder().SetLoopShape(2, 0.5f, 0.25f), CreateRoomSet(4));
        Assert.NotNull(result);
        // 结果应包含原房间 + 添加的连接房间（≥ 6）
        Assert.True(result!.Count >= 6);

        // 每个房间都应该有连接
        foreach (var r in result)
        {
            Assert.True(r.Connected.Count > 0, $"Room {r} has no connections");
        }
    }

    [Fact]
    public void LoopBuilder_AllRooms_Reachable()
    {
        var result = BuildWithRetry(new LoopBuilder(), CreateRoomSet(6));
        Assert.NotNull(result);

        // BFS 检查连通性
        var visited = new HashSet<Room>();
        var queue = new Queue<Room>();
        queue.Enqueue(result![0]);
        visited.Add(result[0]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in current.Connected.Keys)
            {
                if (visited.Add(next)) queue.Enqueue(next);
            }
        }

        Assert.Equal(result.Count, visited.Count);
    }

    [Fact]
    public void LoopBuilder_Repeated_GeneratesDifferentLayouts()
    {
        var layouts = new HashSet<string>();
        int attempts = 0;
        while (layouts.Count < 3 && attempts < 30)
        {
            attempts++;
            var result = BuildWithRetry(new LoopBuilder(), CreateRoomSet(4));
            if (result == null) continue;

            var fingerprint = string.Join(";",
                result.Select(r => $"{r.Left},{r.Top},{r.Right},{r.Bottom}").OrderBy(s => s));
            layouts.Add(fingerprint);
        }

        Assert.True(layouts.Count >= 3, $"Expected >= 3 distinct layouts, got {layouts.Count}");
    }

    [Fact]
    public void LoopBuilder_WithManyRooms_Succeeds()
    {
        // 15 个房间 + 重试机制 → 应该总能成功
        var result = BuildWithRetry(new LoopBuilder(), CreateRoomSet(13));
        Assert.NotNull(result);
    }
}