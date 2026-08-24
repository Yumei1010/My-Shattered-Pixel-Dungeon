using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.dungeon.painters;
using MyShatteredPixelDungeon.scripts.dungeon.rooms;

namespace MyShatteredPixelDungeon.scripts.dungeon.rooms.standard;

/// <summary>
///     标准房间基类，对应原版 StandardRoom
///     带尺寸分类（NORMAL/LARGE/GIANT）和概率控制
/// </summary>
public abstract class StandardRoom : Room
{
    /// <summary>
    ///     尺寸分类
    /// </summary>
    public enum SizeCategory
    {
        Normal = 0,  // 4-10 格，roomValue=1
        Large = 1,   // 10-14 格，roomValue=2
        Giant = 2    // 14-18 格，roomValue=3
    }

    public SizeCategory SizeCat { get; set; } = SizeCategory.Normal;

    /// <summary>
    ///     尺寸分类概率，默认全 Normal
    /// </summary>
    public virtual float[] SizeCatProbs() => new[] { 1f, 0f, 0f };

    /// <summary>
    ///     随机设置尺寸分类
    /// </summary>
    public bool SetSizeCategory()
    {
        return SetSizeCategory(0, SizeCategoryExtensions.MaxOrdinal);
    }

    /// <summary>
    ///     限制最大 roomValue 的尺寸分类
    /// </summary>
    public bool SetSizeCategory(int maxRoomValue)
    {
        return SetSizeCategory(0, maxRoomValue - 1);
    }

    /// <summary>
    ///     在指定 ordinal 范围内随机设置尺寸分类
    /// </summary>
    public bool SetSizeCategory(int minOrdinal, int maxOrdinal)
    {
        var probs = SizeCatProbs();
        if (probs.Length != 3) return false;

        for (int i = 0; i < minOrdinal; i++) probs[i] = 0;
        for (int i = maxOrdinal + 1; i < 3; i++) probs[i] = 0;

        int ordinal = RandomChances(probs);
        if (ordinal >= 0)
        {
            SizeCat = (SizeCategory)ordinal;
            return true;
        }
        return false;
    }

    public override int MinWidth() => SizeCat.MinDimension();
    public override int MaxWidth() => SizeCat.MaxDimension();
    public override int MinHeight() => SizeCat.MinDimension();
    public override int MaxHeight() => SizeCat.MaxDimension();

    /// <summary>
    ///     尺寸因子（大房间占多个计数）
    /// </summary>
    public virtual int SizeFactor() => SizeCat.RoomValue();

    /// <summary>
    ///     怪物生成权重
    /// </summary>
    public virtual int MobSpawnWeight() => IsEntrance() ? 1 : SizeFactor();

    /// <summary>
    ///     连接权重
    /// </summary>
    public virtual int ConnectionWeight() => SizeFactor() * SizeFactor();

    private static int RandomChances(float[] probs)
    {
        float total = probs.Sum();
        if (total <= 0) return -1;
        float roll = (float)System.Random.Shared.NextDouble() * total;
        float cumulative = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            cumulative += probs[i];
            if (roll < cumulative) return i;
        }
        return -1;
    }
}

/// <summary>
///     SizeCategory 扩展方法
/// </summary>
public static class SizeCategoryExtensions
{
    public const int MaxOrdinal = 2;

    public static int MinDimension(this StandardRoom.SizeCategory cat) => cat switch
    {
        StandardRoom.SizeCategory.Normal => 4,
        StandardRoom.SizeCategory.Large => 10,
        StandardRoom.SizeCategory.Giant => 14,
        _ => 4
    };

    public static int MaxDimension(this StandardRoom.SizeCategory cat) => cat switch
    {
        StandardRoom.SizeCategory.Normal => 10,
        StandardRoom.SizeCategory.Large => 14,
        StandardRoom.SizeCategory.Giant => 18,
        _ => 10
    };

    public static int RoomValue(this StandardRoom.SizeCategory cat) => cat switch
    {
        StandardRoom.SizeCategory.Normal => 1,
        StandardRoom.SizeCategory.Large => 2,
        StandardRoom.SizeCategory.Giant => 3,
        _ => 1
    };
}