namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     地形常量定义，对应原版 com.shatteredpixel.shatteredpixeldungeon.levels.Terrain
///     每个地形类型对应一个整数 ID，用于 map[] 数组存储
/// </summary>
public static class Terrain
{
    public const int Chasm = 0;
    public const int Empty = 1;
    public const int Grass = 2;
    public const int EmptyWell = 3;
    public const int Wall = 4;
    public const int Door = 5;
    public const int OpenDoor = 6;
    public const int Entrance = 7;
    public const int EntranceSp = 37;
    public const int Exit = 8;
    public const int Embers = 9;
    public const int LockedDoor = 10;
    public const int HeroLkdDr = 38;
    public const int CrystalDoor = 31;
    public const int Pedestal = 11;
    public const int WallDeco = 12;
    public const int Barricade = 13;
    public const int EmptySp = 14;
    public const int HighGrass = 15;
    public const int FurrowedGrass = 30;
    public const int SecretDoor = 16;
    public const int SecretTrap = 17;
    public const int Trap = 18;
    public const int InactiveTrap = 19;
    public const int EmptyDeco = 20;
    public const int LockedExit = 21;
    public const int UnlockedExit = 22;
    public const int Well = 24;
    public const int Bookshelf = 27;
    public const int Alchemy = 28;
    public const int CustomDecoEmpty = 32;
    public const int CustomDeco = 23;
    public const int Statue = 25;
    public const int StatueSp = 26;
    public const int RegionDeco = 33;
    public const int RegionDecoAlt = 34;
    public const int MineCrystal = 35;
    public const int MineBoulder = 36;
    public const int Water = 29;

    /// <summary>
    ///     根据地形类型获取发现后的地形（用于隐藏门/陷阱）
    /// </summary>
    public static int Discover(int terr) => terr switch
    {
        SecretDoor => Door,
        SecretTrap => Trap,
        _ => terr
    };

    /// <summary>
    ///     是否为可通过的地形
    /// </summary>
    public static bool IsPassable(int terr) => (Flags[terr] & TileFlags.Passable) != 0;

    /// <summary>
    ///     是否为阻挡视线的地形
    /// </summary>
    public static bool IsLosBlocking(int terr) => (Flags[terr] & TileFlags.LosBlocking) != 0;

    /// <summary>
    ///     是否为固体
    /// </summary>
    public static bool IsSolid(int terr) => (Flags[terr] & TileFlags.Solid) != 0;

    /// <summary>
    ///     是否为可燃
    /// </summary>
    public static bool IsFlammable(int terr) => (Flags[terr] & TileFlags.Flammable) != 0;

    /// <summary>
    ///     是否为隐藏
    /// </summary>
    public static bool IsSecret(int terr) => (Flags[terr] & TileFlags.Secret) != 0;

    /// <summary>
    ///     是否为回避（怪物避免走）
    /// </summary>
    public static bool IsAvoid(int terr) => (Flags[terr] & TileFlags.Avoid) != 0;

    /// <summary>
    ///     是否为液体（水）
    /// </summary>
    public static bool IsLiquid(int terr) => (Flags[terr] & TileFlags.Liquid) != 0;

    /// <summary>
    ///     是否为深坑
    /// </summary>
    public static bool IsPit(int terr) => (Flags[terr] & TileFlags.Pit) != 0;

    /// <summary>
    ///     地形标志数组，按地形 ID 索引，每个值为 TileFlags 位组合
    /// </summary>
    public static readonly TileFlags[] Flags = new TileFlags[256];

    static Terrain()
    {
        // 初始化所有地形标志
        Flags[Chasm] = TileFlags.Avoid | TileFlags.Pit;
        Flags[Empty] = TileFlags.Passable;
        Flags[Grass] = TileFlags.Passable | TileFlags.Flammable;
        Flags[EmptyWell] = TileFlags.Passable;
        Flags[Water] = TileFlags.Passable | TileFlags.Liquid;
        Flags[Wall] = TileFlags.LosBlocking | TileFlags.Solid;
        Flags[Door] = TileFlags.Passable | TileFlags.LosBlocking | TileFlags.Flammable | TileFlags.Solid;
        Flags[OpenDoor] = TileFlags.Passable | TileFlags.Flammable;
        Flags[Entrance] = TileFlags.Passable;
        Flags[EntranceSp] = Flags[Entrance];
        Flags[Exit] = TileFlags.Passable;
        Flags[Embers] = TileFlags.Passable;
        Flags[LockedDoor] = TileFlags.LosBlocking | TileFlags.Solid;
        Flags[HeroLkdDr] = Flags[LockedDoor];
        Flags[CrystalDoor] = TileFlags.Solid;
        Flags[Pedestal] = TileFlags.Passable;
        Flags[WallDeco] = Flags[Wall];
        Flags[Barricade] = TileFlags.Flammable | TileFlags.Solid | TileFlags.LosBlocking;
        Flags[EmptySp] = Flags[Empty];
        Flags[HighGrass] = TileFlags.Passable | TileFlags.LosBlocking | TileFlags.Flammable;
        Flags[FurrowedGrass] = Flags[HighGrass];
        Flags[SecretDoor] = Flags[Wall] | TileFlags.Secret;
        Flags[SecretTrap] = Flags[Empty] | TileFlags.Secret;
        Flags[Trap] = TileFlags.Avoid;
        Flags[InactiveTrap] = Flags[Empty];
        Flags[EmptyDeco] = Flags[Empty];
        Flags[LockedExit] = TileFlags.Solid;
        Flags[UnlockedExit] = TileFlags.Passable;
        Flags[Well] = TileFlags.Avoid;
        Flags[Bookshelf] = Flags[Barricade];
        Flags[Alchemy] = TileFlags.Solid;
        Flags[CustomDecoEmpty] = Flags[Empty];
        Flags[CustomDeco] = TileFlags.Solid;
        Flags[Statue] = TileFlags.Solid;
        Flags[StatueSp] = Flags[Statue];
        Flags[RegionDeco] = Flags[Statue];
        Flags[RegionDecoAlt] = Flags[StatueSp];
        Flags[MineCrystal] = TileFlags.Solid;
        Flags[MineBoulder] = TileFlags.Solid;
    }
}