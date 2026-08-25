using MyShatteredPixelDungeon.scripts.entities.buffs;

namespace MyShatteredPixelDungeon.scripts.entities;

/// <summary>
///     怪物 AI 状态类型，对应原版 Mob 的 6 种状态
/// </summary>
public enum AiStateType
{
    Sleeping,
    Wandering,
    Hunting,
    Investigating,
    Fleeing,
    Passive
}

/// <summary>
///     怪物实体，对应原版 com.shatteredpixel.shatteredpixeldungeon.actors.mobs.Mob
///     扩展 CharEntity，增加 AI 状态机和敌人选择逻辑
/// </summary>
public abstract class MobEntity : CharEntity
{
    public override int ActPriority => ActorPriority.Mob;

    /// <summary>当前 AI 状态</summary>
    public AiStateType AiState { get; set; } = AiStateType.Sleeping;

    /// <summary>当前目标位置</summary>
    public int TargetPos { get; set; } = -1;

    /// <summary>当前敌人</summary>
    public CharEntity? Enemy { get; set; }

    /// <summary>是否发现敌人</summary>
    public bool EnemySeen { get; set; }

    /// <summary>经验值</summary>
    public int Exp { get; set; } = 1;

    /// <summary>最大等级（超过此等级不获得经验）</summary>
    public int MaxLevel { get; set; } = 30;

    /// <summary>防御技能</summary>
    public int DefenseSkillValue { get; set; }

    protected MobEntity()
    {
        Alignment = Alignment.Enemy;
    }

    protected override bool Act()
    {
        // 麻痹时跳过
        if (FindBuff<ParalysisBuff>() != null)
        {
            EnemySeen = false;
            Spend(Tick);
            return true;
        }

        // 恐惧/威慑时逃跑
        if (FindBuff<TerrorBuff>() != null || FindBuff<DreadBuff>() != null)
        {
            AiState = AiStateType.Fleeing;
        }

        // 选择敌人
        Enemy = ChooseEnemy();

        // 执行当前状态
        return ExecuteState();
    }

    /// <summary>
    ///     执行当前 AI 状态
    /// </summary>
    private bool ExecuteState()
    {
        bool enemyInFov = Enemy != null && Enemy.IsAlive;

        switch (AiState)
        {
            case AiStateType.Sleeping:
                return ActSleeping(enemyInFov);

            case AiStateType.Wandering:
                return ActWandering(enemyInFov);

            case AiStateType.Hunting:
                return ActHunting(enemyInFov);

            case AiStateType.Investigating:
                return ActInvestigating(enemyInFov);

            case AiStateType.Fleeing:
                return ActFleeing(enemyInFov);

            case AiStateType.Passive:
                Spend(Tick);
                return true;

            default:
                Spend(Tick);
                return true;
        }
    }

    /// <summary>
    ///     睡眠状态：检测敌人，概率唤醒
    /// </summary>
    private bool ActSleeping(bool enemyInFov)
    {
        if (enemyInFov && Enemy != null)
        {
            // 检测概率 = 1 / (距离 + 潜行)
            float chance = 1f / (Distance(Enemy) + Enemy.Stealth());
            if (Random.Shared.NextSingle() < chance)
            {
                WakeUp();
                return true;
            }
        }

        EnemySeen = false;
        Spend(Tick);
        return true;
    }

    /// <summary>
    ///     巡逻状态：检测敌人或随机移动
    /// </summary>
    private bool ActWandering(bool enemyInFov)
    {
        if (enemyInFov && Enemy != null)
        {
            float chance = 1f / (Distance(Enemy) / 2f + Enemy.Stealth());
            if (Random.Shared.NextSingle() < chance)
            {
                AiState = AiStateType.Hunting;
                EnemySeen = true;
                TargetPos = Enemy.Pos;
                return true;
            }
        }

        return ActWander();
    }

    /// <summary>
    ///     追击状态：接近并攻击敌人
    /// </summary>
    private bool ActHunting(bool enemyInFov)
    {
        if (enemyInFov && Enemy != null && CanAttack(Enemy))
        {
            return DoAttack(Enemy);
        }

        if (enemyInFov)
        {
            TargetPos = Enemy!.Pos;
        }
        else if (Enemy == null)
        {
            AiState = AiStateType.Wandering;
            Spend(Tick);
            return true;
        }

        return MoveToward(TargetPos);
    }

