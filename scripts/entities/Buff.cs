namespace MyShatteredPixelDungeon.scripts.entities;

/// <summary>
///     Buff 类型（正面/负面/中性）
/// </summary>
public enum BuffType
{
    Positive,
    Negative,
    Neutral
}

/// <summary>
///     Buff 基类，对应原版 com.shatteredpixel.shatteredpixeldungeon.actors.buffs.Buff
///     作为计时状态附加到 CharEntity 上，每回合执行
/// </summary>
public abstract class Buff
{
    /// <summary>所属角色</summary>
    public CharEntity? Target { get; private set; }

    /// <summary>Buff 类型</summary>
    public BuffType Type { get; init; } = BuffType.Neutral;

    /// <summary>是否已显示名称</summary>
    public bool Announced { get; set; }

    /// <summary>持续时间（秒），<= 0 表示无限</summary>
    public float Duration { get; set; }

    /// <summary>剩余时间</summary>
    public float Remaining { get; set; }

    /// <summary>
    ///     附加到角色
    /// </summary>
    public bool AttachTo(CharEntity target)
    {
        if (target.IsImmune(GetType()))
            return false;

        Target = target;
        return target.AddBuff(this);
    }

    /// <summary>
    ///     从角色移除
    /// </summary>
    public void Detach()
    {
        if (Target != null)
        {
            Target.RemoveBuff(this);
            Target = null;
        }
    }

    /// <summary>
    ///     每回合执行（由 TurnSystem 驱动）
    /// </summary>
    public virtual void OnAct() { }

    /// <summary>
    ///     附加时调用
    /// </summary>
    public virtual void OnAttach() { }

    /// <summary>
    ///     移除时调用
    /// </summary>
    public virtual void OnDetach() { }

    /// <summary>
    ///     名称
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    ///     描述
    /// </summary>
    public virtual string Description => "";

    /// <summary>
    ///     图标索引
    /// </summary>
    public virtual int Icon => -1;

    /// <summary>
    ///     静态方法：附加（如已有则复用）
    /// </summary>
    public static T? Affect<T>(CharEntity target, float duration = 0) where T : Buff, new()
    {
        var existing = target.FindBuff<T>();
        if (existing != null)
        {
            if (duration > 0) existing.Remaining = duration;
            return existing;
        }

        var buff = new T { Duration = duration, Remaining = duration };
        return buff.AttachTo(target) ? buff : null;
    }

    /// <summary>
    ///     静态方法：附加（总是新建实例）
    /// </summary>
    public static T? Append<T>(CharEntity target, float duration = 0) where T : Buff, new()
    {
        var buff = new T { Duration = duration, Remaining = duration };
        return buff.AttachTo(target) ? buff : null;
    }

    /// <summary>
    ///     静态方法：移除指定类型的 Buff
    /// </summary>
    public static void Detach<T>(CharEntity target) where T : Buff
    {
        target.RemoveBuff<T>();
    }

    /// <summary>
    ///     静态方法：延长持续时间
    /// </summary>
    public static T? Prolong<T>(CharEntity target, float duration) where T : Buff, new()
    {
        var buff = Affect<T>(target, duration);
        if (buff != null) buff.Remaining = duration;
        return buff;
    }
}