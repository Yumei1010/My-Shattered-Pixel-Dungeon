using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

/// <summary>
///     书房房间——有书架装饰的安静房间
/// </summary>
public sealed class StudyRoom : StandardRoom
{
    public override float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);

        // 在墙壁内沿放置书架（随机选 1-2 面墙）
        bool topBookshelf = System.Random.Shared.Next(2) == 0;
        bool bottomBookshelf = System.Random.Shared.Next(2) == 0;
        bool leftBookshelf = System.Random.Shared.Next(2) == 0;
        bool rightBookshelf = System.Random.Shared.Next(2) == 0;

        // 至少有一面墙有书架
        if (!topBookshelf && !bottomBookshelf && !leftBookshelf && !rightBookshelf)
            topBookshelf = true;

        if (topBookshelf && Height > 2)
            for (int x = Left + 1; x < Right; x++)
                Painter.Set(data, x, Top + 1, Terrain.Bookshelf);

        if (bottomBookshelf && Height > 2)
            for (int x = Left + 1; x < Right; x++)
                Painter.Set(data, x, Bottom - 1, Terrain.Bookshelf);

        if (leftBookshelf && Width > 2)
            for (int y = Top + 1; y < Bottom; y++)
                Painter.Set(data, Left + 1, y, Terrain.Bookshelf);

        if (rightBookshelf && Width > 2)
            for (int y = Top + 1; y < Bottom; y++)
                Painter.Set(data, Right - 1, y, Terrain.Bookshelf);

        // 中央放置一张桌子（基座）
        if (Width >= 4 && Height >= 4)
        {
            var center = Center();
            Painter.Set(data, center.X, center.Y, Terrain.Pedestal);
        }

        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }
    }
}