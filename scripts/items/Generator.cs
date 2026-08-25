using MyShatteredPixelDungeon.scripts.items.weapons;
using MyShatteredPixelDungeon.scripts.items.armors;
using MyShatteredPixelDungeon.scripts.dungeon;

namespace MyShatteredPixelDungeon.scripts.items;

/// <summary>
///     物品生成类别，对应原版 Generator.Category
/// </summary>
public sealed class ItemCategory
{
    /// <summary>类别名称</summary>
    public string Name { get; }

    /// <summary>基础生成概率</summary>
    public float Chance { get; set; }

    /// <summary>数量标准差</summary>
    public int QuantitySigma { get; set; } = 1;

    /// <summary>默认物品类型</summary>
    public Type? DefaultItem { get; set; }

    /// <summary>可能生成的物品列表</summary>
    public List<Type> Items { get; } = new();

    public ItemCategory(string name, float chance)
    {
        Name = name;
        Chance = chance;
    }
}

/// <summary>
///     物品生成器，对应原版 com.shatteredpixel.shatteredpixeldungeon.items.Generator
///     管理所有物品的生成概率和随机生成
/// </summary>
public static class Generator
{
    // 预定义类别
    public static readonly ItemCategory Weapon = new("WEAPON", 1f) { QuantitySigma = 1 };
    public static readonly ItemCategory Armor = new("ARMOR", 0.8f) { QuantitySigma = 1 };
    public static readonly ItemCategory Potion = new("POTION", 0.5f) { QuantitySigma = 2 };
    public static readonly ItemCategory Scroll = new("SCROLL", 0.5f) { QuantitySigma = 2 };
    public static readonly ItemCategory Wand = new("WAND", 0.3f) { QuantitySigma = 1 };
    public static readonly ItemCategory Ring = new("RING", 0.2f) { QuantitySigma = 1 };
    public static readonly ItemCategory Food = new("FOOD", 0.3f) { QuantitySigma = 1 };
    public static readonly ItemCategory Gold = new("GOLD", 1f) { QuantitySigma = 5 };
    public static readonly ItemCategory Seed = new("SEED", 0.2f) { QuantitySigma = 2 };
    public static readonly ItemCategory Bomb = new("BOMB", 0.1f) { QuantitySigma = 1 };
    public static readonly ItemCategory Missile = new("MISSILE", 0.5f) { QuantitySigma = 3 };

    /// <summary>所有类别</summary>
    public static readonly List<ItemCategory> AllCategories = new()
    {
        Weapon, Armor, Potion, Scroll, Wand, Ring, Food, Gold, Seed, Bomb, Missile
    };

    /// <summary>已使用的物品追踪（用于控制稀有度）</summary>
    private static readonly HashSet<Type> _usedItems = new();

    /// <summary>已生成的物品数量</summary>
    private static int _totalGenerated;

    static Generator()
    {
        InitCategories();
    }

    /// <summary>
    ///     初始化各类别的物品列表
    /// </summary>
    private static void InitCategories()
    {
        // 武器（近战）
        Weapon.Items.AddRange(new[]
        {
            typeof(Dagger), typeof(ShortSword), typeof(Gladius), typeof(LongSword),
            typeof(BattleAxe), typeof(WarHammer), typeof(Crossbow), typeof(Spear),
            typeof(Glaive), typeof(Scimitar), typeof(Sickle), typeof(HandAxe)
        });

        // 护甲
        Armor.Items.AddRange(new[]
        {
            typeof(ClothArmor), typeof(LeatherArmor), typeof(MailArmor),
            typeof(ScaleArmor), typeof(PlateArmor)
        });

        // 法杖（预留）
        // Wand.Items.AddRange(...);

        // 戒指（预留）
        // Ring.Items.AddRange(...);
    }

    /// <summary>
    ///     随机生成一个物品
    /// </summary>
    public static Item? RandomItem(ItemCategory category = null)
    {
        category ??= RandomCategory();
        if (category == null) return null;

        return CreateItem(category);
    }

    /// <summary>
    ///     随机选择一个类别（基于概率权重）
    /// </summary>
    public static ItemCategory? RandomCategory()
    {
        float total = AllCategories.Sum(c => c.Chance);
        if (total <= 0) return null;

        float roll = System.Random.Shared.NextSingle() * total;
        float cumulative = 0;

        foreach (var cat in AllCategories)
        {
            cumulative += cat.Chance;
            if (roll <= cumulative) return cat;
        }

        return AllCategories.LastOrDefault();
    }

