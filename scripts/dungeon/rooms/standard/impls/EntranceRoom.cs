using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

/// <summary>
///     入口房间，对应原版 EntranceRoom
///     放置玩家出生点和入口楼梯
/// </summary>
public sealed class EntranceRoom : StandardRoom
{
    public override int MinWidth() => Math.Max(base.MinWidth(), 5);
    public override int MinHeight() => Math.Max(base.MinHeight(), 5);

    public override bool IsEntrance() => true;

    public override float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);

        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }

        // 入口放在房间内部随机位置
        var entrancePoint = Random(2);
        int entrance = data.PointToCell(entrancePoint);
        data.Map[entrance] = Terrain.Entrance;
        data.Entrance = entrance;
    }
}

/// <summary>
///     出口房间，对应原版 ExitRoom
///     放置下楼楼梯
/// </summary>
public sealed class ExitRoom : StandardRoom
{
    public override int MinWidth() => Math.Max(base.MinWidth(), 5);
    public override int MinHeight() => Math.Max(base.MinHeight(), 5);

    public override bool IsExit() => true;

    public override float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    public override void Paint(DungeonData data)
    {
        Painter.Fill(data, this, Terrain.Wall);
        Painter.Fill(data, this, 1, Terrain.Empty);

        foreach (var door in Connected.Values)
        {
            door.Set(DoorType.Regular);
        }

        // 出口放在房间内部随机位置
        var exitPoint = Random(2);
        int exit = data.PointToCell(exitPoint);
        data.Map[exit] = Terrain.Exit;
        data.Exit = exit;
    }
}