using GFramework.Core.system;
using GFramework.Core.extensions;
using MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;
using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.weapons;
using MyShatteredPixelDungeon.scripts.items.armors;

namespace MyShatteredPixelDungeon.scripts.systems;

/// <summary>
///     背包系统，管理物品拾取/丢弃/使用/装备
/// </summary>
public sealed class InventorySystem : AbstractSystem
{
    protected override void OnInit()
    {
        // 注册事件处理
        this.RegisterEvent<ItemPickedUpEvent>(OnItemPickedUp);
        this.RegisterEvent<ItemDroppedEvent>(OnItemDropped);
        this.RegisterEvent<ItemUsedEvent>(OnItemUsed);
        this.RegisterEvent<ItemEquippedEvent>(OnItemEquipped);
        this.RegisterEvent<ItemUnequippedEvent>(OnItemUnequipped);
    }

    private void OnItemPickedUp(ItemPickedUpEvent e)
    {
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault(h => h.Id == e.EntityId);
        if (hero == null) return;

        // 发送背包变化事件通知 UI 刷新
        this.SendEvent(new InventoryChangedEvent
        {
            EntityId = e.EntityId,
            ChangeType = "Add",
            ItemType = e.ItemType,
            Quantity = e.Quantity
        });
    }

    private void OnItemDropped(ItemDroppedEvent e)
    {
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault(h => h.Id == e.EntityId);
        if (hero == null) return;

        this.SendEvent(new InventoryChangedEvent
        {
            EntityId = e.EntityId,
            ChangeType = "Remove",
            ItemType = e.ItemType,
            Quantity = 0
        });
    }

    private void OnItemUsed(ItemUsedEvent e)
    {
        this.SendEvent(new InventoryChangedEvent
        {
            EntityId = e.EntityId,
            ChangeType = "Update",
            ItemType = e.ItemType,
            Quantity = 0
        });
    }

    private void OnItemEquipped(ItemEquippedEvent e)
    {
        this.SendEvent(new InventoryChangedEvent
        {
            EntityId = e.EntityId,
            ChangeType = "Update",
            ItemType = e.ItemType,
            Quantity = 0
        });
    }

    private void OnItemUnequipped(ItemUnequippedEvent e)
    {
        this.SendEvent(new InventoryChangedEvent
        {
            EntityId = e.EntityId,
            ChangeType = "Update",
            ItemType = e.ItemType,
            Quantity = 0
        });
    }

    /// <summary>
    ///     拾取地面物品
    /// </summary>
    public static bool PickUpItem(HeroEntity hero, Item item)
    {
        if (hero == null || item == null) return false;

        if (item.DoPickUp(hero)) return true;

        if (item.Collect(hero.Inventory))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     丢弃物品到地面
    /// </summary>
    public static void DropItem(HeroEntity hero, Item item, int pos)
    {
        if (hero == null || item == null) return;

        item.DoDrop(hero);
        hero.Inventory.Remove(item);
    }

    /// <summary>
    ///     使用物品
    /// </summary>
    public static bool UseItem(HeroEntity hero, Item item)
    {
        if (hero == null || item == null) return false;

        // 根据物品默认动作执行
        switch (item.DefaultAction)
        {
            case ItemAction.Equip:
                return EquipItem(hero, item);
            case ItemAction.Drink:
            case ItemAction.Eat:
            case ItemAction.Read:
                // 消耗品使用
                return ConsumeItem(hero, item);
            default:
                return false;
        }
    }

    /// <summary>
    ///     装备物品
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
    ///     消耗物品
    /// </summary>
    public static bool ConsumeItem(HeroEntity hero, Item item)
    {
        // 从背包移除
        item.Detach(hero.Inventory);
        return true;
    }
}