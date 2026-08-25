using GFramework.Core.Abstractions.controller;
using GFramework.Game.Abstractions.enums;
using GFramework.Game.Abstractions.ui;
using GFramework.Godot.ui;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.core.ui;
using MyShatteredPixelDungeon.scripts.items;
using Godot;

namespace MyShatteredPixelDungeon.scripts.menu.inventory_page;

/// <summary>
///     背包页面，展示英雄背包中的物品列表
///     支持选择、使用、装备、丢弃物品
/// </summary>
[Log]
[ContextAware]
public partial class InventoryPage : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    private IUiPageBehavior? _page;

    /// <summary>当前选中的物品索引</summary>
    private int _selectedIndex = -1;

    /// <summary>当前选中的物品</summary>
    private Item? _selectedItem;

    public override void _Ready()
    {
        _ = ReadyAsync();
        ConnectPageSignals();
        RegisterEvents();
    }

    public IUiPageBehavior GetPage()
    {
        _page ??= UiPageBehaviorFactory.Create<InventoryPage>(this, UiKeyStr, UiLayer.Page);
        return _page;
    }

    /// <summary>
    ///     刷新物品列表显示
    /// </summary>
    private void RefreshItemList()
    {
        // 由子类实现的节点填充
        RefreshList();
    }

    /// <summary>
    ///     刷新物品详情面板
    /// </summary>
    private void RefreshItemDetail()
    {
        UpdateDetail();
    }

    /// <summary>
    ///     选中物品
    /// </summary>
    private void SelectItem(int index)
    {
        _selectedIndex = index;
        _selectedItem = GetItemAt(index);
        RefreshItemDetail();
        RefreshActionButtons();
    }

    /// <summary>
    ///     刷新动作按钮
    /// </summary>
    private void RefreshActionButtons()
    {
        UpdateActionButtons();
    }
}