namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     地形标志位枚举，对应原版 Terrain 的位掩码常量
///     通过 [Flags] 组合使用，描述地形的通行/视线/交互等属性
/// </summary>
[Flags]
public enum TileFlags
{
    /// <summary>
    ///     无属性
    /// </summary>
    None = 0,

    /// <summary>
    ///     可通过（角色可以走上去）
    /// </summary>
    Passable = 0x01,

    /// <summary>
    ///     阻挡视线（影响 FOV 计算）
    /// </summary>
    LosBlocking = 0x02,

    /// <summary>
    ///     可燃（可被点燃）
    /// </summary>
    Flammable = 0x04,

    /// <summary>
    ///     隐藏（密门/隐藏陷阱）
    /// </summary>
    Secret = 0x08,

    /// <summary>
    ///     固体（完全阻挡移动）
    /// </summary>
    Solid = 0x10,

    /// <summary>
    ///     回避（怪物寻路时尽量避免）
    /// </summary>
    Avoid = 0x20,

    /// <summary>
    ///     液体（水等，影响行走效果）
    /// </summary>
    Liquid = 0x40,

    /// <summary>
    ///     深坑（掉落伤害）
    /// </summary>
    Pit = 0x80
}