using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.weapons;
using MyShatteredPixelDungeon.scripts.items.armors;

namespace MyShatteredPixelDungeon.scripts.systems;

/// <summary>
///     背包系统工具类，提供物品拾取/丢弃/使用/装备等静态方法
/// </summary>
public static class InventorySystem
{
    /// <summary>
    ///     拾取物品到英雄背包
    /// </summary>
    public static bool PickUpItem(HeroEntity hero, Item item)
    {
        if (hero == null || item == null) return false;
        if (item.DoPickUp(hero)) return true;
        return item.Collect(hero.Inventory);
    }

    /// <summary>
    ///     从英雄背包丢弃物品
    /// </summary>
    public static void DropItem(HeroEntity hero, Item item, int pos)
    {
        if (hero == null || item == null) return;
        item.DoDrop(hero);
        hero.Inventory.Remove(item);
    }

    /// <summary>
    ///     装备物品到英雄
    /// </summary>
    public static bool EquipItem(HeroEntity hero, Item item)
    {
        if (item is Weapon weapon)
        {
            hero.EquipWeapon(weapon);
            return true;
        }
        if (item is Armor armor)
        {
            hero.EquipArmor(armor);
            return true;
        }
        return false;
    }

    /// <summary>
    ///     消耗物品（药水/卷轴/食物）
    /// </summary>
    public static bool ConsumeItem(HeroEntity hero, Item item)
    {
        if (item == null) return false;
        item.Detach(hero.Inventory);
        return true;
    }

    /// <summary>
    ///     使用物品（根据默认动作分发）
    /// </summary>
    public static bool UseItem(HeroEntity hero, Item item)
    {
        if (hero == null || item == null) return false;

        return item.DefaultAction switch
        {
            ItemAction.Equip => EquipItem(hero, item),
            ItemAction.Drink or ItemAction.Eat or ItemAction.Read => ConsumeItem(hero, item),
            _ => false
        };
    }

    /// <summary>
    ///     获取金币总量
    /// </summary>
    public static int GetTotalGold(HeroEntity hero)
    {
        if (hero == null) return 0;
        var gold = hero.Inventory.Find<Gold>();
        return gold?.Quantity ?? 0;
    }
}