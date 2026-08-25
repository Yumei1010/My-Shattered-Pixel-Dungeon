using GFramework.Core.Abstractions.command;
using GFramework.Core.command;

namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.command;

/// <summary>
///     丢弃物品命令输入
/// </summary>
public sealed class DropItemCommandInput : ICommandInput
{
    public required int EntityId { get; init; }
    public required int InventoryIndex { get; init; }
    public required int Position { get; init; }
}

/// <summary>
///     丢弃物品命令
/// </summary>
public sealed class DropItemCommand : AbstractAsyncCommand<DropItemCommandInput>
{
    public DropItemCommand(DropItemCommandInput input) : base(input) { }

    protected override async Task OnExecuteAsync(DropItemCommandInput input)
    {
        await Task.CompletedTask;
    }
}