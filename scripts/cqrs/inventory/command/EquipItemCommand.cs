using GFramework.Core.Abstractions.command;
using GFramework.Core.command;

namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.command;

/// <summary>
///     装备物品命令输入
/// </summary>
public sealed class EquipItemCommandInput : ICommandInput
{
    public required int EntityId { get; init; }
    public required int InventoryIndex { get; init; }
}

/// <summary>
///     装备物品命令
/// </summary>
public sealed class EquipItemCommand : AbstractAsyncCommand<EquipItemCommandInput>
{
    public EquipItemCommand(EquipItemCommandInput input) : base(input) { }

    protected override async Task OnExecuteAsync(EquipItemCommandInput input)
    {
        await Task.CompletedTask;
    }
}

/// <summary>
///     卸下物品命令输入
/// </summary>
public sealed class UnequipItemCommandInput : ICommandInput
{
    public required int EntityId { get; init; }
    public required string Slot { get; init; }
}

/// <summary>
///     卸下物品命令
/// </summary>
public sealed class UnequipItemCommand : AbstractAsyncCommand<UnequipItemCommandInput>
{
    public UnequipItemCommand(UnequipItemCommandInput input) : base(input) { }

    protected override async Task OnExecuteAsync(UnequipItemCommandInput input)
    {
        await Task.CompletedTask;
    }
}