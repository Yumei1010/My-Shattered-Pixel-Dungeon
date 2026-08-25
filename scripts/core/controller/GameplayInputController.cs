using GFramework.Core.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.core.intent;
using MyShatteredPixelDungeon.scripts.cqrs.intent.@event;
using MyShatteredPixelDungeon.scripts.enums.input;
using Godot;

namespace MyShatteredPixelDungeon.scripts.core.controller;

/// <summary>
///     游戏输入控制器，处理鼠标点击和键盘输入，生成意图
///     对应原版 GameScene 的输入处理逻辑
/// </summary>
[Log]
[ContextAware]
public partial class GameplayInputController : GameInputController
{
    protected override bool AcceptPhase(InputPhase phase) => phase == InputPhase.Gameplay;

    protected override void Handle(InputPhase phase, InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse:
                HandleLeftClick(mouse);
                break;

            case InputEventKey { Pressed: true, Keycode: Key.Space }:
                this.SendEvent(new IntentGeneratedEvent { Intent = new WaitIntent() });
                break;

            case InputEventKey { Pressed: true, Keycode: Key.Z }:
                this.SendEvent(new IntentGeneratedEvent { Intent = new RestIntent() });
                break;
        }
    }

    /// <summary>
    ///     处理鼠标左键点击
    /// </summary>
    private void HandleLeftClick(InputEventMouseButton mouse)
    {
        var cell = ScreenToCell(mouse.Position);
        if (!cell.HasValue) return;

        // 生成交互意图，由 IntentInterpreter 解析为具体命令
        this.SendEvent(new IntentGeneratedEvent
        {
            Intent = new InteractIntent(cell.Value)
        });
    }

    /// <summary>
    ///     屏幕坐标转地图格子（暂作占位，后续实现）
    /// </summary>
    private static int? ScreenToCell(Vector2 screenPos)
    {
        // 简化：需要接入 TileMap 和 Camera 后实现
        return null;
    }
}