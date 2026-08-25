namespace MyShatteredPixelDungeon.scripts.entities.buffs;

/// <summary>
///     饥饿度 Buff，对应原版 Hunger
///     每回合消耗饥饿值，为 0 时开始扣血
/// </summary>
public sealed class Hunger : Buff
{
    /// <summary>最大饥饿值（600 = 约 10 分钟）</summary>
    public const float MaxHunger = 600f;

    /// <summary>当前饥饿值</summary>
    public float Value { get; set; } = MaxHunger;

    public Hunger()
    {
        Type = BuffType.Negative;
    }

    public override void OnAct()
    {
        if (Value > 0)
        {
            Value -= 1f;
            if (Value <= 0 && Target != null && Target.IsAlive)
            {
                // 饥饿伤害：每回合 1-2 点
                int dmg = Random.Shared.Next(1, 3);
                Target.Damage(dmg, this);
            }
        }
    }

    /// <summary>
    ///     恢复饥饿值
    /// </summary>
    public void Eat(float amount)
    {
        Value = Math.Min(MaxHunger, Value + amount);
    }

    /// <summary>
    ///     是否饥饿（低于 1/3）
    /// </summary>
    public bool IsStarving => Value <= MaxHunger / 3;

    public override string Name => "饥饿";
    public override string Description => "每回合消耗饥饿值，为 0 时开始扣血";
}