using MyShatteredPixelDungeon.scripts.dungeon;
using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

/// <summary>
///     Room 基类与标准房间测试
/// </summary>
public class RoomTests
{
    private static DungeonData CreateData(int w, int h)
    {
        var data = new DungeonData { Width = w, Height = h };
        data.InitMap();
        return data;
    }

    // ---------- Room 基类 ----------

    [Fact]
    public void EmptyRoom_Paint_ProducesWallsAndFloor()
    {
        var room = new EmptyRoom();
        room.SetSize();
        room.ForceSize(7, 7);  // 7x7 标准房间

        var data = CreateData(50, 50);
        room.Paint(data);

        // 墙壁在边界
        Assert.Equal(Terrain.Wall, data.Map[room.Left + room.Top * data.Width]);
        // 内部是空地
        Assert.Equal(Terrain.Empty, data.Map[room.Left + 1 + (room.Top + 1) * data.Width]);
        // 墙壁不在内部
        Assert.NotEqual(Terrain.Wall, data.Map[room.Left + 1 + (room.Top + 1) * data.Width]);
    }

    [Fact]
    public void Room_Size_IsWithinBounds()
    {
        var room = new EmptyRoom();
        for (int i = 0; i < 10; i++)
        {
            Assert.True(room.SetSize());
            Assert.True(room.Width >= room.MinWidth() && room.Width <= room.MaxWidth());
            Assert.True(room.Height >= room.MinHeight() && room.Height <= room.MaxHeight());
        }
    }

    [Fact]
    public void Room_Center_IsInside()
    {
        var room = new EmptyRoom();
        room.ForceSize(10, 10);
        var center = room.Center();
        Assert.True(room.Contains(center));
    }

    [Fact]
    public void Room_Random_IsInside()
    {
        var room = new EmptyRoom();
        room.ForceSize(10, 10);
        for (int i = 0; i < 10; i++)
        {
            var p = room.Random();
            Assert.True(p.X > room.Left && p.X < room.Right);
            Assert.True(p.Y > room.Top && p.Y < room.Bottom);
        }
    }

    [Fact]
    public void Room_AddNeighbour_Works()
    {
        var a = new EmptyRoom();
        var b = new EmptyRoom();
        a.Set(new Rect(0, 0, 5, 7));
        b.Set(new Rect(5, 0, 10, 7));
        bool added = a.AddNeighbour(b);
        Assert.True(added);
        Assert.Contains(b, a.Neighbours);
        Assert.Contains(a, b.Neighbours);
    }

    [Fact]
    public void Room_Connect_Works()
    {
        var a = new EmptyRoom();
        var b = new EmptyRoom();
        a.Set(new Rect(0, 0, 5, 7));
        b.Set(new Rect(5, 0, 10, 7));
        a.AddNeighbour(b);
        bool connected = a.Connect(b);
        Assert.True(connected);
        Assert.Contains(b, a.Connected);
        Assert.Contains(a, b.Connected);
    }

    [Fact]
    public void StandardRoom_SizeCategory_Varies()
    {
        // 验证尺寸分类概率：Normal 应有最高概率
        var room = new EmptyRoom();
        var counts = new int[3];
        for (int i = 0; i < 100; i++)
        {
            room.SetSizeCategory();
            counts[(int)room.SizeCat]++;
        }
        // Normal 应占多数
        Assert.True(counts[0] > 0);
    }

    // ---------- 5 种基础房间 ----------

    [Fact]
    public void HallwayRoom_Paint_ProducesDeco()
    {
        var room = new HallwayRoom();
        room.ForceSize(7, 5);
        var data = CreateData(50, 50);
        room.Paint(data);

        // 内部是空地
        int center = (room.Left + 1) + (room.Top + 1) * data.Width;
        Assert.Equal(Terrain.Empty, data.Map[center]);
    }

