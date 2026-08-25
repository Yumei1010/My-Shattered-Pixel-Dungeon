using GFramework.Core.extensions;
using GFramework.Godot.extensions;
using MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;

namespace MyShatteredPixelDungeon.scripts.menu.inventory_page;

public partial class InventoryPage
{
    /// <summary>
    ///     注册 CQRS 事件订阅（背包变化时刷新 UI）
    /// </summary>
    private void RegisterEvents()
    {
        this.RegisterEvent<InventoryChangedEvent>(OnInventoryChanged)
            .UnRegisterWhenNodeExitTree(this);
    }

    private void OnInventoryChanged(InventoryChangedEvent e)
    {
        // 背包变化时刷新列表
        RefreshItemList();
    }
}