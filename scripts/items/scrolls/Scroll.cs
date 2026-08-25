using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.items.scrolls;

/// <summary>
///     卷轴基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.scrolls.Scroll
///     12 种卷轴
/// </summary>
public abstract class Scroll : Item
{
    /// <summary>卷轴名称索引（新游戏随机分配）</summary>
    public int LabelIndex { get; set; }

    public override string DefaultAction => ItemAction.Read;

    /// <summary>
    ///     执行卷轴效果
    /// </summary>
    public abstract void Read(HeroEntity hero);
}

/// <summary>
///     鉴定卷轴
/// </summary>
public sealed class IdentifyScroll : Scroll
{
    public override string Name => "鉴定卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 鉴定背包中未鉴定的物品
    }
}

/// <summary>
///     升级卷轴（SoU）
/// </summary>
public sealed class UpgradeScroll : Scroll
{
    public override string Name => "升级卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 选择一个装备升级
    }
}

/// <summary>
///     移除诅咒卷轴
/// </summary>
public sealed class RemoveCurseScroll : Scroll
{
    public override string Name => "移除诅咒卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 解除所有装备诅咒
    }
}

/// <summary>
///     魔法映射卷轴
/// </summary>
public sealed class MagicMappingScroll : Scroll
{
    public override string Name => "魔法映射卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 显示全图地形
    }
}

/// <summary>
///     召唤卷轴（召唤怪物）
/// </summary>
public sealed class SummonScroll : Scroll
{
    public override string Name => "召唤卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 召唤怪物
    }
}

/// <summary>
///     传送卷轴
/// </summary>
public sealed class TeleportScroll : Scroll
{
    public override string Name => "传送卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 随机传送
    }
}

/// <summary>
///     挑战卷轴（激怒附近怪物）
/// </summary>
public sealed class ChallengeScroll : Scroll
{
    public override string Name => "挑战卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 激怒附近怪物
    }
}

/// <summary>
///     恐怖卷轴（吓跑怪物）
/// </summary>
public sealed class TerrorScroll : Scroll
{
    public override string Name => "恐怖卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 施加恐惧 Buff
    }
}

/// <summary>
///     复仇卷轴（伤害附近怪物）
/// </summary>
public sealed class RetributionScroll : Scroll
{
    public override string Name => "复仇卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 对附近怪物造成伤害
    }
}

/// <summary>
///     镜像卷轴（召唤分身）
/// </summary>
public sealed class MirrorImageScroll : Scroll
{
    public override string Name => "镜像卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 召唤分身
    }
}

/// <summary>
///     迷雾卷轴（释放烟雾）
/// </summary>
public sealed class FogScroll : Scroll
{
    public override string Name => "迷雾卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 释放烟雾区域效果
    }
}

/// <summary>
///     噬咒卷轴（诅咒装备）
/// </summary>
public sealed class CurseScroll : Scroll
{
    public override string Name => "噬咒卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 诅咒装备
    }
}

/// <summary>
///     觉醒卷轴（唤醒附近怪物）
/// </summary>
public sealed class AwakeningScroll : Scroll
{
    public override string Name => "觉醒卷轴";

    public override void Read(HeroEntity hero)
    {
        // TODO: 唤醒附近怪物
    }
}