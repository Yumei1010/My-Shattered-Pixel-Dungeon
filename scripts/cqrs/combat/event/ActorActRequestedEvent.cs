namespace MyShatteredPixelDungeon.scripts.cqrs.combat.@event;

/// <summary>
///     Actor 行动请求事件，通知某 Actor 执行 act()
/// </summary>
public sealed class ActorActRequestedEvent
{
    public required int ActorId { get; init; }
}