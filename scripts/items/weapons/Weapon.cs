namespace MyShatteredPixelDungeon.scripts.items.weapons;

/// <summary>
///     武器基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.weapon.Weapon
/// </summary>
public abstract class Weapon : EquipableItem
{
    /// <summary>攻击范围</summary>
    public virtual int Reach { get; set; } = 1;

    /// <summary>命中修正</summary>
    public float Accuracy { get; set; } = 1f;

    /// <summary>攻击速度（延迟系数）</summary>
    public float Delay { get; set; } = 1f;

    /// <summary>最小伤害</summary>
    public int DamageMin { get; set; } = 1;

    /// <summary>最大伤害（含等级修正）</summary>
    public int DamageMax { get; set; } = 4;

    /// <summary>附魔</summary>
    public Enchantment? Enchantment { get; set; }

    public override bool IsUpgradable => true;
    public override bool IsEnchantable => true;

    /// <summary>
    ///     伤害骰子
    /// </summary>
    public virtual int DamageRoll()
    {
        int dmg = Random.Shared.Next(DamageMin, DamageMax + 1);
        // 每级 +1 伤害
        dmg += Level;
        return Math.Max(0, dmg);
    }

    /// <summary>
    ///     根据力量需求计算速度惩罚/奖励
    /// </summary>
    public float SpeedFactor(int strength)
    {
        if (strength >= StrReq) return 1f;
        return 1f - 0.04f * (StrReq - strength);
    }

    /// <summary>
    ///     根据力量需求计算命中惩罚/奖励
    /// </summary>
    public float AccuracyFactor(int strength)
    {
        if (strength >= StrReq) return 1f;
        return 1f - 0.1f * (StrReq - strength);
    }
}

/// <summary>
///     近战武器，对应原版 MeleeWeapon
///     12 种：短剑/阔剑/长戟/巨剑/匕首/镰刀/弯刀/拳套/重锤/长矛/矛/棍
/// </summary>
public abstract class MeleeWeapon : Weapon
{
    /// <summary>每级额外伤害</summary>
    public abstract int Tier { get; }

    /// <summary>
    ///     根据等级计算力量需求
    /// </summary>
    public void UpdateStrReq()
    {
        StrReq = 8 + Tier * 2;
    }

    /// <summary>
    ///     根据等级更新伤害
    /// </summary>
    public void UpdateDamage()
    {
        DamageMin = Tier * 2 + Level;
        DamageMax = (Tier + 1) * 4 + Level * 2;
    }
}

/// <summary>
///     远程武器基类，对应原版 MissileWeapon
/// </summary>
public abstract class MissileWeapon : Weapon
{
    public override bool IsUpgradable => true;
    public override bool IsEnchantable => true;

    /// <summary>弹药类型</summary>
    public abstract string AmmoType { get; }
}

/// <summary>
///     飞镖，对应原版 Dart
/// </summary>
public abstract class Dart : MissileWeapon
{
    public override string AmmoType => "dart";
}