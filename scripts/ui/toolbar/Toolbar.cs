using GFramework.Core.extensions;
using GFramework.Godot.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.core.intent;
using MyShatteredPixelDungeon.scripts.cqrs.intent.@event;
using Godot;

namespace MyShatteredPixelDungeon.scripts.ui.toolbar;

/// <summary>
///     动作工具栏，显示在屏幕底部
///     对应原版 com.shatteredpixel.shatteredpixeldungeon.ui.Toolbar
/// </summary>
[Log]
[ContextAware]
public partial class Toolbar : Control
{
    private Button AttackBtn => GetNode<Button>("%AttackBtn");
    private Button WaitBtn => GetNode<Button>("%WaitBtn");
    private Button RestBtn => GetNode<Button>("%RestBtn");
    private Button InventoryBtn => GetNode<Button>("%InventoryBtn");

    public override void _Ready()
    {
        AttackBtn.Pressed += () => this.SendEvent(new IntentGeneratedEvent { Intent = new InteractIntent(0) });
        WaitBtn.Pressed += () => this.SendEvent(new IntentGeneratedEvent { Intent = new WaitIntent() });
        RestBtn.Pressed += () => this.SendEvent(new IntentGeneratedEvent { Intent = new RestIntent() });
        InventoryBtn.Pressed += () => _log.Info("背包（待实现）");
    }
}