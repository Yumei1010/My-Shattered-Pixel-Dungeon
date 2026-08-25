namespace MyShatteredPixelDungeon.scripts.cqrs.intent.@event;

/// <summary>
///     意图生成事件，由 InputController 触发
///     各系统可订阅此事件以响应玩家意图
/// </summary>
public sealed class IntentGeneratedEvent
{
    public required object Intent { get; init; }
}