using GFramework.Core.Abstractions.controller;
using GFramework.Game.Abstractions.enums;
using GFramework.Game.Abstractions.ui;
using GFramework.Godot.ui;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.core.ui;
using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.potions;
using MyShatteredPixelDungeon.scripts.items.scrolls;
using MyShatteredPixelDungeon.scripts.items.armors;
using MyShatteredPixelDungeon.scripts.items.food;
using MyShatteredPixelDungeon.scripts.systems;
using Godot;

namespace MyShatteredPixelDungeon.scripts.menu.demo_page;

/// <summary>
///     简易 Demo 测试页面，用于验证物品/战斗/背包等核心系统
/// </summary>
[Log]
[ContextAware]
public partial class DemoPage : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    private IUiPageBehavior? _page;
    private HeroEntity? _hero;
    private RichTextLabel? _demoLog;

    public override void _Ready()
    {
        _ = ReadyAsync();
        ConnectPageSignals();
        RegisterEvents();
    }

    public IUiPageBehavior GetPage()
    {
        _page ??= UiPageBehaviorFactory.Create<DemoPage>(this, UiKeyStr, UiLayer.Page);
        return _page;
    }

    private async Task ReadyAsync()
    {
        await Task.CompletedTask;
        _demoLog = GetNode<RichTextLabel>("%DemoLog");
        _hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();

        if (_hero == null)
        {
            Log("❌ 未找到英雄实体，请先启动游戏");
            return;
        }

        Log("=== My-Shattered-Pixel-Dungeon Demo ===");
        Log($"英雄: HP {_hero.Hp}/{_hero.MaxHp}, STR {_hero.Strength}, 等级 {_hero.Level}");
        Log("");

        // 按钮通过信号连接
    }

    private void Log(string msg)
    {
        if (_demoLog != null)
            _demoLog.Text += msg + "\n";
    }

    private void RegisterEvents() { }

    private void ConnectPageSignals()
    {
        GetNode<Button>("%SpawnItems").Pressed += OnSpawnItems;
        GetNode<Button>("%EquipWeapon").Pressed += OnEquipWeapon;
        GetNode<Button>("%EquipArmor").Pressed += OnEquipArmor;
        GetNode<Button>("%ShowInventory").Pressed += OnShowInventory;
        GetNode<Button>("%TestCombat").Pressed += OnTestCombat;
        GetNode<Button>("%TestIdentify").Pressed += OnTestIdentification;
        GetNode<Button>("%ClearLog").Pressed += OnClearLog;
    }

    // ========== 按钮回调 ==========

    /// <summary>生成测试物品</summary>
    private void OnSpawnItems()
    {
        if (_hero == null) return;

        // 生成各种测试物品
        var potion = new HealingPotion();
        var scroll = new IdentifyScroll();
        var food = new MysteryMeat();
        var gold = new Gold { Quantity = 50 };

        _hero.Inventory.TryAdd(potion);
        _hero.Inventory.TryAdd(scroll);
        _hero.Inventory.TryAdd(food);
        _hero.Inventory.TryAdd(gold);

        Log($"✅ 添加测试物品: 治疗药水, 鉴定卷轴, 神秘肉, 金币×50");
    }

    /// <summary>装备测试武器</summary>
    private void OnEquipWeapon()
    {
        if (_hero == null) return;
        var weapon = new Gladius();
        weapon.Identify();
        _hero.Inventory.TryAdd(weapon);
        _hero.EquipWeapon(weapon);
        Log($"✅ 装备长剑（Tier 2, 伤害 {weapon.DamageMin}-{weapon.DamageMax})");
    }

    /// <summary>装备测试护甲</summary>
    private void OnEquipArmor()
    {
        if (_hero == null) return;
        var armor = new MailArmor();
        armor.Identify();
        _hero.Inventory.TryAdd(armor);
        _hero.EquipArmor(armor);
        Log($"✅ 装备锁甲（减伤 {armor.DamageReduction}-{armor.DamageReductionMax})");
    }

    /// <summary>显示背包</summary>
    private void OnShowInventory()
    {
        if (_hero == null) return;
        Log($"--- 背包 ({_hero.Inventory.Items.Count} 件) ---");
        foreach (var item in _hero.Inventory.Items)
        {
            string equip = item is EquipableItem e && e.IsEquipped ? " [E]" : "";
            string qty = item.Stackable && item.Quantity > 1 ? $" x{item.Quantity}" : "";
            Log($"  {item.Name}{equip}{qty}");
        }
        if (_hero.EquippedWeapon != null) Log($"武器: {_hero.EquippedWeapon.Name}");
        if (_hero.EquippedArmor != null) Log($"护甲: {_hero.EquippedArmor.Name}");
        Log($"金币: {InventorySystem.GetTotalGold(_hero)}");
    }

    /// <summary>测试战斗</summary>
    private void OnTestCombat()
    {
        if (_hero == null) return;
        // 创建一个测试怪物
        var rat = Actor.All().OfType<MobEntity>().FirstOrDefault();
        if (rat == null)
        {
            Log("⚠️ 未找到测试怪物");
            return;
        }

        Log($"攻击 {rat.GetType().Name} (HP: {rat.Hp})...");
        var result = CombatSystem.PerformAttack(_hero, rat);
        if (result.Missed)
            Log("  ❌ 未命中!");
        else
            Log($"  💥 造成 {result.Damage} 点伤害 (剩余 HP: {rat.Hp})");
    }

    /// <summary>测试随机识别</summary>
    private void OnTestIdentification()
    {
        // 初始化识别系统
        IdentificationSystem.Initialize();
        Log("--- 药水颜色映射 ---");
        var potions = new Potion[] {
            new HealingPotion(), new StrengthPotion(), new InvisibilityPotion(),
            new ParalysisPotion(), new HastePotion(), new FrostPotion()
        };
        foreach (var p in potions)
            Log($"  {p.Name}");

        Log("--- 卷轴标签映射 ---");
        var scrolls = new Scroll[] {
            new IdentifyScroll(), new UpgradeScroll(), new TerrorScroll(),
            new RetributionScroll(), new RemoveCurseScroll(), new TeleportScroll()
        };
        foreach (var s in scrolls)
            Log($"  {s.Name}");
    }

    /// <summary>清空日志</summary>
    private void OnClearLog()
    {
        if (_demoLog != null) _demoLog.Text = "";
    }
}