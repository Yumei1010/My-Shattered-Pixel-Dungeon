namespace MyShatteredPixelDungeon.scripts.cqrs.combat.@event;

/// <summary>
///     角色受伤事件
/// </summary>
public sealed class CharDamagedEvent
{
    public required int EntityId { get; init; }
    public required int Damage { get; init; }
    public required string Source { get; init; }
    public required int RemainingHp { get; init; }
}