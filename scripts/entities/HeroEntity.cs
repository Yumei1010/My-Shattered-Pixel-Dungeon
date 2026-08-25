namespace MyShatteredPixelDungeon.scripts.entities;

/// <summary>
///     英雄实体，对应原版 Hero
///     持有职业、天赋、背包、动作系统
/// </summary>
public sealed class HeroEntity : CharEntity
{
    public override int ActPriority => ActorPriority.Hero;

    /// <summary>
    ///     是否有待处理的玩家指令
    /// </summary>
    public bool HasAction => CurAction != null;

    /// <summary>当前动作</summary>
    public object? CurAction { get; set; }

    /// <summary>上一个动作</summary>
    public object? LastAction { get; set; }

    /// <summary>是否就绪（等待输入）</summary>
    public bool Ready { get; set; }

    public HeroEntity()
    {
        Alignment = Alignment.Ally;
        MaxHp = 30;
        Hp = 30;
        Strength = 10;
        ViewDistance = 8;
    }

    protected override bool Act()
    {
        // 更新视野
        // 处理 Buff
        // 如果有待执行动作则执行，否则等待输入
        return HasAction;
    }

    /// <summary>
    ///     标记为就绪（等待玩家输入）
    /// </summary>
    public void SetReady()
    {
        Ready = true;
        CurAction = null;
    }
}