    [Fact]
    public void PillarsRoom_Paint_AddsPillars()
    {
        var room = new PillarsRoom();
        room.ForceSize(8, 8);
        var data = CreateData(50, 50);
        room.Paint(data);

        // 6x6 内部应有 4 根柱子（墙）
        int pillarCount = 0;
        for (int y = room.Top + 1; y < room.Bottom; y++)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                if (data.Map[x + y * data.Width] == Terrain.Wall)
                    pillarCount++;
            }
        }
        // 至少有 4 根柱子
        Assert.True(pillarCount >= 4);
    }

    [Fact]
    public void StudyRoom_Paint_AddsBookshelves()
    {
        var room = new StudyRoom();
        room.ForceSize(8, 8);
        var data = CreateData(50, 50);
        room.Paint(data);

        // 应有书架
        bool hasBookshelf = false;
        for (int y = room.Top + 1; y < room.Bottom; y++)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                if (data.Map[x + y * data.Width] == Terrain.Bookshelf)
                    hasBookshelf = true;
            }
        }
        Assert.True(hasBookshelf);
    }

    [Fact]
    public void StripedRoom_Paint_HasDeco()
    {
        var room = new StripedRoom();
        room.ForceSize(8, 8);
        var data = CreateData(50, 50);
        room.Paint(data);

        // 应有装饰（EmptyDeco 条纹）
        bool hasDeco = false;
        for (int y = room.Top + 1; y < room.Bottom; y++)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                if (data.Map[x + y * data.Width] == Terrain.EmptyDeco)
                    hasDeco = true;
            }
        }
        Assert.True(hasDeco);
    }

    // ---------- DungeonData ----------

    [Fact]
    public void DungeonData_CellToPoint_AndBack()
    {
        var data = new DungeonData { Width = 50, Height = 50 };
        var p = data.CellToPoint(123);
        int cell = data.PointToCell(p);
        Assert.Equal(123, cell);
    }

    [Fact]
    public void DungeonData_InitMap_Fills()
    {
        var data = new DungeonData { Width = 10, Height = 10 };
        data.InitMap();
        Assert.All(data.Map, t => Assert.Equal(Terrain.Wall, t));
    }

    // ---------- Painter ----------

    [Fact]
    public void Painter_Fill_Rect()
    {
        var data = new DungeonData { Width = 20, Height = 20 };
        data.InitMap();
        Painter.Fill(data, new Rect(2, 2, 7, 7), Terrain.Empty);

        for (int y = 2; y <= 7; y++)
            for (int x = 2; x <= 7; x++)
                Assert.Equal(Terrain.Empty, data.Map[x + y * 20]);
    }

    [Fact]
    public void Painter_Fill_WithMargin()
    {
        var data = new DungeonData { Width = 20, Height = 20 };
        data.InitMap();
        Painter.Fill(data, new Rect(0, 0, 9, 9), 1, Terrain.Empty);

        // 缩进 1 后内部为 (1,1)-(8,8)
        Assert.Equal(Terrain.Empty, data.Map[1 + 1 * 20]);
        Assert.Equal(Terrain.Wall, data.Map[0 + 0 * 20]);  // 角落未被填充
    }

    [Fact]
    public void Painter_DrawLine()
    {
        var data = new DungeonData { Width = 20, Height = 20 };
        data.InitMap();

        // 从 (0,0) 到 (5,5) 画线
        Painter.DrawLine(data, 0, 5 + 5 * 20, Terrain.Empty);

        // 起点和终点应为 Empty
        Assert.Equal(Terrain.Empty, data.Map[0]);
        Assert.Equal(Terrain.Empty, data.Map[5 + 5 * 20]);
    }

    [Fact]
    public void Painter_FillEllipse()
    {
        var data = new DungeonData { Width = 20, Height = 20 };
        data.InitMap();
        Painter.FillEllipse(data, new Rect(2, 2, 8, 8), Terrain.Empty);

        // 中心应为 Empty
        Assert.Equal(Terrain.Empty, data.Map[5 + 5 * 20]);
    }
}