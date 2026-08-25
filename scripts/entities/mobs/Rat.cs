namespace MyShatteredPixelDungeon.scripts.entities.mobs;

/// <summary>
///     老鼠，对应原版 Rat
///     最简单的怪物，低 HP 低伤害
/// </summary>
public sealed class Rat : MobEntity
{
    public Rat()
    {
        MaxHp = 10;
        Hp = 10;
        DefenseSkillValue = 1;
        Exp = 3;
        BaseSpeed = 1f;
    }

    public override int AttackSkill(CharEntity target) => 4;
    public override int DefenseSkill(CharEntity enemy) => DefenseSkillValue;
    public override int DamageRoll() => Random.Shared.Next(1, 5);
    public override int DrRoll() => 0;
}