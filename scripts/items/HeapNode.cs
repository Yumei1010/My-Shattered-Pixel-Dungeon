using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using Godot;

namespace MyShatteredPixelDungeon.scripts.items;

/// <summary>
///     地面物品堆类型
/// </summary>
public enum HeapType
{
    Heap,           // 普通堆
    ForSale,        // 待售（商店）
    Chest,          // 箱子
    LockedChest,    // 锁着的箱子
    CrystalChest,   // 水晶箱
    Tomb,           // 坟墓
    Skeleton,       // 骷髅
    Remains,        // 遗骸
    Mimic           // 宝箱怪
}

/// <summary>
///     地面物品堆，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.Heap
///     作为 Node2D 挂载到地牢场景中
/// </summary>
[Log]
[ContextAware]
[GlobalClass]
public partial class HeapNode : Node2D
{
    /// <summary>地图位置</summary>
    public int Pos { get; set; }

    /// <summary>物品列表</summary>
    public List<Item> Items { get; } = new();

    /// <summary>堆类型</summary>
    public HeapType Type { get; set; } = HeapType.Heap;

    /// <summary>是否已被看到</summary>
    public bool Seen { get; set; }

    /// <summary>是否怨灵（被诅咒物品）</summary>
    public bool Haunted { get; set; }

    /// <summary>精灵图索引</summary>
    public int Image { get; set; }

    /// <summary>
    ///     添加物品到堆
    /// </summary>
    public void AddItem(Item item)
    {
        if (item == null) return;

        // 尝试堆叠
        if (item.Stackable)
        {
            foreach (var existing in Items)
            {
                if (existing.Merge(item))
                {
                    if (item.Quantity <= 0) return;
                }
            }
        }

        Items.Add(item);
        UpdateVisual();
    }

    /// <summary>
    ///     移除物品
    /// </summary>
    public Item? RemoveItem(Item item)
    {
        if (Items.Remove(item))
        {
            UpdateVisual();
            return item;
        }
        return null;
    }

    /// <summary>
    ///     拾取所有物品（返回列表中移除）
    /// </summary>
    public List<Item> TakeAll()
    {
        var taken = new List<Item>(Items);
        Items.Clear();
        UpdateVisual();
        return taken;
    }

    /// <summary>
    ///     顶部物品
    /// </summary>
    public Item? TopItem => Items.Count > 0 ? Items[^1] : null;

    /// <summary>
    ///     是否为空
    /// </summary>
    public bool IsEmpty => Items.Count == 0;

    /// <summary>
    ///     更新精灵显示
    /// </summary>
    private void UpdateVisual()
    {
        // TODO: 根据物品类型和数量更新精灵
        Visible = !IsEmpty;
    }

    /// <summary>
    ///     自动拾取（如果满足条件）
    /// </summary>
    public bool AutoPickup()
    {
        // 金币自动拾取
        if (Items.Count == 1 && Items[0] is Gold)
        {
            return true;
        }
        return false;
    }
}