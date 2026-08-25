namespace MyShatteredPixelDungeon.scripts.cqrs.combat.@event;

/// <summary>
///     角色死亡事件
/// </summary>
public sealed class CharDiedEvent
{
    public required int EntityId { get; init; }
    public required string Source { get; init; }
}