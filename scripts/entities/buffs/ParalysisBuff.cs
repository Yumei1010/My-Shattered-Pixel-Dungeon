namespace MyShatteredPixelDungeon.scripts.entities.buffs;

/// <summary>
///     麻痹效果，无法行动
/// </summary>
public sealed class ParalysisBuff : Buff
{
    public ParalysisBuff() { Type = BuffType.Negative; }
}