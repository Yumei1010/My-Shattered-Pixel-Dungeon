using GFramework.Core.Abstractions.command;
using GFramework.Core.command;

namespace MyShatteredPixelDungeon.scripts.cqrs.inventory.command;

/// <summary>
///     拾取物品命令输入
/// </summary>
public sealed class PickUpItemCommandInput : ICommandInput
{
    public required int EntityId { get; init; }
    public required int Position { get; init; }
}

/// <summary>
///     拾取物品命令
/// </summary>
public sealed class PickUpItemCommand : AbstractAsyncCommand<PickUpItemCommandInput>
{
    public PickUpItemCommand(PickUpItemCommandInput input) : base(input) { }

    protected override async Task OnExecuteAsync(PickUpItemCommandInput input)
    {
        await Task.CompletedTask;
    }
}