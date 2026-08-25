namespace MyShatteredPixelDungeon.scripts.entities;

/// <summary>
///     Actor 优先级常量，对应原版 Actor 的优先级链
/// </summary>
public static class ActorPriority
{
    /// <summary>视觉特效（粒子、动画），最先行动</summary>
    public const int Vfx = 100;
    /// <summary>英雄（玩家），正数在英雄前，负数在英雄后</summary>
    public const int Hero = 0;
    /// <summary>区域效果（毒气、蛛网），英雄后怪物前</summary>
    public const int Blob = -10;
    /// <summary>怪物</summary>
    public const int Mob = -20;
    /// <summary>计时状态（Buff），回合最后</summary>
    public const int Buff = -30;
    /// <summary>默认优先级</summary>
    public const int Default = -100;
}

/// <summary>
///     Actor 基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.actors.Actor
///     持有时间、优先级、ID，由 TurnSystem 调度
/// </summary>
public abstract class Actor
{
    /// <summary>基本时间单位</summary>
    public const float Tick = 1f;

    /// <summary>全局 Actor 计数器</summary>
    private static int _nextId;
    private static readonly object _idLock = new();

    /// <summary>所有活跃 Actor</summary>
    private static readonly HashSet<Actor> AllActors = new();

    /// <summary>按 ID 索引</summary>
    private static readonly Dictionary<int, Actor> ActorsById = new();

    /// <summary>唯一标识</summary>
    public int Id { get; private set; }

    /// <summary>当前时间（从 0 递增）</summary>
    public float Time { get; private set; }

    /// <summary>行动优先级，时间相同时高优先级先行动</summary>
    public virtual int ActPriority => ActorPriority.Default;

    /// <summary>是否激活</summary>
    public bool IsActive { get; private set; } = true;

    protected Actor()
    {
        lock (_idLock) { Id = ++_nextId; }
    }

    /// <summary>
    ///     执行行动（内部调用，由 TurnSystem 调度）
    /// </summary>
    internal bool ExecuteAct() => Act();

    /// <summary>
    ///     执行行动，子类实现具体逻辑
    /// </summary>
    /// <returns>true 继续循环，false 暂停等待输入</returns>
    protected abstract bool Act();

    /// <summary>
    ///     消耗固定时间（不受速度影响）
    /// </summary>
    public void SpendConstant(float time)
    {
        Time += time;
        // 修正浮点误差
        float ex = Math.Abs(Time % 1f);
        if (ex < 0.001f) Time = MathF.Round(Time);
    }

    /// <summary>
    ///     消耗时间（受速度影响，由子类重写）
    /// </summary>
    public virtual void Spend(float time)
    {
        SpendConstant(time);
    }

    /// <summary>
    ///     推迟到指定时间点
    /// </summary>
    public void Postpone(float time)
    {
        if (Time < Now + time)
        {
            Time = Now + time;
            float ex = Math.Abs(Time % 1f);
            if (ex < 0.001f) Time = MathF.Round(Time);
        }
    }

    /// <summary>
    ///     剩余时间
    /// </summary>
    public float Cooldown() => Time - Now;

    /// <summary>
    ///     向上取整到整数值
    /// </summary>
    public void SpendToWhole()
    {
        Time = MathF.Ceiling(Time);
    }

    /// <summary>
    ///     停用（时间设为最大值）
    /// </summary>
    public void Deactivate()
    {
        Time = float.MaxValue;
    }

    /// <summary>
    ///     移除
    /// </summary>
    public void Remove()
    {
        IsActive = false;
        AllActors.Remove(this);
        ActorsById.Remove(Id);
    }

    // ---------- 静态成员 ----------

    /// <summary>当前时间</summary>
    public static float Now { get; internal set; }

    /// <summary>当前正在行动的 Actor</summary>
    public static Actor? Current { get; internal set; }

    /// <summary>
    ///     添加 Actor 到调度队列
    /// </summary>
    public static void Add(Actor actor)
    {
        if (!AllActors.Add(actor)) return;
        ActorsById[actor.Id] = actor;
    }

    /// <summary>
    ///     延迟添加
    /// </summary>
    public static void AddDelayed(Actor actor, float delay)
    {
        actor.SpendConstant(Math.Max(delay, 0));
        Add(actor);
    }

    /// <summary>
    ///     清除所有 Actor
    /// </summary>
    public static void Clear()
    {
        Now = 0;
        AllActors.Clear();
        ActorsById.Clear();
    }

    /// <summary>
    ///     修正时间（所有 Actor 减去最小时间）
    /// </summary>
    public static void FixTime()
    {
        if (AllActors.Count == 0) return;
        float min = AllActors.Min(a => a.Time);
        min = MathF.Floor(min);
        foreach (var a in AllActors) a.Time -= min;
        Now -= min;
    }

    /// <summary>
    ///     按 ID 查找 Actor
    /// </summary>
    public static Actor? FindById(int id) => ActorsById.GetValueOrDefault(id);

    /// <summary>
    ///     获取位置上的角色
    /// </summary>
    public static CharEntity? FindChar(int pos)
    {
        // 由 CharEntity 管理自己的位置注册
        return CharEntity.FindAt(pos);
    }

    /// <summary>
    ///     获取所有 Actor
    /// </summary>
    public static IEnumerable<Actor> All() => AllActors;

    /// <summary>
    ///     获取所有 Char
    /// </summary>
    public static IEnumerable<CharEntity> Chars() => AllActors.OfType<CharEntity>();
}