namespace MyShatteredPixelDungeon.scripts.entities.buffs;

/// <summary>
///     隐身效果，怪物无法发现玩家
/// </summary>
public sealed class InvisibilityBuff : Buff
{
    public InvisibilityBuff() { Type = BuffType.Positive; }

    public override string Name => "隐身";
    public override string Description => "怪物无法发现你";
}