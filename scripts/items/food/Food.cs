using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.entities.buffs;

namespace MyShatteredPixelDungeon.scripts.items.food;

/// <summary>
///     食物基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.food.Food
/// </summary>
public abstract class Food : Item
{
    /// <summary>饱食度恢复量</summary>
    public virtual int HungerValue => 0;

    /// <summary>HP 恢复量</summary>
    public virtual int HpRestore => 0;

    /// <summary>食用时间</summary>
    public virtual float EatTime => 1f;

    public override string DefaultAction => ItemAction.Eat;

    /// <summary>
    ///     食用食物
    /// </summary>
    public virtual void Eat(HeroEntity hero)
    {
        // 恢复饥饿值
        var hunger = hero.FindBuff<Hunger>();
        if (hunger != null)
        {
            hunger.Eat(HungerValue);
        }

        // 恢复 HP
        if (HpRestore > 0)
        {
            hero.Hp = Math.Min(hero.Hp + HpRestore, hero.MaxHp);
        }

        Identify();
    }
}

/// <summary>
///     神秘肉（恢复 100 饱食度）
/// </summary>
public sealed class MysteryMeat : Food
{
    public override string Name => "神秘肉";
    public override int HungerValue => 100;
}

/// <summary>
///     烤肉（恢复 250 饱食度）
/// </summary>
public sealed class ChargrilledMeat : Food
{
    public override string Name => "烤肉";
    public override int HungerValue => 250;
}

/// <summary>
///     小圆面包（恢复 100 饱食度）
/// </summary>
public sealed class Pasty : Food
{
    public override string Name => "小圆面包";
    public override int HungerValue => 100;
}

/// <summary>
///     腐肉（恢复 50 饱食度，可能中毒）
/// </summary>
public sealed class RottenMeat : Food
{
    public override string Name => "腐肉";
    public override int HungerValue => 50;
}

/// <summary>
///     盲眼草（恢复 50 饱食度）
/// </summary>
public sealed class Blindweed : Food
{
    public override string Name => "盲眼草";
    public override int HungerValue => 50;
}

/// <summary>
///     全营养面包（恢复 450 饱食度）
/// </summary>
public sealed class NutritiousBread : Food
{
    public override string Name => "全营养面包";
    public override int HungerValue => 450;
}

/// <summary>
///     冻肉（恢复 75 饱食度）
/// </summary>
public sealed class FrozenCarpaccio : Food
{
    public override string Name => "冻肉";
    public override int HungerValue => 75;
}