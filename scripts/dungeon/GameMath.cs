namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     数学工具，对应原版 com.watabou.utils.GameMath
/// </summary>
public static class GameMath
{
    /// <summary>
    ///     将值限制在 [min, max] 范围内
    /// </summary>
    public static float Gate(float min, float value, float max)
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    ///     将值限制在 [min, max] 范围内（整数版）
    /// </summary>
    public static int Gate(int min, int value, int max)
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    ///     1 或 -1（基于随机符号）
    /// </summary>
    public static float Sign(float value) => value < 0 ? -1 : 1;

    /// <summary>
    ///     线性插值
    /// </summary>
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;
}