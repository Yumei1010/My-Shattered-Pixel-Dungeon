using GFramework.Core.system;
using GFramework.Core.extensions;
using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.cqrs.combat.@event;

namespace MyShatteredPixelDungeon.scripts.systems;

/// <summary>
///     回合调度系统，对应原版 Actor.process()
///     每帧检查是否有 Actor 需要行动，按时间+优先级顺序调度
/// </summary>
public sealed class TurnSystem : AbstractSystem
{
    private bool _processing;

    protected override void OnInit()
    {
        InitTurn();
    }

    /// <summary>
    ///     是否正在处理回合
    /// </summary>
    public bool IsProcessing => _processing;

    /// <summary>
    ///     推进一个 Actor 的行动
    ///     每帧调用一次，由 GameLoop 或 Godot _Process 驱动
    /// </summary>
    /// <returns>true 表示还有更多 Actor 待行动，false 表示等待玩家输入</returns>
    public bool ProcessTurn()
    {
        // 找到时间最早的 Actor（时间相同则优先级高的优先）
        Actor? next = null;
        float earliest = float.MaxValue;

        foreach (var actor in Actor.All())
        {
            if (!actor.IsActive) continue;
            if (actor.Time < earliest ||
                (actor.Time == earliest && (next == null || actor.ActPriority > next.ActPriority)))
            {
                earliest = actor.Time;
                next = actor;
            }
        }

        if (next == null) return false;

        Actor.Current = next;
        Actor.Now = next.Time;

        // 如果是英雄且无输入 → 暂停等待
        if (next is HeroEntity hero && !hero.HasAction)
        {
            _processing = false;
            Actor.Current = null;
            return false;
        }

        // 执行行动
        _processing = true;
        this.SendEvent(new ActorActRequestedEvent { ActorId = next.Id });

        bool continueLoop = next.ExecuteAct();

        this.SendEvent(new ActorActCompletedEvent { ActorId = next.Id });

        // 如果英雄死亡或行动需等待 → 暂停
        if (!continueLoop || (next is HeroEntity h && !h.IsAlive))
        {
            _processing = false;
            return false;
        }

        Actor.Current = null;
        return true;
    }

    /// <summary>
    ///     初始化回合（添加所有 Actor 到调度）
    /// </summary>
    public void InitTurn()
    {
        Actor.Clear();
        Actor.Now = 0;
        _processing = false;
    }

    /// <summary>
    ///     等待玩家输入（英雄行动后调用）
    /// </summary>
    public void WaitForInput()
    {
        _processing = false;
        Actor.Current = null;
    }
}