    /// <summary>
    ///     创建指定类别的一个物品实例
    /// </summary>
    public static Item? CreateItem(ItemCategory category)
    {
        if (category.Items.Count == 0)
        {
            // 使用默认物品
            if (category.DefaultItem != null)
                return Activator.CreateInstance(category.DefaultItem) as Item;
            return null;
        }

        // 随机选择物品类型
        int idx = System.Random.Shared.Next(category.Items.Count);
        var itemType = category.Items[idx];
        var item = Activator.CreateInstance(itemType) as Item;

        if (item != null)
        {
            _totalGenerated++;
            _usedItems.Add(itemType);

            // 随机数量（仅对可堆叠物品）
            if (item.Stackable && category.QuantitySigma > 1)
            {
                int qty = 1 + (int)(System.Random.Shared.NextSingle() * category.QuantitySigma);
                item.Quantity = Math.Min(qty, item.MaxStack);
            }
        }

        return item;
    }

    /// <summary>
    ///     按指定概率权重随机生成物品
    /// </summary>
    public static Item? RandomUsingDefaults()
    {
        var category = RandomCategory();
        return category != null ? CreateItem(category) : null;
    }



    /// <summary>
    ///     重置所有生成状态（新游戏）
    /// </summary>
    public static void FullReset()
    {
        _usedItems.Clear();
        _totalGenerated = 0;
    }

    /// <summary>
    ///     获取已生成物品总数
    /// </summary>
    public static int TotalGenerated => _totalGenerated;

    /// <summary>
    ///     生成金币
    /// </summary>
    public static Item CreateGold(int amount)
    {
        return new Gold { Quantity = amount };
    }
}

// ========== 基础物品实现 ==========

/// <summary>
///     金币
/// </summary>
public sealed class Gold : Item
{
    public override string Name => "金币";
    public override int MaxStack => 99999;
}

/// <summary>
///     匕首（Tier 1）
/// </summary>
public sealed class Dagger : MeleeWeapon
{
    public override int Tier => 1;
    public override string Name => "匕首";
}

/// <summary>
///     短剑（Tier 1）
/// </summary>
public sealed class ShortSword : MeleeWeapon
{
    public override int Tier => 1;
    public override string Name => "短剑";
}

/// <summary>
///     长剑（Tier 2）
/// </summary>
public sealed class Gladius : MeleeWeapon
{
    public override int Tier => 2;
    public override string Name => "长剑";
}

/// <summary>
///     阔剑（Tier 3）
/// </summary>
public sealed class LongSword : MeleeWeapon
{
    public override int Tier => 3;
    public override string Name => "阔剑";
}

/// <summary>
///     战斧（Tier 4）
/// </summary>
public sealed class BattleAxe : MeleeWeapon
{
    public override int Tier => 4;
    public override string Name => "战斧";
}

/// <summary>
///     战锤（Tier 5）
/// </summary>
public sealed class WarHammer : MeleeWeapon
{
    public override int Tier => 5;
    public override string Name => "战锤";
}

/// <summary>
///     十字弓（Tier 3）
/// </summary>
public sealed class Crossbow : MeleeWeapon
{
    public override int Tier => 3;
    public override string Name => "十字弓";
}

/// <summary>
///     长矛（Tier 2）
/// </summary>
public sealed class Spear : MeleeWeapon
{
    public override int Tier => 2;
    public override string Name => "长矛";
    public override int Reach => 2;
    public Spear() { Reach = 2; }
}

/// <summary>
///     长戟（Tier 4）
/// </summary>
public sealed class Glaive : MeleeWeapon
{
    public override int Tier => 4;
    public override string Name => "长戟";
    public override int Reach => 2;
    public Glaive() { Reach = 2; }
}

/// <summary>
///     弯刀（Tier 3）
/// </summary>
public sealed class Scimitar : MeleeWeapon
{
    public override int Tier => 3;
    public override string Name => "弯刀";
}

/// <summary>
///     镰刀（Tier 2）
/// </summary>
public sealed class Sickle : MeleeWeapon
{
    public override int Tier => 2;
    public override string Name => "镰刀";
}

/// <summary>
///     手斧（Tier 1）
/// </summary>
public sealed class HandAxe : MeleeWeapon
{
    public override int Tier => 1;
    public override string Name => "手斧";
}