using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

/// <summary>
///     走廊房间——长条形，两端有门
/// </summary>
public sealed class HallwayRoom : StandardRoom
{
    public override float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);

        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }

        // 在走廊两端增加装饰柱
        if (Width > 4)
        {
            int midY = Top + Height / 2;
            Painter.Set(data, Left + 1, midY, Terrain.EmptyDeco);
            Painter.Set(data, Right - 1, midY, Terrain.EmptyDeco);
        }
    }
}