    /// <summary>
    ///     调查状态：追踪最后已知位置
    /// </summary>
    private bool ActInvestigating(bool enemyInFov)
    {
        if (enemyInFov)
        {
            TargetPos = Enemy!.Pos;
        }
        else if (DistanceTo(TargetPos) <= 1)
        {
            AiState = AiStateType.Wandering;
            Spend(Tick);
            return true;
        }

        return ActWandering(enemyInFov);
    }

    /// <summary>
    ///     逃跑状态：远离敌人
    /// </summary>
    private bool ActFleeing(bool enemyInFov)
    {
        if (Enemy == null || (!enemyInFov && Random.Shared.Next(6) >= 5))
        {
            AiState = AiStateType.Wandering;
            Spend(Tick);
            return true;
        }

        if (enemyInFov)
        {
            TargetPos = Enemy!.Pos;
        }

        return MoveAway(TargetPos);
    }

    // ---------- 行为方法 ----------

    /// <summary>
    ///     唤醒
    /// </summary>
    protected void WakeUp()
    {
        EnemySeen = true;
        AiState = AiStateType.Hunting;
        if (Enemy != null) TargetPos = Enemy.Pos;
        Spend(Tick);
    }

    /// <summary>
    ///     随机巡逻
    /// </summary>
    protected virtual bool ActWander()
    {
        EnemySeen = false;
        if (TargetPos != -1 && MoveToward(TargetPos))
        {
            Spend(1f / Speed());
            return true;
        }
        TargetPos = RandomShallow();
        Spend(Tick);
        return true;
    }

    /// <summary>
    ///     向目标移动一步
    /// </summary>
    protected virtual bool MoveToward(int target)
    {
        // 简化：相邻则直接移动，否则使用 PathFinder
        if (target == -1) return false;
        Spend(1f / Speed());
        return true;
    }

    /// <summary>
    ///     远离目标移动一步
    /// </summary>
    protected virtual bool MoveAway(int target)
    {
        Spend(1f / Speed());
        return true;
    }

    /// <summary>
    ///     攻击敌人
    /// </summary>
    protected virtual bool DoAttack(CharEntity enemy)
    {
        Attack(enemy);
        Spend(AttackDelay());
        return true;
    }

    /// <summary>
    ///     攻击延迟
    /// </summary>
    protected virtual float AttackDelay() => 1f;

    /// <summary>
    ///     是否能攻击到目标
    /// </summary>
    protected virtual bool CanAttack(CharEntity target) => DistanceTo(target.Pos) <= 1;

    // ---------- 选择敌人 ----------

    /// <summary>
    ///     选择目标敌人
    /// </summary>
    protected virtual CharEntity? ChooseEnemy()
    {
        if (Enemy != null && Enemy.IsAlive && AiState != AiStateType.Wandering)
            return Enemy;

        // 寻找最近的敌对角色
        CharEntity? closest = null;
        int closestDist = int.MaxValue;

        foreach (var ch in Chars())
        {
            if (ch == this || !ch.IsAlive || ch.Alignment == Alignment) continue;
            if (ch.Alignment == Alignment.Enemy) continue; // 不攻击同类

            int dist = DistanceTo(ch.Pos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = ch;
            }
        }

        return closest;
    }

    // ---------- 辅助方法 ----------

    /// <summary>
    ///     到目标的距离
    /// </summary>
    protected int DistanceTo(int pos) => Math.Abs(Pos - pos);

    /// <summary>
    ///     与另一角色的距离
    /// </summary>
    protected int Distance(CharEntity other) => DistanceTo(other.Pos);

    /// <summary>
    ///     潜行值
    /// </summary>
    public virtual float Stealth() => 0f;

    /// <summary>
    ///     随机浅水位置（简化版，后续接入 DungeonData）
    /// </summary>
    private static int RandomShallow() => Random.Shared.Next(100);
}
