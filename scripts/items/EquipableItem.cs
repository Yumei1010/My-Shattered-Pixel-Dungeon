using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.items;

/// <summary>
///     可装备物品基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.EquipableItem
/// </summary>
public abstract class EquipableItem : Item
{
    /// <summary>力量需求</summary>
    public int StrReq { get; set; }

    /// <summary>是否已装备</summary>
    public bool IsEquipped { get; set; }

    /// <summary>诅咒附着</summary>
    public bool CurseInflicted { get; set; }

    public override string DefaultAction => ItemAction.Equip;

    public override List<string> Actions(HeroEntity hero)
    {
        var actions = base.Actions(hero);
        actions.Add(IsEquipped ? ItemAction.Unequip : ItemAction.Equip);
        return actions;
    }

    /// <summary>
    ///     装备
    /// </summary>
    public virtual void OnEquip(HeroEntity hero) { }

    /// <summary>
    ///     卸下
    /// </summary>
    public virtual void OnUnequip(HeroEntity hero) { }
}