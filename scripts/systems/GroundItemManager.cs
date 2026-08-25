using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.items;

namespace MyShatteredPixelDungeon.scripts.systems;

/// <summary>
///     地面物品管理器，管理当前地牢层中的所有 HeapNode
/// </summary>
public static class GroundItemManager
{
    /// <summary>所有地面物品堆</summary>
    private static readonly Dictionary<int, HeapNode> _heapsByPos = new();

    /// <summary>
    ///     在指定位置创建物品堆
    /// </summary>
    public static HeapNode Drop(int pos, Item item)
    {
        if (_heapsByPos.TryGetValue(pos, out var existing))
        {
            existing.AddItem(item);
            return existing;
        }

        var heap = new HeapNode { Pos = pos };
        heap.AddItem(item);
        _heapsByPos[pos] = heap;
        return heap;
    }

    /// <summary>
    ///     从指定位置拾取物品
    /// </summary>
    public static Item? PickUp(int pos, HeroEntity hero)
    {
        if (!_heapsByPos.TryGetValue(pos, out var heap)) return null;
        if (heap.IsEmpty)
        {
            _heapsByPos.Remove(pos);
            return null;
        }

        var item = heap.TopItem;
        if (item == null) return null;

        // 尝试放入背包
        if (InventorySystem.PickUpItem(hero, item))
        {
            heap.RemoveItem(item);
            if (heap.IsEmpty) _heapsByPos.Remove(pos);
            return item;
        }
        return null;
    }

    /// <summary>
    ///     拾取指定位置的所有物品
    /// </summary>
    public static List<Item> PickUpAll(int pos, HeroEntity hero)
    {
        if (!_heapsByPos.TryGetValue(pos, out var heap)) return new List<Item>();

        var taken = new List<Item>();
        foreach (var item in heap.Items.ToList())
        {
            if (InventorySystem.PickUpItem(hero, item))
            {
                taken.Add(item);
            }
        }

        // 从堆中移除已拾取的物品
        foreach (var item in taken) heap.RemoveItem(item);
        if (heap.IsEmpty) _heapsByPos.Remove(pos);

        return taken;
    }

    /// <summary>
    ///     获取指定位置的物品堆
    /// </summary>
    public static HeapNode? GetHeap(int pos)
    {
        return _heapsByPos.GetValueOrDefault(pos);
    }

    /// <summary>
    ///     指定位置是否有物品
    /// </summary>
    public static bool HasItems(int pos)
    {
        return _heapsByPos.ContainsKey(pos) && !_heapsByPos[pos].IsEmpty;
    }

    /// <summary>
    ///     自动拾取（金币自动拾取）
    /// </summary>
    public static bool AutoPickup(int pos, HeroEntity hero)
    {
        if (!_heapsByPos.TryGetValue(pos, out var heap)) return false;
        if (!heap.AutoPickup()) return false;

        var gold = heap.TopItem;
        if (gold != null && InventorySystem.PickUpItem(hero, gold))
        {
            heap.RemoveItem(gold);
            if (heap.IsEmpty) _heapsByPos.Remove(pos);
            return true;
        }
        return false;
    }

    /// <summary>
    ///     清空所有物品堆（楼层切换时）
    /// </summary>
    public static void Clear()
    {
        _heapsByPos.Clear();
    }

    /// <summary>
    ///     获取所有物品堆
    /// </summary>
    public static IEnumerable<HeapNode> AllHeaps => _heapsByPos.Values;

    /// <summary>
    ///     物品堆数量
    /// </summary>
    public static int Count => _heapsByPos.Count;
}