using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

/// <summary>
///     条纹房间——交替的地板砖条纹装饰
/// </summary>
public sealed class StripedRoom : StandardRoom
{
    public override float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);

        // 绘制交替条纹（水平或垂直）
        bool horizontal = DeterministicRng.Range(2) == 0;

        if (horizontal)
        {
            for (int y = Top + 2; y < Bottom; y += 2)
            {
                for (int x = Left + 1; x < Right; x++)
                {
                    Painter.Set(data, x, y, Terrain.EmptyDeco);
                }
            }
        }
        else
        {
            for (int x = Left + 2; x < Right; x += 2)
            {
                for (int y = Top + 1; y < Bottom; y++)
                {
                    Painter.Set(data, x, y, Terrain.EmptyDeco);
                }
            }
        }

        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }
    }
}