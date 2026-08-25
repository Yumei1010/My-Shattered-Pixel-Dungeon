namespace MyShatteredPixelDungeon.scripts.items.armors;

/// <summary>
///     护甲基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.armor.Armor
///     7 种：布/皮/锁/鳞/板/职业
/// </summary>
public abstract class Armor : EquipableItem
{
    /// <summary>减伤值</summary>
    public int DamageReduction { get; set; }

    /// <summary>最大减伤</summary>
    public int DamageReductionMax { get; set; }

    /// <summary>护甲技能（职业专属）</summary>
    public virtual string? SpecialAbility => null;

    public override bool IsUpgradable => true;
    public override bool IsEnchantable => true;

    /// <summary>
    ///     减伤骰子
    /// </summary>
    public virtual int DrRoll()
    {
        if (DamageReductionMax <= 0) return 0;
        int dr = Random.Shared.Next(DamageReduction, DamageReductionMax + 1);
        // 每级额外减伤
        dr += Level;
        return Math.Max(0, dr);
    }

    /// <summary>
    ///     根据等级更新减伤
    /// </summary>
    public void UpdateDr()
    {
        int tier = GetTier();
        DamageReduction = tier;
        DamageReductionMax = tier * 2 + 3 + Level;
    }

    /// <summary>
    ///     获取护甲等级
    /// </summary>
    protected abstract int GetTier();

    /// <summary>
    ///     根据等级更新力量需求
    /// </summary>
    public void UpdateStrReq()
    {
        int tier = GetTier();
        StrReq = 7 + tier * 2;
    }

    /// <summary>
    ///     根据力量需求计算速度惩罚
    /// </summary>
    public float SpeedFactor(int strength)
    {
        if (strength >= StrReq) return 1f;
        return 1f - 0.04f * (StrReq - strength);
    }
}

/// <summary>
///     布甲（Tier 1）
/// </summary>
public sealed class ClothArmor : Armor
{
    protected override int GetTier() => 1;
}

/// <summary>
///     皮甲（Tier 2）
/// </summary>
public sealed class LeatherArmor : Armor
{
    protected override int GetTier() => 2;
}

/// <summary>
///     锁甲（Tier 3）
/// </summary>
public sealed class MailArmor : Armor
{
    protected override int GetTier() => 3;
}

/// <summary>
///     鳞甲（Tier 4）
/// </summary>
public sealed class ScaleArmor : Armor
{
    protected override int GetTier() => 4;
}

/// <summary>
///     板甲（Tier 5）
/// </summary>
public sealed class PlateArmor : Armor
{
    protected override int GetTier() => 5;
}