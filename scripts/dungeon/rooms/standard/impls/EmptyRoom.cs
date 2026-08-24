using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

/// <summary>
///     空地房间——最简单的基础房间，内部全为空地
/// </summary>
public sealed class EmptyRoom : StandardRoom
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
    }
}