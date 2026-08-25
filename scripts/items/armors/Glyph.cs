using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.items.armors;

/// <summary>
///     护甲雕纹基类，对应原版 13 种雕纹
/// </summary>
public abstract class Glyph
{
    /// <summary>雕纹名称</summary>
    public abstract string Name { get; }

    /// <summary>
    ///     雕纹效果：受到攻击时触发
    /// </summary>
    public abstract int OnDefend(int damage, CharEntity attacker, CharEntity defender);
}

/// <summary>
///     反魔法雕纹
/// </summary>
public sealed class AntiMagicGlyph : Glyph
{
    public override string Name => "反魔法";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     硫磺雕纹
/// </summary>
public sealed class BrimstoneGlyph : Glyph
{
    public override string Name => "硫磺";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     厚重雕纹
/// </summary>
public sealed class BulkGlyph : Glyph
{
    public override string Name => "厚重";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => Math.Max(damage - 3, 0);
}

/// <summary>
///     石肤雕纹
/// </summary>
public sealed class StoneGlyph : Glyph
{
    public override string Name => "石肤";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => Math.Max(damage - 2, 0);
}

/// <summary>
///     荆棘雕纹
/// </summary>
public sealed class ThornsGlyph : Glyph
{
    public override string Name => "荆棘";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender)
    {
        // 反弹伤害
        if (attacker != null)
            attacker.Damage(damage / 3, defender);
        return damage;
    }
}

/// <summary>
///     粘稠雕纹
/// </summary>
public sealed class ViscosityGlyph : Glyph
{
    public override string Name => "粘稠";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     迷彩雕纹
/// </summary>
public sealed class CamouflageGlyph : Glyph
{
    public override string Name => "迷彩";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     纠缠雕纹
/// </summary>
public sealed class EntanglementGlyph : Glyph
{
    public override string Name => "纠缠";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     流动雕纹
/// </summary>
public sealed class FlowGlyph : Glyph
{
    public override string Name => "流动";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     模糊雕纹
/// </summary>
public sealed class ObfuscationGlyph : Glyph
{
    public override string Name => "模糊";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     潜能雕纹
/// </summary>
public sealed class PotentialGlyph : Glyph
{
    public override string Name => "潜能";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     排斥雕纹
/// </summary>
public sealed class RepulsionGlyph : Glyph
{
    public override string Name => "排斥";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     迅捷雕纹
/// </summary>
public sealed class SwiftnessGlyph : Glyph
{
    public override string Name => "迅捷";
    public override int OnDefend(int damage, CharEntity attacker, CharEntity defender) => damage;
}