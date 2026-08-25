namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;

/// <summary>
///     背包变化事件（UI 刷新用）
/// </summary>
public sealed class InventoryChangedEvent
{
    /// <summary>所属实体 ID</summary>
    public required int EntityId { get; init; }

    /// <summary>变化类型：Add/Remove/Update</summary>
    public required string ChangeType { get; init; }

    /// <summary>变化的物品类型</summary>
    public required string ItemType { get; init; }

    /// <summary>当前物品数量</summary>
    public required int Quantity { get; init; }
}