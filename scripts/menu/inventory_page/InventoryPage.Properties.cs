using MyShatteredPixelDungeon.scripts.enums.ui;

namespace MyShatteredPixelDungeon.scripts.menu.inventory_page;

public partial class InventoryPage
{
    /// <summary>
    ///     UI 键字符串
    /// </summary>
    public static string UiKeyStr => nameof(UiKey.InventoryPage);

    /// <summary>物品列表容器（VBoxContainer 或 ItemList）</summary>
    private ItemListWidget? _itemList;

    /// <summary>物品详情标签</summary>
    private DetailWidget? _detailWidget;

    /// <summary>动作按钮容器</summary>
    private ActionWidget? _actionWidget;
}