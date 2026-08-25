namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;

/// <summary>
///     物品丢弃事件
/// </summary>
public sealed class ItemDroppedEvent
{
    /// <summary>丢弃者 ID</summary>
    public required int EntityId { get; init; }

    /// <summary>物品类型</summary>
    public required string ItemType { get; init; }

    /// <summary>丢弃位置</summary>
    public required int Position { get; init; }
}