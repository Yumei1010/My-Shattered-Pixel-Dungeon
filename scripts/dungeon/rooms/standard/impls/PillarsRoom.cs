using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

/// <summary>
///     柱子房间——房间内有四根柱子（装饰墙）
/// </summary>
public sealed class PillarsRoom : StandardRoom
{
    public override float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);

        // 在房间内部对称放置四根柱子（至少需要 6x6 空间）
        if (Width >= 6 && Height >= 6)
        {
            int pillarX1 = Left + 2;
            int pillarX2 = Right - 2;
            int pillarY1 = Top + 2;
            int pillarY2 = Bottom - 2;

            Painter.Set(data, pillarX1, pillarY1, Terrain.Wall);
            Painter.Set(data, pillarX2, pillarY1, Terrain.Wall);
            Painter.Set(data, pillarX1, pillarY2, Terrain.Wall);
            Painter.Set(data, pillarX2, pillarY2, Terrain.Wall);
        }

        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }
    }
}