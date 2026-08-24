using MyShatteredPixelDungeon.scripts.dungeon.builders;
using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;
using MyShatteredPixelDungeon.scripts.dungeon.rooms.standard.impls;

namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     LevelGenerator 管线入口，对应原版 Level.create()
///     接受 depth 和 seed，生成完整的 DungeonData
/// </summary>
public sealed class LevelGenerator
{
    private readonly int _depth;
    private readonly long _seed;

    private DungeonData _data = null!;
    private List<Room> _rooms = null!;

    public LevelGenerator(int depth, long seed)
    {
        _depth = depth;
        _seed = seed;
    }

    public DungeonData Generate()
    {
        _data = new DungeonData { Depth = _depth };
        long depthSeed = SeedForDepth(_depth, 0, _seed);

        using (DeterministicRng.WithSeed(depthSeed))
        {
            _data.Width = 64;
            _data.Height = 64;
            _data.InitMap(Terrain.Wall);

            List<Room>? result = null;
            for (int attempts = 0; attempts < 100; attempts++)
            {
                _rooms = InitRooms();
                var builder = SelectBuilder();
                result = builder.Build(_rooms);
                if (result != null) break;
            }

            _rooms = result ?? BuildFallbackLayout();
            OffsetRoomsToMap();

            foreach (var room in _rooms) room.Paint(_data);
            PaintDoors();
            BuildFlagMaps();
            SetupTransitions();
        }

        return _data;
    }

    private List<Room> BuildFallbackLayout()
    {
        var entrance = new EntranceRoom();
        var exit = new ExitRoom();
        entrance.SetSize();
        entrance.SetPos(0, 0);
        exit.SetSize();
        exit.SetPos(0, entrance.Height); // 直接相邻，不设间隙
        entrance.AddNeighbour(exit);
        entrance.Connect(exit);
        return new List<Room> { entrance, exit };
    }

    private void OffsetRoomsToMap()
    {
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var r in _rooms)
        {
            minX = Math.Min(minX, r.Left); minY = Math.Min(minY, r.Top);
            maxX = Math.Max(maxX, r.Right); maxY = Math.Max(maxY, r.Bottom);
        }
        int margin = 2, dx = margin - minX, dy = margin - minY;
        if (maxX + dx > _data.Width - margin) dx = _data.Width - margin - maxX;
        if (maxY + dy > _data.Height - margin) dy = _data.Height - margin - maxY;
        foreach (var r in _rooms) r.Shift(dx, dy);
    }

    private static List<Room> InitRooms()
    {
        var rooms = new List<Room> { new EntranceRoom(), new ExitRoom() };
        int count = 5 + DeterministicRng.Range(5);
        for (int i = 0; i < count; i++) rooms.Add(CreateStandardRoom());
        return rooms;
    }

    private static StandardRoom CreateStandardRoom() => DeterministicRng.Range(5) switch
    {
        0 => new EmptyRoom(), 1 => new HallwayRoom(), 2 => new PillarsRoom(),
        3 => new StudyRoom(), _ => new StripedRoom()
    };

    private static Builder SelectBuilder()
    {
        return new LoopBuilder().SetLoopShape(2, DeterministicRng.Float() * 0.65f, DeterministicRng.Float() * 0.50f);
    }

    private void PaintDoors()
    {
        foreach (var room in _rooms)
        {
            foreach (var (other, door) in room.Connected)
            {
                var intersect = room.Intersect(other);
                if (intersect.IsEmpty) continue;
                int cx = (intersect.Left + intersect.Right) / 2;
                int cy = (intersect.Top + intersect.Bottom) / 2;
                int cell = _data.PointToCell(new Point(cx, cy));
                if (cell < 0 || cell >= _data.Length) continue;
                _data.Map[cell] = door.Type switch
                {
                    DoorType.Regular => Terrain.Door, DoorType.Hidden => Terrain.SecretDoor,
                    DoorType.Locked => Terrain.LockedDoor, DoorType.Barricade => Terrain.Barricade,
                    DoorType.Tunnel => Terrain.Empty, _ => Terrain.Door
                };
                _data.Doors[cell] = door;
            }
        }
    }

    private void BuildFlagMaps()
    {
        int len = _data.Length;
        _data.Passable = new bool[len]; _data.LosBlocking = new bool[len]; _data.Solid = new bool[len];
        _data.Flammable = new bool[len]; _data.Avoid = new bool[len]; _data.Water = new bool[len]; _data.Pit = new bool[len];
        for (int i = 0; i < len; i++)
        {
            int flags = (int)Terrain.Flags[_data.Map[i]];
            _data.Passable[i] = (flags & (int)TileFlags.Passable) != 0;
            _data.LosBlocking[i] = (flags & (int)TileFlags.LosBlocking) != 0;
            _data.Solid[i] = (flags & (int)TileFlags.Solid) != 0;
            _data.Flammable[i] = (flags & (int)TileFlags.Flammable) != 0;
            _data.Avoid[i] = (flags & (int)TileFlags.Avoid) != 0;
            _data.Water[i] = (flags & (int)TileFlags.Liquid) != 0;
            _data.Pit[i] = (flags & (int)TileFlags.Pit) != 0;
        }
    }

    private void SetupTransitions()
    {
        if (_data.Entrance == 0)
            for (int i = 0; i < _data.Length; i++)
                if (_data.Map[i] == Terrain.Entrance) { _data.Entrance = i; break; }
        if (_data.Exit == 0)
            for (int i = 0; i < _data.Length; i++)
                if (_data.Map[i] == Terrain.Exit) { _data.Exit = i; break; }
    }

    private static long SeedForDepth(int depth, int branch, long globalSeed)
    {
        int lookAhead = depth + 30 * branch;
        var rng = new System.Random((int)globalSeed);
        for (int i = 0; i < lookAhead; i++) rng.Next();
        return rng.NextInt64();
    }
}