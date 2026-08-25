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
    private Label HungerLabel => GetNode<Label>("%HungerLabel");
    private Label DepthLabel => GetNode<Label>("%DepthLabel");
    private Label GoldLabel => GetNode<Label>("%GoldLabel");
    private TextureProgressBar HpBar => GetNode<TextureProgressBar>("%HpBar");

    private int _heroId = -1;

    public override void _Ready()
    {
        this.RegisterEvent<CharDamagedEvent>(OnCharDamaged)
            .UnRegisterWhenNodeExitTree(this);
    }

    /// <summary>
    ///     绑定英雄
    /// </summary>
    public void BindHero(HeroEntity hero)
    {
        _heroId = hero.Id;
        UpdateAll(hero);
    }

    /// <summary>
    ///     更新所有状态
    /// </summary>
    public void UpdateAll(HeroEntity hero)
    {
        HpLabel.Text = $"HP: {hero.Hp}/{hero.MaxHp}";
        HpBar.Value = (double)hero.Hp / hero.MaxHp * 100;
        DepthLabel.Text = $"层数: {hero.Level}F";
        GoldLabel.Text = $"金币: 0";
    }

    private void OnCharDamaged(CharDamagedEvent e)
    {
        if (e.EntityId != _heroId) return;
        HpLabel.Text = $"HP: {e.RemainingHp}/{GetMaxHp()}";
        HpBar.Value = (double)e.RemainingHp / GetMaxHp() * 100;
    }

    private static int GetMaxHp()
    {
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();
        return hero?.MaxHp ?? 30;
    }
}