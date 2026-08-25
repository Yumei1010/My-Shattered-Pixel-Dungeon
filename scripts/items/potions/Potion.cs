using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.entities.buffs;

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

    /// <summary>
    ///     使用药水（消耗时间 + 应用效果）
    /// </summary>
    public override void DoThrow(HeroEntity hero)
    {
        // 投掷时在目标位置释放效果
        Apply(hero);
    }

    /// <summary>
    ///     标记药水为已鉴定
    /// </summary>
    protected void MarkIdentified()
    {
        Identify();
    }
}

/// <summary>
///     治疗药水 — 恢复 HP = 20 + 等级 × 5
/// </summary>
public sealed class HealingPotion : Potion
{
    public override string Name => "治疗药水";

    public override void Apply(HeroEntity hero)
    {
        int heal = 20 + hero.Level * 5;
        hero.Hp = Math.Min(hero.Hp + heal, hero.MaxHp);
        MarkIdentified();
    }
}

/// <summary>
///     生命活力药水 — 完全恢复 HP
/// </summary>
public sealed class VitalityPotion : Potion
{
    public override string Name => "生命活力药水";

    public override void Apply(HeroEntity hero)
    {
        hero.Hp = hero.MaxHp;
        MarkIdentified();
    }
}

/// <summary>
///     力量药水 — 永久 +1 力量
/// </summary>
public sealed class StrengthPotion : Potion
{
    public override string Name => "力量药水";

    public override void Apply(HeroEntity hero)
    {
        hero.Strength++;
        MarkIdentified();
    }
}

/// <summary>
///     经验药水 — 获得经验
/// </summary>
public sealed class ExperiencePotion : Potion
{
    public override string Name => "经验药水";

    public override void Apply(HeroEntity hero)
    {
        hero.Exp += 10;
        MarkIdentified();
    }
}

/// <summary>
///     隐身药水 — 施加隐身效果
/// </summary>
public sealed class InvisibilityPotion : Potion
{
    public override string Name => "隐身药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 添加隐身 Buff
        MarkIdentified();
    }
}

/// <summary>
///     火焰药水 — 投掷后造成火焰伤害
/// </summary>
public sealed class LiquidFlamePotion : Potion
{
    public override string Name => "火焰药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 对目标位置造成火焰伤害
        MarkIdentified();
    }
}

/// <summary>
///     冰冻药水 — 投掷后冻结目标
/// </summary>
public sealed class FrostPotion : Potion
{
    public override string Name => "冰冻药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 施加冰冻效果（减速 + 冻结）
        MarkIdentified();
    }
}

/// <summary>
///     麻痹药水 — 施加麻痹 Buff
/// </summary>
public sealed class ParalysisPotion : Potion
{
    public override string Name => "麻痹药水";

    public override void Apply(HeroEntity hero)
    {
        Buff.Prolong<ParalysisBuff>(hero, 10f);
        MarkIdentified();
    }
}

/// <summary>
///     净化药水 — 移除所有负面 Buff
/// </summary>
public sealed class PurifyPotion : Potion
{
    public override string Name => "净化药水";

    public override void Apply(HeroEntity hero)
    {
        // 移除所有负面 Buff
        var debuffs = hero.Buffs.Where(b => b.Type == BuffType.Negative).ToList();
        foreach (var debuff in debuffs)
        {
            hero.RemoveBuff(debuff);
        }
        MarkIdentified();
    }
}

/// <summary>
///     疾跑药水 — 临时提高速度
/// </summary>
public sealed class HastePotion : Potion
{
    public override string Name => "疾跑药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 施加疾跑 Buff（速度 × 2）
        MarkIdentified();
    }
}

/// <summary>
///     思维诱导药水 — 显示全图怪物位置
/// </summary>
public sealed class MindVisionPotion : Potion
{
    public override string Name => "思维诱导药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 显示全图怪物位置标记
        MarkIdentified();
    }
}

/// <summary>
///     毒性瓦斯药水 — 投掷后释放毒气区域
/// </summary>
public sealed class ToxicGasPotion : Potion
{
    public override string Name => "毒性瓦斯药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 在目标位置释放毒气 Blob
        MarkIdentified();
    }
}

/// <summary>
///     传送药水 — 随机传送
/// </summary>
public sealed class TeleportPotion : Potion
{
    public override string Name => "传送药水";

    public override void Apply(HeroEntity hero)
    {
        // TODO: 随机传送到地牢中其他位置
        MarkIdentified();
    }
}