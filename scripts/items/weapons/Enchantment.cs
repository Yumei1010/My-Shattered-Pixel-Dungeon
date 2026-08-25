using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.items.weapons;

/// <summary>
///     武器附魔基类，对应原版 13 种附魔
/// </summary>
public abstract class Enchantment
{
    /// <summary>附魔名称</summary>
    public abstract string Name { get; }

    /// <summary>
    ///     附魔效果：攻击时触发
    /// </summary>
    public abstract int OnAttack(int damage, CharEntity attacker, CharEntity defender);
}

/// <summary>
///     灼烧附魔
/// </summary>
public sealed class BlazingEnchantment : Enchantment
{
    public override string Name => "灼烧";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage + 2;
}

/// <summary>
///     冰冻附魔
/// </summary>
public sealed class ChillingEnchantment : Enchantment
{
    public override string Name => "冰冻";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage + 1;
}

/// <summary>
///     闪电附魔
/// </summary>
public sealed class ShockingEnchantment : Enchantment
{
    public override string Name => "闪电";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage + 3;
}

/// <summary>
///     致死附魔
/// </summary>
public sealed class GrimEnchantment : Enchantment
{
    public override string Name => "致死";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage * 2;
}

/// <summary>
///     动能附魔
/// </summary>
public sealed class KineticEnchantment : Enchantment
{
    public override string Name => "动能";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage + 1;
}

/// <summary>
///     幸运附魔
/// </summary>
public sealed class LuckyEnchantment : Enchantment
{
    public override string Name => "幸运";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     繁花附魔
/// </summary>
public sealed class BloomingEnchantment : Enchantment
{
    public override string Name => "繁花";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     吸血附魔
/// </summary>
public sealed class VampiricEnchantment : Enchantment
{
    public override string Name => "吸血";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender)
    {
        attacker.Hp = Math.Min(attacker.Hp + damage / 2, attacker.MaxHp);
        return damage;
    }
}

/// <summary>
///     弹性附魔
/// </summary>
public sealed class ElasticEnchantment : Enchantment
{
    public override string Name => "弹性";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     投射附魔
/// </summary>
public sealed class ProjectingEnchantment : Enchantment
{
    public override string Name => "投射";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage;
}

/// <summary>
///     格挡附魔
/// </summary>
public sealed class BlockingEnchantment : Enchantment
{
    public override string Name => "格挡";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => Math.Max(damage - 2, 0);
}

/// <summary>
///     腐化附魔
/// </summary>
public sealed class CorruptingEnchantment : Enchantment
{
    public override string Name => "腐化";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage + 1;
}

/// <summary>
///     击晕附魔
/// </summary>
public sealed class StunningEnchantment : Enchantment
{
    public override string Name => "击晕";
    public override int OnAttack(int damage, CharEntity attacker, CharEntity defender) => damage;
}