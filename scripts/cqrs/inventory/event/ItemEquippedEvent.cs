namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;

/// <summary>
///     物品装备事件
/// </summary>
public sealed class ItemEquippedEvent
{
    /// <summary>装备者 ID</summary>
    public required int EntityId { get; init; }

    /// <summary>装备的物品类型</summary>
    public required string ItemType { get; init; }

    /// <summary>装备槽位</summary>
    public required string Slot { get; init; }
}

/// <summary>
///     物品卸下事件
/// </summary>
public sealed class ItemUnequippedEvent
{
    /// <summary>卸下者 ID</summary>
    public required int EntityId { get; init; }

    /// <summary>卸下的物品类型</summary>
    public required string ItemType { get; init; }

    /// <summary>装备槽位</summary>
    public required string Slot { get; init; }
}