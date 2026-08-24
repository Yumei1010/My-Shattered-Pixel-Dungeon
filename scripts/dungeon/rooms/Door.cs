namespace MyShatteredPixelDungeon.scripts.dungeon.rooms;

/// <summary>
///     门类型，对应原版 Room.Door.Type
///     控制门的可见性、通行性和锁定状态
/// </summary>
public enum DoorType
{
    /// <summary>空（无门，纯通道）</summary>
    Empty,
    /// <summary>隧道（连接房间的走廊）</summary>
    Tunnel,
    /// <summary>水道</summary>
    Water,
    /// <summary>普通木门</summary>
    Regular,
    /// <summary>已解锁的门</summary>
    Unlocked,
    /// <summary>隐藏门（密门）</summary>
    Hidden,
    /// <summary>路障</summary>
    Barricade,
    /// <summary>锁着的门</summary>
    Locked,
    /// <summary>水晶门</summary>
    Crystal,
    /// <summary>墙（无门）</summary>
    Wall
}

/// <summary>
///     房间门，包含位置和类型
///     对应原版 Room.Door
/// </summary>
public sealed class Door
{
    public int X { get; set; }
    public int Y { get; set; }
    public DoorType Type { get; set; } = DoorType.Empty;
    public bool TypeLocked { get; set; }

    public Door() { }

    public Door(int x, int y, DoorType type = DoorType.Empty)
    {
        X = x;
        Y = y;
        Type = type;
    }

    /// <summary>
    ///     设置门类型（仅能升级，不能降级，除非 typeLocked）
    /// </summary>
    public void Set(DoorType type)
    {
        if (!TypeLocked && type > Type)
        {
            Type = type;
        }
    }

    /// <summary>
    ///     锁定门类型变更
    /// </summary>
    public void LockTypeChanges(bool locked)
    {
        TypeLocked = locked;
    }
}