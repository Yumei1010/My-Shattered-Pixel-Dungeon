namespace MyShatteredPixelDungeon.scripts.cqrs.combat.@event;

/// <summary>
///     Actor 行动完成事件
/// </summary>
public sealed class ActorActCompletedEvent
{
    public required int ActorId { get; init; }
}