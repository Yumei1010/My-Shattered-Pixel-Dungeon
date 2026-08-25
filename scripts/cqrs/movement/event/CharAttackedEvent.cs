namespace MyShatteredPixelDungeon.scripts.cqrs.movement.@event;

/// <summary>
///     角色攻击事件
/// </summary>
public sealed class CharAttackedEvent
{
    public required int AttackerId { get; init; }
    public required int TargetId { get; init; }
    public required int Damage { get; init; }
    public required bool Hit { get; init; }
}