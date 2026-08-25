using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.entities.buffs;

namespace MyShatteredPixelDungeon.scripts.items.scrolls;

/// <summary>
///     卷轴基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.scrolls.Scroll
///     12 种卷轴，未鉴定时显示随机标签名，已鉴定显示真名
/// </summary>
public abstract class Scroll : Item
{
    /// <summary>卷轴名称索引（新游戏随机分配）</summary>
    public int LabelIndex { get; set; }

    public override string DefaultAction => ItemAction.Read;

    /// <summary>
    ///     显示名称（未鉴定时显示标签，已鉴定显示真名）
    /// </summary>
    public override string Name
    {
        get
        {
            if (IsIdentified)
                return TrueName;
            string label = IdentificationSystem.GetScrollLabel(this);
            return $"卷轴「{label}」";
        }
    }

    /// <summary>
    ///     真实名称
    /// </summary>
    protected abstract string TrueName { get; }

    /// <summary>
    ///     执行卷轴效果
    /// </summary>
    public abstract void Read(HeroEntity hero);

    /// <summary>
    ///     标记卷轴为已鉴定
    /// </summary>
    protected void MarkIdentified()
    {
        Identify();
    }
}

/// <summary>
///     鉴定卷轴 — 鉴定一件未鉴定的物品
/// </summary>
public sealed class IdentifyScroll : Scroll
{
    protected override string TrueName => "鉴定卷轴";
    public override void Read(HeroEntity hero)
    {
        var unident = hero.Inventory.Items.FirstOrDefault(i => !i.IsIdentified);
        if (unident != null) unident.Identify();
        MarkIdentified();
    }
}

/// <summary>
///     升级卷轴（SoU）— 升级一件装备
/// </summary>
public sealed class UpgradeScroll : Scroll
{
    protected override string TrueName => "升级卷轴";
    public override void Read(HeroEntity hero)
    {
        if (hero.EquippedWeapon != null)
        {
            hero.EquippedWeapon.Level++;
            hero.EquippedWeapon.Identify();
        }
        else if (hero.EquippedArmor != null)
        {
            hero.EquippedArmor.Level++;
            hero.EquippedArmor.Identify();
        }
        MarkIdentified();
    }
}

/// <summary>
///     移除诅咒卷轴 — 解除所有装备诅咒
/// </summary>
public sealed class RemoveCurseScroll : Scroll
{
    protected override string TrueName => "移除诅咒卷轴";
    public override void Read(HeroEntity hero)
    {
        foreach (var item in hero.Inventory.Items) { item.Cursed = false; item.CursedKnown = true; }
        if (hero.EquippedWeapon != null) { hero.EquippedWeapon.Cursed = false; hero.EquippedWeapon.CursedKnown = true; }
        if (hero.EquippedArmor != null) { hero.EquippedArmor.Cursed = false; hero.EquippedArmor.CursedKnown = true; }
        MarkIdentified();
    }
}

/// <summary>
///     魔法映射卷轴 — 显示全图地形
/// </summary>
public sealed class MagicMappingScroll : Scroll
{
    protected override string TrueName => "魔法映射卷轴";
    public override void Read(HeroEntity hero)
    {
        // TODO: 标记所有已探索但未看到的地形（需要视距系统支持）
        MarkIdentified();
    }
}

/// <summary>
///     传送卷轴 — 随机传送
/// </summary>
public sealed class TeleportScroll : Scroll
{
    protected override string TrueName => "传送卷轴";
    public override void Read(HeroEntity hero)
    {
        // TODO: 随机传送到地牢中其他位置（需要地图数据支持）
        MarkIdentified();
    }
}

/// <summary>
///     挑战卷轴 — 激怒附近怪物，使其攻击力提升
/// </summary>
public sealed class ChallengeScroll : Scroll
{
    protected override string TrueName => "挑战卷轴";
    public override void Read(HeroEntity hero)
    {
        // 激怒附近所有怪物（简化版：唤醒并标记）
        foreach (var mob in Actor.All().OfType<MobEntity>())
        {
            if (hero.DistanceTo(mob.Pos) <= hero.ViewDistance)
            {
                // TODO: 唤醒怪物并激怒
            }
        }
        MarkIdentified();
    }
}

/// <summary>
///     恐怖卷轴 — 吓跑附近怪物（施加恐惧 Buff）
/// </summary>
public sealed class TerrorScroll : Scroll
{
    protected override string TrueName => "恐怖卷轴";
    public override void Read(HeroEntity hero)
    {
        foreach (var mob in Actor.All().OfType<MobEntity>())
        {
            if (hero.DistanceTo(mob.Pos) <= hero.ViewDistance)
                Buff.Prolong<TerrorBuff>(mob, 15f);
        }
        MarkIdentified();
    }
}

/// <summary>
///     复仇卷轴 — 伤害附近所有怪物
/// </summary>
public sealed class RetributionScroll : Scroll
{
    protected override string TrueName => "复仇卷轴";
    public override void Read(HeroEntity hero)
    {
        foreach (var mob in Actor.All().OfType<MobEntity>())
        {
            if (hero.DistanceTo(mob.Pos) <= hero.ViewDistance)
            {
                int dmg = 10 + hero.Level * 2;
                mob.Damage(dmg, hero);
            }
        }
        MarkIdentified();
    }
}

/// <summary>
///     镜像卷轴 — 召唤分身
/// </summary>
public sealed class MirrorImageScroll : Scroll
{
    protected override string TrueName => "镜像卷轴";
    public override void Read(HeroEntity hero)
    {
        // TODO: 在英雄旁边召唤一个分身（需要实体生成系统支持）
        MarkIdentified();
    }
}

/// <summary>
///     迷雾卷轴 — 释放烟雾区域
/// </summary>
public sealed class FogScroll : Scroll
{
    protected override string TrueName => "迷雾卷轴";
    public override void Read(HeroEntity hero)
    {
        // TODO: 在英雄周围释放烟雾 Blob（需要 Blob 系统支持）
        MarkIdentified();
    }
}

/// <summary>
///     噬咒卷轴 — 诅咒装备
/// </summary>
public sealed class CurseScroll : Scroll
{
    protected override string TrueName => "噬咒卷轴";
    public override void Read(HeroEntity hero)
    {
        if (hero.EquippedWeapon != null) { hero.EquippedWeapon.Cursed = true; hero.EquippedWeapon.CursedKnown = true; }
        if (hero.EquippedArmor != null) { hero.EquippedArmor.Cursed = true; hero.EquippedArmor.CursedKnown = true; }
        MarkIdentified();
    }
}

/// <summary>
///     觉醒卷轴 — 唤醒附近所有沉睡的怪物
/// </summary>
public sealed class AwakeningScroll : Scroll
{
    protected override string TrueName => "觉醒卷轴";
    public override void Read(HeroEntity hero)
    {
        // 唤醒附近所有怪物
        foreach (var mob in Actor.All().OfType<MobEntity>())
        {
            if (hero.DistanceTo(mob.Pos) <= hero.ViewDistance * 2)
            {
                // TODO: 唤醒怪物（设置怪物状态为追逐）
            }
        }
        MarkIdentified();
    }
}