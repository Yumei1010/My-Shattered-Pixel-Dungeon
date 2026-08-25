using GFramework.Core.extensions;
using GFramework.Game.Abstractions.ui;
using MyShatteredPixelDungeon.global;
using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.weapons;
using MyShatteredPixelDungeon.scripts.items.armors;
using MyShatteredPixelDungeon.scripts.systems;
using Godot;

namespace MyShatteredPixelDungeon.scripts.menu.inventory_page;

public partial class InventoryPage
{
    private IUiRouter _uiRouter = null!;
    private HeroEntity? _hero;

    /// <summary>
    ///     异步初始化
    /// </summary>
    private async Task ReadyAsync()
    {
        await GameEntryPoint.Architecture.WaitUntilReadyAsync().ConfigureAwait(false);
        _uiRouter = this.GetSystem<IUiRouter>()!;
        _hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();

        // 获取节点引用
        _itemList = new ItemListWidget(GetNode<VBoxContainer>("%ItemListContainer"));
        _detailWidget = new DetailWidget(GetNode<RichTextLabel>("%ItemDetail"));
        _actionWidget = new ActionWidget(GetNode<HBoxContainer>("%ActionButtons"));

        _itemList?.Clear();
        _actionWidget?.Hide();

        if (_hero != null)
            PopulateItemList();
    }

    /// <summary>
    ///     填充物品列表
    /// </summary>
    private void PopulateItemList()
    {
        if (_hero == null || _itemList == null) return;
        _itemList.Clear();
        foreach (var item in _hero.Inventory.Items)
        {
            _itemList.AddItem(item);
        }
    }

    /// <summary>
    ///     刷新列表
    /// </summary>
    private void RefreshList()
    {
        PopulateItemList();
    }

    /// <summary>
    ///     获取指定索引的物品
    /// </summary>
    private Item? GetItemAt(int index)
    {
        if (_hero == null || index < 0 || index >= _hero.Inventory.Items.Count)
            return null;
        return _hero.Inventory.Items[index];
    }

    /// <summary>
    ///     更新详情面板
    /// </summary>
    private void UpdateDetail()
    {
        if (_detailWidget == null) return;
        if (_selectedItem != null)
        {
            _detailWidget.ShowItem(_selectedItem);
        }
        else
        {
            _detailWidget.Clear();
        }
    }

    /// <summary>
    ///     更新动作按钮
    /// </summary>
    private void UpdateActionButtons()
    {
        if (_actionWidget == null || _hero == null) return;
        _actionWidget.Clear();

        if (_selectedItem == null)
        {
            _actionWidget.Hide();
            return;
        }

        _actionWidget.Show();

        // 根据物品类型显示可用动作
        var actions = _selectedItem.Actions(_hero);
        foreach (var action in actions)
        {
            _actionWidget.AddButton(action, () => OnActionSelected(action));
        }
    }

    /// <summary>
    ///     动作按钮点击处理
    /// </summary>
    private void OnActionSelected(string action)
    {
        if (_hero == null || _selectedItem == null) return;

        switch (action)
        {
            case ItemAction.Equip:
                InventorySystem.EquipItem(_hero, _selectedItem);
                break;
            case ItemAction.Unequip:
                if (_selectedItem is EquipableItem equipable)
                {
                    if (equipable is Weapon)
                        _hero.UnequipWeapon();
                    else if (equipable is Armor)
                        _hero.UnequipArmor();
                }
                break;
            case ItemAction.Drink:
            case ItemAction.Eat:
            case ItemAction.Read:
                InventorySystem.ConsumeItem(_hero, _selectedItem);
                break;
            case ItemAction.Throw:
                // TODO: 投掷逻辑
                break;
        }

        // 刷新显示
        PopulateItemList();
        RefreshItemDetail();
        RefreshActionButtons();
    }

    /// <summary>
    ///     关闭背包
    /// </summary>
    private void CloseInventory()
    {
        _uiRouter.Pop();
    }
}

// ========== 辅助 Widget 类 ==========

/// <summary>
///     物品列表 Widget
/// </summary>
internal sealed class ItemListWidget
{
    private readonly VBoxContainer _container;

    public ItemListWidget(VBoxContainer container) => _container = container;

    public void Clear()
    {
        foreach (var child in _container.GetChildren())
            child.QueueFree();
    }

    public void AddItem(Item item)
    {
        var label = new Label
        {
            Text = FormatItemName(item),
            AutoTranslate = false
        };
        _container.AddChild(label);
    }

    private static string FormatItemName(Item item)
    {
        string prefix = "";
        string suffix = "";

        if (item is EquipableItem equip && equip.IsEquipped)
            prefix = "[E] ";

        if (item.Stackable && item.Quantity > 1)
            suffix = $" x{item.Quantity}";

        return $"{prefix}{item.Name}{suffix}";
    }
}

/// <summary>
///     物品详情 Widget
/// </summary>
internal sealed class DetailWidget
{
    private readonly RichTextLabel _label;

    public DetailWidget(RichTextLabel label) => _label = label;

    public void ShowItem(Item item)
    {
        string text = $"[b]{item.Name}[/b]\n";
        text += $"类型: {item.GetType().Name}\n";
        text += $"等级: {item.Level}\n";
        text += item.Info();
        _label.Text = text;
    }

    public void Clear()
    {
        _label.Text = "";
    }
}

/// <summary>
///     动作按钮 Widget
/// </summary>
internal sealed class ActionWidget
{
    private readonly HBoxContainer _container;

    public ActionWidget(HBoxContainer container) => _container = container;

    public void Show() => _container.Visible = true;
    public void Hide() => _container.Visible = false;

    public void Clear()
    {
        foreach (var child in _container.GetChildren())
            child.QueueFree();
    }

    public void AddButton(string text, Action callback)
    {
        var button = new Button
        {
            Text = text,
            AutoTranslate = false
        };
        button.Pressed += callback;
        _container.AddChild(button);
    }
}