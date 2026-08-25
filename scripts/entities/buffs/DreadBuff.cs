namespace MyShatteredPixelDungeon.scripts.entities.buffs;

/// <summary>
///     威慑效果，强制逃跑（比恐惧更强）
/// </summary>
public sealed class DreadBuff : Buff
{
    public DreadBuff() { Type = BuffType.Negative; }
}