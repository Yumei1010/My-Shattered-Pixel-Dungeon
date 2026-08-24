namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     递归阴影投射 FOV 算法，对应原版 com.shatteredpixel.shatteredpixeldungeon.mechanics.ShadowCaster
///     基于 http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting
///     通过 8 个象限扫描实现圆形视野，性能极高
/// </summary>
public static class ShadowCaster
{
    /// <summary>
    ///     最大视野距离
    /// </summary>
    public const int MaxDistance = 20;

    /// <summary>
    ///     每层视野距离对应的行宽（用于将方形 FOV 裁剪为圆形）
    ///     rounding[distance][row] = 该行允许的最大列偏移
    /// </summary>
    public static int[][] Rounding;

    static ShadowCaster()
    {
        Rounding = new int[MaxDistance + 1][];
        for (int i = 1; i <= MaxDistance; i++)
        {
            Rounding[i] = new int[i + 1];
            for (int j = 1; j <= i; j++)
            {
                // 测试单元格中心，使用 i + 0.5
                Rounding[i][j] = Math.Min(j, (int)Math.Round(i * Math.Cos(Math.Asin(j / (i + 0.5)))));
            }
        }
    }

    /// <summary>
    ///     从 (x, y) 投射阴影，将可见单元格写入 fieldOfView
    /// </summary>
    /// <param name="x">源 x 坐标</param>
    /// <param name="y">源 y 坐标</param>
    /// <param name="w">地图宽度</param>
    /// <param name="fieldOfView">输出：视野数组</param>
    /// <param name="blocking">输入：阻挡视线的地形数组</param>
    /// <param name="distance">视野距离</param>
    public static void CastShadow(int x, int y, int w, bool[] fieldOfView, bool[] blocking, int distance)
    {
        if (distance >= MaxDistance)
        {
            distance = MaxDistance;
        }

        BArray.SetFalse(fieldOfView);

        // 源单元格可见
        fieldOfView[y * w + x] = true;

        // 顺时针扫描 8 个象限
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, +1, -1, false);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, -1, +1, true);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, +1, +1, true);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, +1, +1, false);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, -1, +1, false);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, +1, -1, true);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, -1, -1, true);
        ScanOctant(distance, fieldOfView, blocking, 1, x, y, w, 0.0, 1.0, -1, -1, false);
    }

    /// <summary>
    ///     扫描 FOV 的一个 45 度象限。
    ///     通过在 X(mX)、Y(mY) 和 X=Y(mXY) 方向镜像，可组合出完整视野
    /// </summary>
    private static void ScanOctant(int distance, bool[] fov, bool[] blocking, int row,
        int x, int y, int w, double lSlope, double rSlope,
        int mX, int mY, bool mXY)
    {
        bool inBlocking = false;
        int start, end;
        int col;

        int[] roundingAtDist;
        if (distance == 2)
        {
            // 视野距离为 2 时填充视觉角落
            // 否则该距离会不成比例地惩罚对角移动
            roundingAtDist = (int[])Rounding[distance].Clone();
            roundingAtDist[2] = 2;
        }
        else
        {
            roundingAtDist = Rounding[distance];
        }

        // 计算偏移 0.5，因为 FOV 来自源单元格中心

        // 从当前行开始，遍历每一行
        for (; row <= distance; row++)
        {
            // 如果右侧斜率小于左侧斜率，扫描结束
            if (rSlope < lSlope) return;

            // 偏移略小于 0.5，以处理斜率刚好接触单元格的情况
            if (lSlope == 0) start = 0;
            else start = (int)Math.Floor((row - 0.5) * lSlope + 0.499);

            if (rSlope == 1) end = roundingAtDist[row];
            else end = Math.Min(roundingAtDist[row],
                (int)Math.Ceiling((row + 0.5) * rSlope - 0.499));

            // 源坐标
            int cell = x + y * w;

            // 加上当前单元格坐标（包括 x/y/x=y 镜像）
            if (mXY) cell += mX * start * w + mY * row;
            else cell += mX * start + mY * row * w;

            // 遍历该行每一列
            for (col = start; col <= end; col++)
            {
                // 处理斜率在单元格末端比开头远 1 的错误情况，
                // 且较早的单元格阻挡视线
                if (col == end && inBlocking && (int)Math.Ceiling((row - 0.5) * rSlope - 0.499) != end)
                {
                    break;
                }

                fov[cell] = true;

                if (blocking[cell])
                {
                    if (!inBlocking)
                    {
                        inBlocking = true;

                        // 从当前单元格左侧开始更深一行的新扫描
                        if (col != start)
                        {
                            ScanOctant(distance, fov, blocking, row + 1, x, y, w, lSlope,
                                (col - 0.5) / (row + 0.5),
                                mX, mY, mXY);
                        }
                    }
                }
                else
                {
                    if (inBlocking)
                    {
                        inBlocking = false;

                        // 将当前扫描限制在单元格左侧，供未来行使用
                        lSlope = (col - 0.5) / (row - 0.5);
                    }
                }

                if (!mXY) cell += mX;
                else cell += mX * w;
            }

            // 如果该行以阻挡单元格结束，扫描结束
            if (inBlocking) return;
        }
    }
}