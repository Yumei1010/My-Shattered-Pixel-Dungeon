using GFramework.Core.extensions;
using GFramework.Godot.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.cqrs.movement.@event;
using MyShatteredPixelDungeon.scripts.entities;
using Godot;

namespace MyShatteredPixelDungeon.scripts.core.camera;

/// <summary>
///     游戏相机控制器，跟随英雄移动
///     使用 Camera2D 节点，支持平滑跟随和缩放
/// </summary>
[Log]
[ContextAware]
public partial class GameCamera : Camera2D
{
    /// <summary>平滑跟随速度（0=瞬间，越大越慢）</summary>
    [Export] public float FollowSpeed { get; set; } = 5f;

    /// <summary>最小缩放</summary>
    [Export] public float MinZoom { get; set; } = 0.5f;

    /// <summary>最大缩放</summary>
    [Export] public float MaxZoom { get; set; } = 2f;

    /// <summary>缩放步长</summary>
    [Export] public float ZoomStep { get; set; } = 0.25f;

    private int _targetId = -1;

    public override void _Ready()
    {
        // 自动查找英雄
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();
        if (hero != null) _targetId = hero.Id;

        this.RegisterEvent<CharMovedEvent>(OnCharMoved)
            .UnRegisterWhenNodeExitTree(this);

        // 初始缩放
        Zoom = new Vector2(2f, 2f);
    }

    public override void _Process(double delta)
    {
        if (_targetId <= 0) return;

        var target = Actor.FindById(_targetId) as CharEntity;
        if (target == null) return;

        // 平滑跟随
        var targetPos = new Vector2(target.Pos % 64 * 16, target.Pos / 64 * 16);
        GlobalPosition = GlobalPosition.Lerp(targetPos, (float)delta * FollowSpeed);
    }

    public override void _Input(InputEvent @event)
    {
        // 滚轮缩放
        if (@event is InputEventMouseButton { Pressed: true } mouse)
        {
            var newZoom = Zoom;
            if (mouse.ButtonIndex == MouseButton.WheelUp)
                newZoom = new Vector2(
                    Mathf.Clamp(Zoom.X + ZoomStep, MinZoom, MaxZoom),
                    Mathf.Clamp(Zoom.Y + ZoomStep, MinZoom, MaxZoom));
            else if (mouse.ButtonIndex == MouseButton.WheelDown)
                newZoom = new Vector2(
                    Mathf.Clamp(Zoom.X - ZoomStep, MinZoom, MaxZoom),
                    Mathf.Clamp(Zoom.Y - ZoomStep, MinZoom, MaxZoom));

            if (newZoom != Zoom)
            {
                Zoom = newZoom;
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void OnCharMoved(CharMovedEvent e)
    {
        if (e.EntityId == _targetId)
        {
            // 立即移动到新位置（smooth follow 在 _Process 中处理）
        }
    }

    /// <summary>
    ///     绑定跟随目标
    /// </summary>
    public void Follow(CharEntity target)
    {
        _targetId = target.Id;
    }
}