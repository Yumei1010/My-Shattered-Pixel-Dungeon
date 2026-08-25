namespace MyShatteredPixelDungeon.scripts.items.wands;

/// <summary>
///     法杖基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.wands.Wand
///     13 种法杖
/// </summary>
public abstract class Wand : EquipableItem
{
    /// <summary>当前充能</summary>
    public int CurrentCharges { get; set; } = 2;

    /// <summary>最大充能</summary>
    public int MaxCharges { get; set; } = 2;

    /// <summary>充能是否已知</summary>
    public bool ChargesKnown { get; set; }

    public override string DefaultAction => ItemAction.Zap;
    public override bool UsesTargeting => true;
    public override bool IsUpgradable => true;

    /// <summary>
    ///     充能（每回合恢复）
    /// </summary>
    public virtual void Recharge()
    {
        if (CurrentCharges < MaxCharges)
            CurrentCharges++;
    }

    /// <summary>
    ///     消耗充能
    /// </summary>
    public virtual bool ConsumeCharge()
    {
        if (CurrentCharges <= 0) return false;
        CurrentCharges--;
        return true;
    }

    /// <summary>
    ///     发射法杖
    /// </summary>
    public virtual void Zap(int targetPos) { }
}

/// <summary>
///     魔法飞弹法杖（基础法杖）
/// </summary>
public sealed class MagicMissileWand : Wand
{
    public override string Name => "魔法飞弹法杖";
}

/// <summary>
///     火焰爆轰法杖
/// </summary>
public sealed class FireblastWand : Wand
{
    public override string Name => "火焰爆轰法杖";
}

/// <summary>
///     冰冻法杖
/// </summary>
public sealed class FrostWand : Wand
{
    public override string Name => "冰冻法杖";
}

/// <summary>
///     闪电法杖
/// </summary>
public sealed class LightningWand : Wand
{
    public override string Name => "闪电法杖";
}

/// <summary>
///     冲击波法杖
/// </summary>
public sealed class BlastWaveWand : Wand
{
    public override string Name => "冲击波法杖";
}

/// <summary>
///     崩解法杖
/// </summary>
public sealed class DisintegrationWand : Wand
{
    public override string Name => "崩解法杖";
}

/// <summary>
///     腐蚀法杖
/// </summary>
public sealed class CorrosionWand : Wand
{
    public override string Name => "腐蚀法杖";
}

/// <summary>
///     腐化法杖
/// </summary>
public sealed class CorruptionWand : Wand
{
    public override string Name => "腐化法杖";
}

/// <summary>
///     再生法杖
/// </summary>
public sealed class RegrowthWand : Wand
{
    public override string Name => "再生法杖";
}

/// <summary>
///     棱彩之光法杖
/// </summary>
public sealed class PrismaticLightWand : Wand
{
    public override string Name => "棱彩之光法杖";
}

/// <summary>
///     输血法杖
/// </summary>
public sealed class TransfusionWand : Wand
{
    public override string Name => "输血法杖";
}

/// <summary>
///     大地生命法杖
/// </summary>
public sealed class LivingEarthWand : Wand
{
    public override string Name => "大地生命法杖";
}

/// <summary>
///     守护法杖
/// </summary>
public sealed class WardingWand : Wand
{
    public override string Name => "守护法杖";
}