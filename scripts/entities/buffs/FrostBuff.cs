namespace MyShatteredPixelDungeon.scripts.entities.buffs;

/// <summary>
///     冰冻效果，减速并可能冻结
/// </summary>
public sealed class FrostBuff : Buff
{
    public FrostBuff() { Type = BuffType.Negative; }

    /// <summary>速度倍率</summary>
    public float SpeedMultiplier => 0.5f;

    /// <summary>是否完全冻结</summary>
    public bool IsFrozen { get; set; }

    public override string Name => "冰冻";
    public override string Description => IsFrozen ? "完全冻结，无法行动" : "移动速度减半";
}