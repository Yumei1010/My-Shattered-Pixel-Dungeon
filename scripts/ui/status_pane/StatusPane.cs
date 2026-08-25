using GFramework.Core.extensions;
using GFramework.Godot.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.cqrs.combat.@event;
using MyShatteredPixelDungeon.scripts.entities;
using Godot;

namespace MyShatteredPixelDungeon.scripts.ui.status_pane;

/// <summary>
///     状态栏，显示在屏幕左上角
///     对应原版 com.shatteredpixel.shatteredpixeldungeon.ui.StatusPane
///     显示 HP、饥饿、层数、金币
/// </summary>
[Log]
[ContextAware]
public partial class StatusPane : Control
{
    private Label HpLabel => GetNode<Label>("%HpLabel");
    private Label DepthLabel => GetNode<Label>("%DepthLabel");
    private Label GoldLabel => GetNode<Label>("%GoldLabel");
    private TextureProgressBar HpBar => GetNode<TextureProgressBar>("%HpBar");

    private int _heroId = -1;
    private int _maxHp = 30;
    private int _depth = 1;

    public override void _Ready()
    {
        // 自动查找英雄
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();
        if (hero != null)
        {
            _heroId = hero.Id;
            _maxHp = hero.MaxHp;
            UpdateAll(hero);
        }

        this.RegisterEvent<CharDamagedEvent>(OnCharDamaged)
            .UnRegisterWhenNodeExitTree(this);

        // 初始显示
        HpLabel.Text = $"HP: {_maxHp}/{_maxHp}";
        HpBar.Value = 100;
        DepthLabel.Text = $"层数: {_depth}F";
        GoldLabel.Text = "金币: 0";
    }

    /// <summary>
    ///     设置层数
    /// </summary>
    public void SetDepth(int depth)
    {
        _depth = depth;
        DepthLabel.Text = $"层数: {_depth}F";
    }

    /// <summary>
    ///     设置金币
    /// </summary>
    public void SetGold(int gold)
    {
        GoldLabel.Text = $"金币: {gold}";
    }

    /// <summary>
    ///     更新所有状态
    /// </summary>
    public void UpdateAll(HeroEntity hero)
    {
        _maxHp = hero.MaxHp;
        HpLabel.Text = $"HP: {hero.Hp}/{_maxHp}";
        HpBar.Value = (double)hero.Hp / _maxHp * 100;
    }

    private void OnCharDamaged(CharDamagedEvent e)
    {
        if (e.EntityId != _heroId) return;
        _maxHp = GetMaxHp();
        HpLabel.Text = $"HP: {e.RemainingHp}/{_maxHp}";
        HpBar.Value = (double)e.RemainingHp / _maxHp * 100;
    }

    private int GetMaxHp()
    {
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();
        return hero?.MaxHp ?? _maxHp;
    }
}