namespace MyShatteredPixelDungeon.scripts.dungeon;

/// <summary>
///     确定性随机数生成器，基于 System.Random
///     通过 Activate()/WithSeed() 设置当前活跃 RNG
/// </summary>
public sealed class DeterministicRng
{
    private readonly System.Random _rng;

    /// <summary>当前活跃的 RNG</summary>
    public static DeterministicRng? Current { get; private set; }

    public DeterministicRng(long seed) => _rng = new System.Random((int)seed);
    public DeterministicRng(int seed) => _rng = new System.Random(seed);

    public int NextInt(int max) => _rng.Next(max);
    public int NextInt(int min, int max) => _rng.Next(min, max);
    public float NextFloat() => (float)_rng.NextDouble();
    public long NextLong() => _rng.NextInt64();

    public RngScope Activate() => new(this);
    public static RngScope WithSeed(long seed) => new(new DeterministicRng(seed));

    // 静态便捷方法（Current 为 null 时回退到 System.Random.Shared）
    public static int Range(int max) => Current?._rng.Next(max) ?? System.Random.Shared.Next(max);
    public static int Range(int min, int max) => Current?._rng.Next(min, max) ?? System.Random.Shared.Next(min, max);
    public static float Float() => (float)(Current?._rng.NextDouble() ?? System.Random.Shared.NextDouble());

    public readonly struct RngScope : IDisposable
    {
        private readonly DeterministicRng? _previous;
        public RngScope(DeterministicRng rng) { _previous = Current; Current = rng; }
        public void Dispose() { Current = _previous; }
    }
}