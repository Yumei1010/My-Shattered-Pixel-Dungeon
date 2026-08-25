using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.entities.buffs;

namespace MyShatteredPixelDungeon.scripts.items.potions;

/// <summary>
///     药水基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.potions.Potion
///     12 种药水，未鉴定时显示颜色名，已鉴定显示真名
/// </summary>
public abstract class Potion : Item
{
    /// <summary>药水颜色（新游戏随机分配）</summary>
    public int Color { get; set; }

    /// <summary>是否已知道颜色对应关系</summary>
    public bool ColorKnown { get; set; }

    public override string DefaultAction => ItemAction.Drink;

    /// <summary>
    ///     显示名称（未鉴定时显示颜色名，已鉴定显示真名）
    /// </summary>
    public override string Name
    {
        get
        {
            if (IsIdentified)
                return TrueName;
            string colorName = IdentificationSystem.GetPotionColorName(this);
            return $"{colorName}色药水";
        }
    }

    /// <summary>
    ///     真实名称
    /// </summary>
    protected abstract string TrueName { get; }

    /// <summary>
    ///     执行药水效果
    /// </summary>
    public abstract void Apply(HeroEntity hero);

    /// <summary>
    ///     使用药水（消耗时间 + 应用效果）
    /// </summary>
    public override void DoThrow(HeroEntity hero)
    {
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
    protected override string TrueName => "治疗药水";
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
    protected override string TrueName => "生命活力药水";
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
    protected override string TrueName => "力量药水";
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
    protected override string TrueName => "经验药水";
    public override void Apply(HeroEntity hero)
    {
        hero.Exp += 10;
        MarkIdentified();
    }
}

/// <summary>
///     隐身药水 — 施加隐身效果（20 回合）
/// </summary>
public sealed class InvisibilityPotion : Potion
{
    protected override string TrueName => "隐身药水";
    public override void Apply(HeroEntity hero)
    {
        Buff.Prolong<InvisibilityBuff>(hero, 20f);
        MarkIdentified();
    }
}

/// <summary>
///     火焰药水 — 投掷后造成火焰伤害
/// </summary>
public sealed class LiquidFlamePotion : Potion
{
    protected override string TrueName => "火焰药水";
    public override void Apply(HeroEntity hero)
    {
        // 对自身和周围造成火焰伤害（简化版：直接对英雄造成少量伤害）
        int dmg = 5 + hero.Level;
        hero.Damage(dmg, this);
        MarkIdentified();
    }
}

/// <summary>
///     冰冻药水 — 投掷后冻结目标
/// </summary>
public sealed class FrostPotion : Potion
{
    protected override string TrueName => "冰冻药水";
    public override void Apply(HeroEntity hero)
    {
        Buff.Prolong<FrostBuff>(hero, 10f);
        // 对附近怪物也施加冰冻
        foreach (var mob in Actor.All().OfType<MobEntity>())
        {
            if (hero.DistanceTo(mob.Pos) <= 2)
                Buff.Prolong<FrostBuff>(mob, 10f);
        }
        MarkIdentified();
    }
}

/// <summary>
///     麻痹药水 — 施加麻痹 Buff
/// </summary>
public sealed class ParalysisPotion : Potion
{
    protected override string TrueName => "麻痹药水";
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
    protected override string TrueName => "净化药水";
    public override void Apply(HeroEntity hero)
    {
        var debuffs = hero.Buffs.Where(b => b.Type == BuffType.Negative).ToList();
        foreach (var debuff in debuffs)
            hero.RemoveBuff(debuff);
        MarkIdentified();
    }
}

/// <summary>
///     疾跑药水 — 临时提高速度（20 回合）
/// </summary>
public sealed class HastePotion : Potion
{
    protected override string TrueName => "疾跑药水";
    public override void Apply(HeroEntity hero)
    {
        Buff.Prolong<HasteBuff>(hero, 20f);
        MarkIdentified();
    }
}

/// <summary>
///     思维诱导药水 — 显示全图怪物位置
/// </summary>
public sealed class MindVisionPotion : Potion
{
    protected override string TrueName => "思维诱导药水";
    public override void Apply(HeroEntity hero)
    {
        // TODO: 显示全图怪物位置标记（需要视距系统支持）
        MarkIdentified();
    }
}

/// <summary>
///     毒性瓦斯药水 — 投掷后释放毒气区域
/// </summary>
public sealed class ToxicGasPotion : Potion
{
    protected override string TrueName => "毒性瓦斯药水";
    public override void Apply(HeroEntity hero)
    {
        // 对英雄及其周围造成毒伤害（简化版，需要 Blob 系统支持）
        int dmg = 3 + hero.Level / 2;
        hero.Damage(dmg, this);
        MarkIdentified();
    }
}

/// <summary>
///     传送药水 — 随机传送
/// </summary>
public sealed class TeleportPotion : Potion
{
    protected override string TrueName => "传送药水";
    public override void Apply(HeroEntity hero)
    {
        // 随机传送（简化版：临时位移，需要地图数据支持）
        // TODO: 接入地牢地图数据，随机选择一个可通行位置
        MarkIdentified();
    }
}