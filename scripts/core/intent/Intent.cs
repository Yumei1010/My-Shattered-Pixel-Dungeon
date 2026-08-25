using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.core.intent;

/// <summary>
///     意图基类，表示玩家"想做什么"
///     不关心具体实现，由 IntentInterpreter 解析为命令序列
/// </summary>
public abstract record Intent
{
    /// <summary>触发意图的源格子</summary>
    public int SourceCell { get; init; }

    /// <summary>目标格子</summary>
    public int? TargetCell { get; init; }
}

/// <summary>
///     移动意图（点击空地/楼梯）
/// </summary>
public sealed record MoveIntent(int TargetCell) : Intent
{
    public int TargetCell { get; } = TargetCell;
}

/// <summary>
///     交互意图（点击敌人/物品/NPC/楼梯）
/// </summary>
public sealed record InteractIntent(int TargetCell) : Intent
{
    public int TargetCell { get; } = TargetCell;
}

/// <summary>
///     等待意图（原地等待一回合）
/// </summary>
public sealed record WaitIntent : Intent;

/// <summary>
///     休息意图（原地休息，自动恢复）
/// </summary>
public sealed record RestIntent : Intent;

/// <summary>
///     使用物品意图
/// </summary>
public sealed record UseItemIntent(int ItemSlot, int? TargetCell = null) : Intent;

/// <summary>
///     检查意图（查看格子信息）
/// </summary>
public sealed record ExamineIntent(int TargetCell) : Intent
{
    public int TargetCell { get; } = TargetCell;
}