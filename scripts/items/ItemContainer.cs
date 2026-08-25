namespace MyShatteredPixelDungeon.scripts.items;

/// <summary>
///     物品容器，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.ItemContainer
///     持有物品列表，支持堆叠、容量限制
/// </summary>
public class ItemContainer
{
    /// <summary>物品列表</summary>
    public List<Item> Items { get; } = new();

    /// <summary>最大容量（-1 表示无限制）</summary>
    public int MaxSize { get; init; } = -1;

    /// <summary>是否已满</summary>
    public bool IsFull => MaxSize > 0 && Items.Count >= MaxSize;

    /// <summary>
    ///     尝试添加物品
    /// </summary>
    public virtual bool TryAdd(Item item)
    {
        if (item == null) return false;

        // 尝试堆叠到已有物品
        if (item.Stackable)
        {
            foreach (var existing in Items)
            {
                if (existing.Merge(item))
                {
                    // item 被合并后如果数量为 0，视为已处理
                    if (item.Quantity <= 0) return true;
                }
            }
        }

        // 需要新槽位
        if (IsFull) return false;

        // 添加到列表
        Items.Add(item);
        return true;
    }

    /// <summary>
    ///     移除指定物品实例
    /// </summary>
    public virtual Item? Remove(Item item)
    {
        if (Items.Remove(item))
        {
            return item;
        }
        return null;
    }

    /// <summary>
    ///     移除指定类型的物品
    /// </summary>
    public virtual Item? Remove<T>() where T : Item
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is T)
            {
                var item = Items[i];
                Items.RemoveAt(i);
                return item;
            }
        }
        return null;
    }

    /// <summary>
    ///     查找指定类型的物品
    /// </summary>
    public T? Find<T>() where T : Item
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is T t) return t;
        }
        return null;
    }

    /// <summary>
    ///     查找所有指定类型的物品
    /// </summary>
    public List<T> FindAll<T>() where T : Item
    {
        var result = new List<T>();
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is T t) result.Add(t);
        }
        return result;
    }

    /// <summary>
    ///     统计指定类型的数量
    /// </summary>
    public int Count<T>() where T : Item
    {
        int count = 0;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is T t) count += t.Quantity;
        }
        return count;
    }

    /// <summary>
    ///     清空容器
    /// </summary>
    public void Clear()
    {
        Items.Clear();
    }

    /// <summary>
    ///     是否包含指定物品
    /// </summary>
    public bool Contains(Item item) => Items.Contains(item);

    /// <summary>
    ///     是否包含指定类型的物品
    /// </summary>
    public bool Contains<T>() where T : Item => Find<T>() != null;
}