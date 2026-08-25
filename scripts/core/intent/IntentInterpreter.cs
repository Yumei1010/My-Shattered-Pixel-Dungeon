using GFramework.Core.Abstractions.command;
using GFramework.Core.command;
using MyShatteredPixelDungeon.scripts.dungeon;
using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.core.intent;

/// <summary>
///     意图解释器，将玩家意图解析为具体命令序列
///     对应原版 Hero.handle(cell) 的逻辑
/// </summary>
public static class IntentInterpreter
{
    /// <summary>
    ///     解析意图为命令序列
    /// </summary>
    public static List<ICommand> Interpret(Intent intent, HeroEntity hero, DungeonData? level)
    {
        return intent switch
        {
            MoveIntent mi => InterpretMove(mi, hero, level),
            InteractIntent ii => InterpretInteract(ii, hero, level),
            WaitIntent _ => new List<ICommand> { new WaitCommand() },
            RestIntent _ => new List<ICommand> { new RestCommand() },
            ExamineIntent ei => InterpretExamine(ei, level),
            UseItemIntent ui => new List<ICommand> { new UseItemCommand(ui.ItemSlot, ui.TargetCell) },
            _ => new List<ICommand>()
        };
    }

    private static List<ICommand> InterpretMove(MoveIntent mi, HeroEntity hero, DungeonData? level)
    {
        var target = mi.TargetCell;
        var commands = new List<ICommand>();

        // 1. 检查目标格是否有敌人 → 攻击
        var enemy = Actor.FindChar(target);
        if (enemy != null && enemy.Alignment != Alignment.Ally && hero.CanAttack(enemy))
        {
            commands.Add(new AttackCommand(hero, enemy));
            return commands;
        }

        // 2. 检查是否有物品堆 → 移动并拾取
        // 简化：暂不处理物品堆

        // 3. 检查是否是楼梯 → 切换楼层
        // 简化：暂不处理

        // 4. 检查是否有 NPC → 交互
        // 简化：暂不处理

        // 5. 空地 → 移动
        commands.Add(new MoveCommand(hero, target));
        return commands;
    }

    private static List<ICommand> InterpretInteract(InteractIntent ii, HeroEntity hero, DungeonData? level)
    {
        // 交互意图：点击目标格，由解释器决定具体动作
        return InterpretMove(new MoveIntent(ii.TargetCell), hero, level);
    }

    private static List<ICommand> InterpretExamine(ExamineIntent ei, DungeonData? level)
    {
        // 检查格子信息（后续实现）
        return new List<ICommand>();
    }
}

// ---------- 命令实现 ----------

/// <summary>
///     移动命令
/// </summary>
public sealed class MoveCommand(HeroEntity hero, int target) : AbstractCommand
{
    protected override void OnExecute()
    {
        hero.Move(target);
    }
}

/// <summary>
///     攻击命令
/// </summary>
public sealed class AttackCommand(HeroEntity hero, CharEntity target) : AbstractCommand
{
    protected override void OnExecute()
    {
        hero.Attack(target);
    }
}

/// <summary>
///     等待命令
/// </summary>
public sealed class WaitCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        // 等待一回合
    }
}

/// <summary>
///     休息命令
/// </summary>
public sealed class RestCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        // 休息恢复
    }
}

/// <summary>
///     使用物品命令
/// </summary>
public sealed class UseItemCommand(int itemSlot, int? targetCell) : AbstractCommand
{
    protected override void OnExecute()
    {
        // 使用物品（后续实现）
    }
}