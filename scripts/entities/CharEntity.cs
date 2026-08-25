using MyShatteredPixelDungeon.scripts.dungeon;

namespace MyShatteredPixelDungeon.scripts.entities;

/// <summary>
///     角色阵营
/// </summary>
public enum Alignment
{
    Enemy,
    Neutral,
    Ally
}

/// <summary>
///     角色实体，对应原版 com.shatteredpixel.shatteredpixeldungeon.actors.Char
///     持有位置、HP、属性、Buff 容器，由 TurnSystem 调度
/// </summary>
public abstract class CharEntity : Actor
{
    private static readonly Dictionary<int, CharEntity> _charsByPos = new();

    /// <summary>地图位置</summary>
    public int Pos { get; set; }

    /// <summary>最大生命值</summary>
    public int MaxHp { get; set; }

    /// <summary>当前生命值</summary>
    public int Hp { get; set; }

    /// <summary>基础速度</summary>
    protected float BaseSpeed { get; set; } = 1f;

    /// <summary>视野距离</summary>
    public int ViewDistance { get; set; } = 8;

    /// <summary>阵营</summary>
    public Alignment Alignment { get; set; } = Alignment.Enemy;

    /// <summary>飞行</summary>
    public bool Flying { get; set; }

    /// <summary>定身</summary>
    public bool Rooted { get; set; }

    /// <summary>力量</summary>
    public int Strength { get; set; } = 10;

    /// <summary>等级</summary>
    public int Level { get; set; } = 1;

    /// <summary>经验值</summary>
    public int Exp { get; set; }

    /// <summary>Buff 列表</summary>
    public List<Buff> Buffs { get; } = new();

    /// <summary>抵抗效果集合</summary>
    protected HashSet<Type> Resistances { get; } = new();

    /// <summary>免疫效果集合</summary>
    protected HashSet<Type> Immunities { get; } = new();

    protected CharEntity()
    {
        _charsByPos[Pos] = this;
    }

    /// <summary>
    ///     速度（受减速/加速影响）
    /// </summary>
    public virtual float Speed()
    {
        float speed = BaseSpeed;
        // Buff 修正（子类可扩展）
        return speed;
    }

    /// <summary>
    ///     消耗时间（受速度影响）
    /// </summary>
    public override void Spend(float time)
    {
        float timeScale = 1f;
        // 减速/加速修正（由 Buff 提供）
        SpendConstant(time / timeScale);
    }

    /// <summary>
    ///     移动
    /// </summary>
    public virtual void Move(int step)
    {
        _charsByPos.Remove(Pos);
        Pos = step;
        _charsByPos[Pos] = this;
    }

    /// <summary>
    ///     受伤
    /// </summary>
    /// <param name="dmg">伤害值</param>
    /// <param name="source">伤害来源</param>
    public virtual void Damage(int dmg, object source)
    {
        if (!IsActive || dmg < 0) return;
        Hp -= dmg;
        if (Hp < 0) Hp = 0;
        if (!IsAlive) Die(source);
    }

    /// <summary>
    ///     死亡
    /// </summary>
    public virtual void Die(object source)
    {
        Remove();
        _charsByPos.Remove(Pos);
    }

    /// <summary>
    ///     是否存活
    /// </summary>
    public bool IsAlive => IsActive && Hp > 0;

    /// <summary>
    ///     攻击技能
    /// </summary>
    public virtual int AttackSkill(CharEntity target) => 0;

    /// <summary>
    ///     防御技能
    /// </summary>
    public virtual int DefenseSkill(CharEntity enemy) => 0;

    /// <summary>
    ///     伤害骰子
    /// </summary>
    public virtual int DamageRoll() => 1;

    /// <summary>
    ///     护甲减伤
    /// </summary>
    public virtual int DrRoll() => 0;

    /// <summary>
    ///     命中判定
    /// </summary>
    public static bool Hit(CharEntity attacker, CharEntity defender, float accMulti = 1f)
    {
        float acuStat = attacker.AttackSkill(defender);
        float defStat = defender.DefenseSkill(attacker);

        if (acuStat >= 1_000_000) return true;
        if (defStat >= 1_000_000) return false;

        float acuRoll = Random.Shared.NextSingle() * acuStat * accMulti;
        float defRoll = Random.Shared.NextSingle() * defStat;
        return acuRoll >= defRoll;
    }

    /// <summary>
    ///     攻击
    /// </summary>
    public bool Attack(CharEntity enemy, float dmgMulti = 1f, float accMulti = 1f)
    {
        if (enemy == null || !enemy.IsAlive) return false;

        if (Hit(this, enemy, accMulti))
        {
            int dmg = (int)(DamageRoll() * dmgMulti);
            dmg = Math.Max(dmg - enemy.DrRoll(), 0);
            enemy.Damage(dmg, this);
            return true;
        }
        return false;
    }

    /// <summary>
    ///     潜行值
    /// </summary>
    public virtual float Stealth() => 0f;

    /// <summary>
    ///     添加 Buff
    /// </summary>
    public bool AddBuff(Buff buff)
    {
        if (IsImmune(buff.GetType())) return false;
        Buffs.Add(buff);
        buff.OnAttach();
        return true;
    }

    /// <summary>
    ///     移除指定 Buff 实例
    /// </summary>
    public void RemoveBuff(Buff buff)
    {
        if (Buffs.Remove(buff))
        {
            buff.OnDetach();
        }
    }

    /// <summary>
    ///     移除指定类型的 Buff
    /// </summary>
    public void RemoveBuff<T>() where T : Buff
    {
        for (int i = Buffs.Count - 1; i >= 0; i--)
        {
            if (Buffs[i] is T)
            {
                Buffs[i].OnDetach();
                Buffs.RemoveAt(i);
            }
        }
    }

    /// <summary>
    ///     查找指定类型的 Buff
    /// </summary>
    public T? FindBuff<T>() where T : Buff
    {
        for (int i = 0; i < Buffs.Count; i++)
        {
            if (Buffs[i] is T t) return t;
        }
        return null;
    }

    /// <summary>
    ///     查找所有指定类型的 Buff
    /// </summary>
    public List<T> FindBuffs<T>() where T : Buff
    {
        var result = new List<T>();
        for (int i = 0; i < Buffs.Count; i++)
        {
            if (Buffs[i] is T t) result.Add(t);
        }
        return result;
    }

    /// <summary>
    ///     是否免疫指定效果
    /// </summary>
    public bool IsImmune(Type effect)
    {
        foreach (var t in Immunities)
        {
            if (t.IsAssignableFrom(effect)) return true;
        }
        return false;
    }

    /// <summary>
    ///     抵抗系数（0.5 = 减半，1.0 = 正常）
    /// </summary>
    public float Resist(Type effect)
    {
        float result = 1f;
        foreach (var t in Resistances)
        {
            if (t.IsAssignableFrom(effect)) result *= 0.5f;
        }
        return result;
    }

    /// <summary>
    ///     按位置查找角色
    /// </summary>
    public static CharEntity? FindAt(int pos) =>
        _charsByPos.GetValueOrDefault(pos);

    /// <summary>
    ///     清除所有位置注册
    /// </summary>
    public static void ClearPositions() => _charsByPos.Clear();
}