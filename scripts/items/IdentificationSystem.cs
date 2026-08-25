using MyShatteredPixelDungeon.scripts.items.potions;
using MyShatteredPixelDungeon.scripts.items.scrolls;
using MyShatteredPixelDungeon.scripts.items.rings;

namespace MyShatteredPixelDungeon.scripts.items;

/// <summary>
///     药水/卷轴随机识别系统，对应原版 ItemStatusHandler
///     每次新游戏随机分配药水颜色和卷轴名称，存档中跨层不变
/// </summary>
public static class IdentificationSystem
{
    // 药水颜色映射（每种药水对应一个颜色索引 0-7）
    private static readonly string[] _potionColors =
    {
        "ruby",      // 红宝石色
        "amber",     // 琥珀色
        "emerald",   // 翡翠色
        "sapphire",  // 蓝宝石色
        "amethyst",  // 紫水晶色
        "garnet",    // 石榴红色
        "citrine",   // 黄水晶色
        "pearl",     // 珍珠白色
    };

    // 卷轴名称映射（每种卷轴对应一个名称索引）
    private static readonly string[] _scrollLabels =
    {
        "SIOH", "OYZY", "QYNA", "XIZO", "HALO",
        "SOMA", "KAHA", "TIKI", "WATU", "ZALO",
        "BENU", "MARI", "NUPO", "LIRA", "POKO",
        "RILY", "TUBO", "VATA", "WELO", "YARA",
        "ZIMA", "FIDO", "GORA", "HURA", "JUKA",
        "KOLO", "LONU", "MIRA", "NOKU", "PALU",
    };

    // 戒指宝石映射
    private static readonly string[] _ringGems =
    {
        "garnet", "ruby", "topaz", "emerald", "sapphire",
        "amethyst", "opal", "tourmaline", "jade", "turquoise",
        "alexandrite", "moonstone", "quartz", "agate", "beryl",
    };

    private static readonly Dictionary<string, int> _potionColorMap = new();
    private static readonly Dictionary<string, int> _scrollLabelMap = new();
    private static readonly Dictionary<string, int> _ringGemMap = new();

    private static bool _initialized;

    /// <summary>
    ///     初始化（新游戏时调用，随机分配映射）
    /// </summary>
    public static void Initialize()
    {
        _potionColorMap.Clear();
        _scrollLabelMap.Clear();
        _ringGemMap.Clear();
        _initialized = true;

        // 按药水类型顺序分配颜色
        var potionTypes = GetPotionTypes();
        ShuffleArray(_potionColors);
        for (int i = 0; i < potionTypes.Count && i < _potionColors.Length; i++)
        {
            _potionColorMap[potionTypes[i]] = i;
        }

        // 按卷轴类型顺序分配名称
        var scrollTypes = GetScrollTypes();
        ShuffleArray(_scrollLabels);
        for (int i = 0; i < scrollTypes.Count && i < _scrollLabels.Length; i++)
        {
            _scrollLabelMap[scrollTypes[i]] = i;
        }

        // 按戒指类型顺序分配宝石
        var ringTypes = GetRingTypes();
        ShuffleArray(_ringGems);
        for (int i = 0; i < ringTypes.Count && i < _ringGems.Length; i++)
        {
            _ringGemMap[ringTypes[i]] = i;
        }
    }

    /// <summary>
    ///     获取药水的颜色索引
    /// </summary>
    public static int GetPotionColor(Potion potion)
    {
        string key = potion.GetType().Name;
        if (_potionColorMap.TryGetValue(key, out int color))
            return color;
        return 0;
    }

    /// <summary>
    ///     获取药水的颜色名称
    /// </summary>
    public static string GetPotionColorName(Potion potion)
    {
        int idx = GetPotionColor(potion);
        return idx >= 0 && idx < _potionColors.Length ? _potionColors[idx] : "未知";
    }

    /// <summary>
    ///     获取卷轴的标签
    /// </summary>
    public static string GetScrollLabel(Scroll scroll)
    {
        string key = scroll.GetType().Name;
        if (_scrollLabelMap.TryGetValue(key, out int label))
            return _scrollLabels[label];
        return "???";
    }

    /// <summary>
    ///     获取戒指的宝石名称
    /// </summary>
    public static string GetRingGemName(Ring ring)
    {
        string key = ring.GetType().Name;
        if (_ringGemMap.TryGetValue(key, out int gem))
            return _ringGems[gem];
        return "未知";
    }

    /// <summary>
    ///     获取所有药水类型
    /// </summary>
    private static List<string> GetPotionTypes()
    {
        return new List<string>
        {
            nameof(HealingPotion), nameof(VitalityPotion), nameof(StrengthPotion),
            nameof(ExperiencePotion), nameof(InvisibilityPotion), nameof(LiquidFlamePotion),
            nameof(FrostPotion), nameof(ParalysisPotion), nameof(PurifyPotion),
            nameof(HastePotion), nameof(MindVisionPotion), nameof(ToxicGasPotion),
            nameof(TeleportPotion),
        };
    }

    /// <summary>
    ///     获取所有卷轴类型
    /// </summary>
    private static List<string> GetScrollTypes()
    {
        return new List<string>
        {
            nameof(IdentifyScroll), nameof(UpgradeScroll), nameof(RemoveCurseScroll),
            nameof(MagicMappingScroll), nameof(TeleportScroll), nameof(ChallengeScroll),
            nameof(TerrorScroll), nameof(RetributionScroll), nameof(MirrorImageScroll),
            nameof(FogScroll), nameof(CurseScroll), nameof(AwakeningScroll),
        };
    }

    /// <summary>
    ///     获取所有戒指类型
    /// </summary>
    private static List<string> GetRingTypes()
    {
        return new List<string>
        {
            nameof(RingOfAccuracy), nameof(RingOfArcana), nameof(RingOfElements),
            nameof(RingOfEnergy), nameof(RingOfEvasion), nameof(RingOfForce),
            nameof(RingOfFuror), nameof(RingOfHaste), nameof(RingOfMight),
            nameof(RingOfSharpshooting), nameof(RingOfTenacity), nameof(RingOfWealth),
        };
    }

    /// <summary>
    ///     打乱数组（Fisher-Yates）
    /// </summary>
    private static void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}