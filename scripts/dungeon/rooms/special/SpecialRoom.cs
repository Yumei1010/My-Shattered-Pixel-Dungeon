using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.special;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.special;

/// <summary>
///     商店房间（占位实现，后续完善）
/// </summary>
public class ShopRoom : SpecialRoom
{
    public override int MaxConnections(int direction) => direction == Room.DirAll ? 1 : 0;

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);
        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }
    }
}

/// <summary>
///     特殊房间基类，对应原版 SpecialRoom
/// </summary>
public abstract class SpecialRoom : Room
{
    public override int MinWidth() => 5;
    public override int MaxWidth() => 10;
    public override int MinHeight() => 5;
    public override int MaxHeight() => 10;
}

/// <summary>
///     秘密房间基类，对应原版 SecretRoom
/// </summary>
public abstract class SecretRoom : Room
{
    public override int MinWidth() => 4;
    public override int MaxWidth() => 8;
    public override int MinHeight() => 4;
    public override int MaxHeight() => 8;
}