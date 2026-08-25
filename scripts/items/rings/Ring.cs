namespace MyShatteredPixelDungeon.scripts.items.rings;

/// <summary>
///     戒指基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.rings.Ring
///     12 种戒指，提供被动属性加成
/// </summary>
public abstract class Ring : EquipableItem
{
    /// <summary>戒指等级（影响加成幅度）</summary>
    public int RingLevel => Level;

    public override bool IsUpgradable => true;
    public override string DefaultAction => ItemAction.Wear;

    /// <summary>
    ///     获取加成值（基于等级）
    /// </summary>
    public virtual int BuffValue => RingLevel;

    /// <summary>
    ///     获取加成百分比（基于等级）
    /// </summary>
    public virtual float BuffPercent => 1f + RingLevel * 0.1f;
}

/// <summary>
///     精准戒指（提高命中）
/// </summary>
public sealed class RingOfAccuracy : Ring
{
    public override string Name => "精准戒指";
}

/// <summary>
///     秘术戒指（提高法杖充能速度）
/// </summary>
public sealed class RingOfArcana : Ring
{
    public override string Name => "秘术戒指";
}

/// <summary>
///     元素戒指（提高元素抗性）
/// </summary>
public sealed class RingOfElements : Ring
{
    public override string Name => "元素戒指";
}

/// <summary>
///     能量戒指（提高法杖最大充能）
/// </summary>
public sealed class RingOfEnergy : Ring
{
    public override string Name => "能量戒指";
}

/// <summary>
///     闪避戒指
/// </summary>
public sealed class RingOfEvasion : Ring
{
    public override string Name => "闪避戒指";
}

/// <summary>
///     力量戒指（提高攻击力）
/// </summary>
public sealed class RingOfForce : Ring
{
    public override string Name => "力量戒指";
}

/// <summary>
///     狂怒戒指（提高攻击速度）
/// </summary>
public sealed class RingOfFuror : Ring
{
    public override string Name => "狂怒戒指";
}

/// <summary>
///     急速戒指（提高移动速度）
/// </summary>
public sealed class RingOfHaste : Ring
{
    public override string Name => "急速戒指";
}

/// <summary>
///     威力戒指（提高力量）
/// </summary>
public sealed class RingOfMight : Ring
{
    public override string Name => "威力戒指";
}

/// <summary>
///     精准射击戒指（提高远程命中）
/// </summary>
public sealed class RingOfSharpshooting : Ring
{
    public override string Name => "精准射击戒指";
}

/// <summary>
///     坚韧戒指（提高减伤）
/// </summary>
public sealed class RingOfTenacity : Ring
{
    public override string Name => "坚韧戒指";
}

/// <summary>
///     财富戒指（提高金币掉落）
/// </summary>
public sealed class RingOfWealth : Ring
{
    public override string Name => "财富戒指";
}