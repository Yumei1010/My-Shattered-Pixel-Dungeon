namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;

/// <summary>
///     物品拾取事件
/// </summary>
public sealed class ItemPickedUpEvent
{
    /// <summary>拾取者 ID</summary>
    public required int EntityId { get; init; }

    /// <summary>物品类型</summary>
    public required string ItemType { get; init; }

    /// <summary>物品数量</summary>
    public required int Quantity { get; init; }
}