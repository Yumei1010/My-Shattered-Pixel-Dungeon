namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.@event;

/// <summary>
///     物品使用事件
/// </summary>
public sealed class ItemUsedEvent
{
    /// <summary>使用者 ID</summary>
    public required int EntityId { get; init; }

    /// <summary>物品类型</summary>
    public required string ItemType { get; init; }

    /// <summary>使用效果简述</summary>
    public required string Effect { get; init; }
}