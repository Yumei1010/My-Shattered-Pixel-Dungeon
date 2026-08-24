using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.connection;

/// <summary>
///     连接房间（走廊），对应原版 ConnectionRoom
///     用于在房间之间创建通道，通常由 Builder 自动创建
/// </summary>
public class ConnectionRoom : Room
{
    public override int MinWidth() => 3;
    public override int MaxWidth() => 10;
    public override int MinHeight() => 3;
    public override int MaxHeight() => 10;

    /// <summary>
    ///     创建连接房间实例（随机尺寸）
    /// </summary>
    public static ConnectionRoom Create()
    {
        var room = new ConnectionRoom();
        room.SetSize();
        return room;
    }

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);
        // 连接房间的所有门都是隧道（无门，开放通道）
        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Tunnel);
        }
    }
}

/// <summary>
///     迷宫连接房间，对应原版 MazeConnectionRoom
///     用于隐藏房间的连接（更曲折）
/// </summary>
public class MazeConnectionRoom : ConnectionRoom
{
    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);
        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Tunnel);
        }
    }
}