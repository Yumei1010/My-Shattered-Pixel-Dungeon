using GFramework.Core.Abstractions.command;
using GFramework.Core.command;

namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.command;

/// <summary>
///     使用物品命令输入
/// </summary>
public sealed class UseItemCommandInput : ICommandInput
{
    public required int EntityId { get; init; }
    public required int InventoryIndex { get; init; }
    public int? TargetPos { get; init; }
}

/// <summary>
///     使用物品命令
/// </summary>
public sealed class UseItemCommand : AbstractAsyncCommand<UseItemCommandInput>
{
    public UseItemCommand(UseItemCommandInput input) : base(input) { }

    protected override async Task OnExecuteAsync(UseItemCommandInput input)
    {
        await Task.CompletedTask;
    }
}