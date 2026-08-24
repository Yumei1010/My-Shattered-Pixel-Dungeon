using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;

namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     地牢生成数据，纯数据容器（可序列化，无 Godot 依赖）
///     LevelGenerator 生成后由 Godot 层渲染
/// </summary>
public sealed class DungeonData
{
    /// <summary>地图宽度</summary>
    public int Width { get; set; }

    /// <summary>地图高度</summary>
    public int Height { get; set; }

    /// <summary>格子总数</summary>
    public int Length => Width * Height;

    /// <summary>地形数组（索引 = x + y * Width）</summary>
    public int[] Map { get; set; } = Array.Empty<int>();

    /// <summary>本层深度（1-26）</summary>
    public int Depth { get; set; }

    /// <summary>入口格子</summary>
    public int Entrance { get; set; }

    /// <summary>出口格子</summary>
    public int Exit { get; set; }

    /// <summary>房间列表（生成后保留引用）</summary>
    public List<Room> Rooms { get; set; } = new();

    /// <summary>门的列表（key = 门位置 cell）</summary>
    public Dictionary<int, Door> Doors { get; set; } = new();

    /// <summary>可通过标志数组</summary>
    public bool[] Passable { get; set; } = Array.Empty<bool>();

    /// <summary>阻挡视线标志数组</summary>
    public bool[] LosBlocking { get; set; } = Array.Empty<bool>();

    /// <summary>固体标志数组</summary>
    public bool[] Solid { get; set; } = Array.Empty<bool>();

    /// <summary>可燃标志数组</summary>
    public bool[] Flammable { get; set; } = Array.Empty<bool>();

    /// <summary>回避标志数组</summary>
    public bool[] Avoid { get; set; } = Array.Empty<bool>();

    /// <summary>水标志数组</summary>
    public bool[] Water { get; set; } = Array.Empty<bool>();

    /// <summary>深坑标志数组</summary>
    public bool[] Pit { get; set; } = Array.Empty<bool>();

    /// <summary>
    ///     单元格 → 坐标
    /// </summary>
    public Point CellToPoint(int cell) => new(cell % Width, cell / Width);

    /// <summary>
    ///     坐标 → 单元格
    /// </summary>
    public int PointToCell(Point p) => p.X + p.Y * Width;

    /// <summary>
    ///     检查坐标是否在地图内
    /// </summary>
    public bool InsideMap(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    /// <summary>
    ///     初始化地图（全部填充为指定地形）
    /// </summary>
    public void InitMap(int fillTerrain = Terrain.Wall)
    {
        Map = new int[Length];
        Array.Fill(Map, fillTerrain);
    }
}