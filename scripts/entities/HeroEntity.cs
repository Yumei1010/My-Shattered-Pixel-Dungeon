using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.weapons;
using MyShatteredPixelDungeon.scripts.items.armors;
using MyShatteredPixelDungeon.scripts.items.wands;
using MyShatteredPixelDungeon.scripts.items.rings;

namespace MyShatteredPixelDungeon.scripts.entities;

/// <summary>
///     英雄实体，对应原版 Hero
///     持有职业、天赋、背包、动作系统
/// </summary>
public sealed class HeroEntity : CharEntity
{
    public override int ActPriority => ActorPriority.Hero;

    /// <summary>
    ///     是否有待处理的玩家指令
    /// </summary>
    public bool HasAction => CurAction != null;

    /// <summary>当前动作</summary>
    public object? CurAction { get; set; }

    /// <summary>上一个动作</summary>
    public object? LastAction { get; set; }

    /// <summary>是否就绪（等待输入）</summary>
    public bool Ready { get; set; }

    // ========== 背包与装备 ==========

    /// <summary>背包容器</summary>
    public ItemContainer Inventory { get; } = new();

    /// <summary>当前装备的武器</summary>
    public Weapon? EquippedWeapon { get; private set; }

    /// <summary>当前装备的护甲</summary>
    public Armor? EquippedArmor { get; private set; }

    /// <summary>当前装备的法杖</summary>
    public Wand? EquippedWand { get; private set; }

    /// <summary>当前装备的戒指（左）</summary>
    public Ring? EquippedRingLeft { get; private set; }

    /// <summary>当前装备的戒指（右）</summary>
    public Ring? EquippedRingRight { get; private set; }

    /// <summary>背包中的金币</summary>
    public int Gold
    {
        get
        {
            var gold = Inventory.Find<Gold>();
            return gold?.Quantity ?? 0;
        }
        set
        {
            var gold = Inventory.Find<Gold>();
            if (gold != null)
                gold.Quantity = value;
        }
    }

    public HeroEntity()
    {
        Alignment = Alignment.Ally;
        MaxHp = 30;
        Hp = 30;
        Strength = 10;
        ViewDistance = 8;
    }

    protected override bool Act()
    {
        // 更新视野
        // 处理 Buff
        // 如果有待执行动作则执行，否则等待输入
        return HasAction;
    }

    /// <summary>
    ///     标记为就绪（等待玩家输入）
    /// </summary>
    public void SetReady()
    {
        Ready = true;
        CurAction = null;
    }

    // ========== 装备管理 ==========

    /// <summary>
    ///     装备武器
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon?.OnUnequip(this);
        EquippedWeapon = weapon;
        weapon.IsEquipped = true;
        weapon.OnEquip(this);
    }

    /// <summary>
    ///     卸下武器
    /// </summary>
    public void UnequipWeapon()
    {
        if (EquippedWeapon != null)
        {
            EquippedWeapon.IsEquipped = false;
            EquippedWeapon.OnUnequip(this);
            Inventory.TryAdd(EquippedWeapon);
            EquippedWeapon = null;
        }
    }

    /// <summary>
    ///     装备护甲
    /// </summary>
    public void EquipArmor(Armor armor)
    {
        EquippedArmor?.OnUnequip(this);
        EquippedArmor = armor;
        armor.IsEquipped = true;
        armor.OnEquip(this);
    }

    /// <summary>
    ///     卸下护甲
    /// </summary>
    public void UnequipArmor()
    {
        if (EquippedArmor != null)
        {
            EquippedArmor.IsEquipped = false;
            EquippedArmor.OnUnequip(this);
            Inventory.TryAdd(EquippedArmor);
            EquippedArmor = null;
        }
    }

    /// <summary>
    ///     伤害骰子（含武器修正）
    /// </summary>
    public override int DamageRoll()
    {
        if (EquippedWeapon != null)
            return EquippedWeapon.DamageRoll();
        return base.DamageRoll();
    }

    /// <summary>
    ///     攻击技能（含武器修正）
    /// </summary>
    public override int AttackSkill(CharEntity target)
    {
        int skill = base.AttackSkill(target);
        if (EquippedWeapon != null && target != null)
        {
            skill = (int)(skill * EquippedWeapon.AccuracyFactor(Strength));
        }
        return skill;
    }

    /// <summary>
    ///     护甲减伤（含护甲修正）
    /// </summary>
    public override int DrRoll()
    {
        if (EquippedArmor != null)
            return EquippedArmor.DrRoll();
        return base.DrRoll();
    }

    /// <summary>
    ///     速度（含装备惩罚）
    /// </summary>
    public override float Speed()
    {
        float speed = base.Speed();
        if (EquippedWeapon != null)
            speed *= EquippedWeapon.SpeedFactor(Strength);
        if (EquippedArmor != null)
            speed *= EquippedArmor.SpeedFactor(Strength);
        return speed;
    }
}