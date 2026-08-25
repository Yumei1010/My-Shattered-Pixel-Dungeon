using GFramework.Core.system;
using GFramework.Core.extensions;
using MyShatteredPixelDungeon.scripts.cqrs.combat.@event;
using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.weapons;
using MyShatteredPixelDungeon.scripts.items.armors;

namespace MyShatteredPixelDungeon.scripts.systems;

/// <summary>
///     战斗系统，处理攻击/伤害/死亡流程
///     通过 CQRS 事件驱动，与 TurnSystem 配合
/// </summary>
public sealed class CombatSystem : AbstractSystem
{
    protected override void OnInit()
    {
        this.RegisterEvent<ActorActRequestedEvent>(OnActorActRequested);
        this.RegisterEvent<CharDamagedEvent>(OnCharDamaged);
        this.RegisterEvent<CharDiedEvent>(OnCharDied);
    }

    /// <summary>
    ///     Actor 行动请求处理
    /// </summary>
    private void OnActorActRequested(ActorActRequestedEvent e)
    {
        var actor = Actor.FindById(e.ActorId);
        if (actor == null) return;

        // 如果是 MobEntity，执行 AI 行动
        if (actor is MobEntity mob && mob.IsAlive)
        {
            ExecuteMobAct(mob);
        }
    }

    /// <summary>
    ///     执行怪物行动
    /// </summary>
    private void ExecuteMobAct(MobEntity mob)
    {
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();
        if (hero == null || !hero.IsAlive) return;

        int dist = mob.DistanceTo(hero.Pos);

        // 在攻击范围内 → 攻击
        if (dist <= 1)
        {
            PerformAttack(mob, hero);
        }
        // 在视野内 → 追击
        else if (dist <= mob.ViewDistance)
        {
            // TODO: 寻路追击
        }
    }

    /// <summary>
    ///     执行攻击
    /// </summary>
    public static CombatResult PerformAttack(CharEntity attacker, CharEntity defender)
    {
        var result = new CombatResult();

        // 命中判定
        float acuStat = attacker.AttackSkill(defender);
        float defStat = defender.DefenseSkill(attacker);
        bool hit = ResolveHit(acuStat, defStat);

        if (!hit)
        {
            result.Missed = true;
            return result;
        }

        // 伤害计算
        int dmg = attacker.DamageRoll();
        dmg = Math.Max(dmg - defender.DrRoll(), 0);
        result.Damage = dmg;
        result.Hit = true;

        // 应用伤害
        defender.Damage(dmg, attacker);

        // 消耗时间
        attacker.Spend(1f);
        defender.Spend(1f);

        return result;
    }

    /// <summary>
    ///     命中判定
    /// </summary>
    private static bool ResolveHit(float attackSkill, float defenseSkill)
    {
        if (attackSkill >= 1_000_000) return true;
        if (defenseSkill >= 1_000_000) return false;

        float acuRoll = Random.Shared.NextSingle() * attackSkill;
        float defRoll = Random.Shared.NextSingle() * defenseSkill;
        return acuRoll >= defRoll;
    }

    /// <summary>
    ///     计算英雄攻击技能（含武器修正）
    /// </summary>
    public static int CalculateAttackSkill(HeroEntity hero)
    {
        int skill = 10 + hero.Level * 2;
        if (hero.EquippedWeapon != null)
        {
            skill = (int)(skill * hero.EquippedWeapon.AccuracyFactor(hero.Strength));
        }
        return skill;
    }

    /// <summary>
    ///     计算英雄防御技能（含护甲修正）
    /// </summary>
    public static int CalculateDefenseSkill(HeroEntity hero)
    {
        int skill = 5 + hero.Level;
        // 闪避 Buff 修正
        return skill;
    }

    /// <summary>
    ///     伤害事件处理
    /// </summary>
    private void OnCharDamaged(CharDamagedEvent e)
    {
        // 通知 UI 更新血量显示
    }

    /// <summary>
    ///     死亡事件处理
    /// </summary>
    private void OnCharDied(CharDiedEvent e)
    {
        var actor = Actor.FindById(e.EntityId);
        if (actor is MobEntity mob)
        {
            // 怪物死亡：掉落物品
            DropLoot(mob);
        }
        else if (actor is HeroEntity)
        {
            // 英雄死亡：游戏结束
            // TODO: 触发游戏结束状态
        }
    }

    /// <summary>
    ///     怪物掉落物品
    /// </summary>
    private static void DropLoot(MobEntity mob)
    {
        // 50% 概率掉落金币
        if (Random.Shared.NextSingle() < 0.5f)
        {
            int goldAmount = 1 + Random.Shared.Next(5);
            GroundItemManager.Drop(mob.Pos, new Gold { Quantity = goldAmount });
        }
    }
}

/// <summary>
///     战斗结果
/// </summary>
public sealed class CombatResult
{
    /// <summary>是否命中</summary>
    public bool Hit { get; set; }

    /// <summary>是否未命中</summary>
    public bool Missed { get; set; }

    /// <summary>伤害值</summary>
    public int Damage { get; set; }

    /// <summary>是否暴击</summary>
    public bool Critical { get; set; }

    /// <summary>是否击杀</summary>
    public bool Killed => Damage > 0;
}