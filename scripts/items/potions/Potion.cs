using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.items.potions;

/// <summary>
///     药水基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.potions.Potion
///     12 种药水
/// </summary>
public abstract class Potion : Item
{
    /// <summary>药水颜色（新游戏随机分配）</summary>
    public int Color { get; set; }

    /// <summary>是否已知道颜色对应关系</summary>
    public bool ColorKnown { get; set; }

    public override string DefaultAction => ItemAction.Drink;

    /// <summary>
    ///     执行药水效果
    /// </summary>
    public abstract void Apply(HeroEntity hero);
}

/// <summary>
///     治疗药水
/// </summary>
public sealed class HealingPotion : Potion
{
    public override string Name => "治疗药水";

    public override void Apply(HeroEntity hero)
    {
        int heal = 20 + hero.Level * 5;
        hero.Hp = Math.Min(hero.Hp + heal, hero.MaxHp);
    }
}

/// <summary>
///     生命活力药水（完全恢复）
/// </summary>
public sealed class VitalityPotion : Potion
{
    public override string Name => "生命活力药水";

    public override void Apply(HeroEntity hero)
    {
        hero.Hp = hero.MaxHp;
    }
}

/// <summary>
///     力量药水（永久 +1 力量）
/// </summary>
public sealed class StrengthPotion : Potion
{
    public override string Name => "力量药水";

    public override void Apply(HeroEntity hero)
    {
        hero.Strength++;
    }
}

/// <summary>
///     经验药水（获得等级经验）
/// </summary>
public sealed class ExperiencePotion : Potion
{
    public override string Name => "经验药水";

    public override void Apply(HeroEntity hero)
    {
        hero.Exp += 10;
    }
}

/// <summary>
///     隐身药水
/// </summary>
public sealed class InvisibilityPotion : Potion
{
    public override string Name => "隐身药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 添加隐身 Buff
    }
}

/// <summary>
///     火焰药水（投掷后爆炸）
/// </summary>
public sealed class LiquidFlamePotion : Potion
{
    public override string Name => "火焰药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 造成火焰伤害
    }
}

/// <summary>
///     冰冻药水（投掷后冻结）
/// </summary>
public sealed class FrostPotion : Potion
{
    public override string Name => "冰冻药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 施加冰冻效果
    }
}

/// <summary>
///     麻痹药水
/// </summary>
public sealed class ParalysisPotion : Potion
{
    public override string Name => "麻痹药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 施加麻痹 Buff
    }
}

/// <summary>
///     净化药水（解除所有负面效果）
/// </summary>
public sealed class PurifyPotion : Potion
{
    public override string Name => "净化药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 移除所有负面 Buff
    }
}

/// <summary>
///     疾跑药水
/// </summary>
public sealed class HastePotion : Potion
{
    public override string Name => "疾跑药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 施加疾跑 Buff
    }
}

/// <summary>
///     思维诱导药水（重置天赋）
/// </summary>
public sealed class MindVisionPotion : Potion
{
    public override string Name => "思维诱导药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 显示全图怪物位置
    }
}

/// <summary>
///     毒性瓦斯药水（投掷后释放毒气）
/// </summary>
public sealed class ToxicGasPotion : Potion
{
    public override string Name => "毒性瓦斯药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 释放毒气区域效果
    }
}

/// <summary>
///     传送药水
/// </summary>
public sealed class TeleportPotion : Potion
{
    public override string Name => "传送药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 随机传送
    }
}