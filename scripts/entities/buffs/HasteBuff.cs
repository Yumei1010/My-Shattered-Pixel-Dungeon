namespace MyShatteredPixelDungeon.scripts.entities.buffs;

/// <summary>
///     疾跑效果，移动速度加倍
/// </summary>
public sealed class HasteBuff : Buff
{
    public HasteBuff() { Type = BuffType.Positive; }

    /// <summary>速度倍率</summary>
    public float SpeedMultiplier => 2f;

    public override string Name => "疾跑";
    public override string Description => "移动速度加倍";
}