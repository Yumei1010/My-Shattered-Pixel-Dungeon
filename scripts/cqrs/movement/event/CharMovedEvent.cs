namespace MyShatteredPixelDungeon.scripts.cqrs.movement.@event;

/// <summary>
///     角色移动事件
/// </summary>
public sealed class CharMovedEvent
{
    public required int EntityId { get; init; }
    public required int From { get; init; }
    public required int To { get; init; }
}