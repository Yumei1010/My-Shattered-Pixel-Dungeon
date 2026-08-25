using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.items;

/// <summary>
///     物品默认动作常量
/// </summary>
public static class ItemAction
{
    public const string Apply = "APPLY";
    public const string Zap = "ZAP";
    public const string Throw = "THROW";
    public const string Equip = "EQUIP";
    public const string Unequip = "UNEQUIP";
    public const string Eat = "EAT";
    public const string Drink = "DRINK";
    public const string Read = "READ";
    public const string Open = "OPEN";
    public const string Wear = "WEAR";
    public const string Remove = "REMOVE";
    public const string Drop = "DROP";
}

/// <summary>
///     物品基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.Item
///     所有物品类型的基类，持有通用属性
/// </summary>
public abstract class Item
{
    /// <summary>精灵图索引</summary>
    public int Image { get; set; }

    /// <summary>图标标识符（随机物品用）</summary>
    public int Icon { get; set; }

    /// <summary>是否可堆叠</summary>
    public virtual bool Stackable { get; init; }

    /// <summary>数量</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>最大堆叠数量</summary>
    public virtual int MaxStack => int.MaxValue;

    /// <summary>等级（升级/强化用）</summary>
    public int Level { get; set; }

    /// <summary>等级是否已知</summary>
    public bool LevelKnown { get; set; }

    /// <summary>是否诅咒</summary>
    public bool Cursed { get; set; }

    /// <summary>诅咒是否已知</summary>
    public bool CursedKnown { get; set; }

    /// <summary>唯一物品（复活保留）</summary>
    public bool Unique { get; init; }

    /// <summary>死亡保留</summary>
    public bool KeptThoughLostInvent { get; init; }

    /// <summary>是否可出现在遗骸中</summary>
    public bool Bones { get; init; } = true;

    /// <summary>默认动作</summary>
    public virtual string DefaultAction => "";

    /// <summary>需要目标选择</summary>
    public virtual bool UsesTargeting => false;

    /// <summary>是否已鉴定</summary>
    public bool IsIdentified => LevelKnown && CursedKnown;

    /// <summary>是否为升级物品（SoU）</summary>
    public virtual bool IsUpgradable => false;

    /// <summary>是否可附魔</summary>
    public virtual bool IsEnchantable => false;

    /// <summary>显示名称（含等级前缀）</summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    ///     鉴定物品
    /// </summary>
    public void Identify()
    {
        LevelKnown = true;
        CursedKnown = true;
    }

    /// <summary>
    ///     获取可用操作列表
    /// </summary>
    public virtual List<string> Actions(HeroEntity hero)
    {
        var actions = new List<string>();
        if (DefaultAction != "") actions.Add(DefaultAction);
        actions.Add(ItemAction.Throw);
        actions.Add(ItemAction.Drop);
        return actions;
    }

    /// <summary>
    ///     拾取
    /// </summary>
    public virtual bool DoPickUp(HeroEntity hero) => false;

    /// <summary>
    ///     丢弃
    /// </summary>
    public virtual void DoDrop(HeroEntity hero) { }

    /// <summary>
    ///     投掷
    /// </summary>
    public virtual void DoThrow(HeroEntity hero) { }

    /// <summary>
    ///     放入容器
    /// </summary>
    public virtual bool Collect(ItemContainer container)
    {
        if (container.TryAdd(this))
        {
            OnCollect(container);
            return true;
        }
        return false;
    }

    /// <summary>
    ///     从容器移除
    /// </summary>
    public virtual Item? Detach(ItemContainer container)
    {
        if (Stackable && Quantity > 1)
        {
            Quantity--;
            // 返回一个副本
            var copy = (Item)MemberwiseClone();
            copy.Quantity = 1;
            return copy;
        }
        return container.Remove(this);
    }

    /// <summary>
    ///     全部取出
    /// </summary>
    public virtual Item? DetachAll(ItemContainer container)
    {
        return container.Remove(this);
    }

    /// <summary>
    ///     放入容器时回调
    /// </summary>
    protected virtual void OnCollect(ItemContainer container) { }

    /// <summary>
    ///     从容器移除时回调
    /// </summary>
    protected virtual void OnDetach(ItemContainer container) { }

    /// <summary>
    ///     描述信息
    /// </summary>
    public virtual string Info() => Name;

    /// <summary>
    ///     克隆物品
    /// </summary>
    public virtual Item Clone()
    {
        return (Item)MemberwiseClone();
    }

    /// <summary>
    ///     是否相同类型（用于堆叠判断）
    /// </summary>
    public virtual bool IsSimilar(Item other)
    {
        return GetType() == other.GetType();
    }

    /// <summary>
    ///     合并堆叠（从 other 合并到 this）
    /// </summary>
    public virtual bool Merge(Item other)
    {
        if (!Stackable || !IsSimilar(other)) return false;
        int space = MaxStack - Quantity;
        if (space <= 0) return false;
        int transfer = Math.Min(space, other.Quantity);
        Quantity += transfer;
        other.Quantity -= transfer;
        return true;
    }

    public override string ToString() => Name;
}