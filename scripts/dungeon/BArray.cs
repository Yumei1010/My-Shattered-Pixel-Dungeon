namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     boolean 数组工具类，对应原版 com.watabou.utils.BArray
///     提供高性能的数组批量操作（清零/与/或/非/比较）
/// </summary>
public static class BArray
{
    private static bool[] _falseArray;

    /// <summary>
    ///     批量置 false，比 new bool[] 或 Array.Fill 更快
    ///     通过复用静态零数组 + Array.Copy 实现
    /// </summary>
    public static void SetFalse(bool[] toBeFalse)
    {
        if (_falseArray == null || _falseArray.Length < toBeFalse.Length)
        {
            _falseArray = new bool[toBeFalse.Length];
        }

        Array.Copy(_falseArray, 0, toBeFalse, 0, toBeFalse.Length);
    }

    /// <summary>
    ///     逐元素与操作（a &amp;&amp; b）
    /// </summary>
    public static bool[] And(bool[] a, bool[] b, bool[]? result)
    {
        int length = a.Length;
        result ??= new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = a[i] && b[i];
        }

        return result;
    }

    /// <summary>
    ///     逐元素或操作（a || b）
    /// </summary>
    public static bool[] Or(bool[] a, bool[] b, bool[]? result)
    {
        return Or(a, b, 0, a.Length, result);
    }

    /// <summary>
    ///     指定偏移和长度的逐元素或操作（a || b）
    /// </summary>
    public static bool[] Or(bool[] a, bool[] b, int offset, int length, bool[]? result)
    {
        result ??= new bool[length];

        for (int i = offset; i < offset + length; i++)
        {
            result[i] = a[i] || b[i];
        }

        return result;
    }

    /// <summary>
    ///     逐元素非操作（!a）
    /// </summary>
    public static bool[] Not(bool[] a, bool[]? result)
    {
        int length = a.Length;
        result ??= new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = !a[i];
        }

        return result;
    }

    /// <summary>
    ///     判断数组元素是否等于指定值，结果写入 result
    /// </summary>
    public static bool[] Is(int[] a, bool[]? result, int v1)
    {
        int length = a.Length;
        result ??= new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = a[i] == v1;
        }

        return result;
    }

    /// <summary>
    ///     判断数组元素是否等于给定值集合中的任意一个
    /// </summary>
    public static bool[] IsOneOf(int[] a, bool[]? result, params int[] v)
    {
        int length = a.Length;
        int nv = v.Length;
        result ??= new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = false;
            for (int j = 0; j < nv; j++)
            {
                if (a[i] == v[j])
                {
                    result[i] = true;
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     判断数组元素是否不等于指定值
    /// </summary>
    public static bool[] IsNot(int[] a, bool[]? result, int v1)
    {
        int length = a.Length;
        result ??= new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = a[i] != v1;
        }

        return result;
    }

    /// <summary>
    ///     判断数组元素是否不等于给定值集合中的任意一个
    /// </summary>
    public static bool[] IsNotOneOf(int[] a, bool[]? result, params int[] v)
    {
        int length = a.Length;
        int nv = v.Length;
        result ??= new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = true;
            for (int j = 0; j < nv; j++)
            {
                if (a[i] == v[j])
                {
                    result[i] = false;
                    break;
                }
            }
        }

        return result;
    }
}