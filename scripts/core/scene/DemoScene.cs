using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.dungeon;
using MyShatteredPixelDungeon.scripts.dungeon.geometry;
using MyShatteredPixelDungeon.scripts.entities;
using MyShatteredPixelDungeon.scripts.entities.mobs;
using MyShatteredPixelDungeon.scripts.entities.buffs;
using MyShatteredPixelDungeon.scripts.items;
using MyShatteredPixelDungeon.scripts.items.potions;
using MyShatteredPixelDungeon.scripts.items.scrolls;
using MyShatteredPixelDungeon.scripts.items.food;
using MyShatteredPixelDungeon.scripts.systems;
using Godot;

namespace MyShatteredPixelDungeon.scripts.core.scene;

/// <summary>
///     简易可玩 Demo 场景，包含地牢渲染、英雄移动、回合系统
///     WASD 移动，空格等待，H 喝药水
/// </summary>
[Log]
[ContextAware]
public partial class DemoScene : Node2D
{
    // 节点
    private Control? _uiRoot;
    private RichTextLabel? _statusLabel;
    private RichTextLabel? _logLabel;

    // 游戏状态
    private DungeonData? _data;
    private HeroEntity? _hero;
    private readonly List<(int Pos, string Glyph, Color Color)> _entities = new();

    // 图块尺寸
    private const int TileSize = 16;

    public override void _Ready()
    {
        SetupUI();
        InitGame();
        QueueRedraw(); // 触发 _Draw
        UpdateStatus();
        _log.Debug("Demo 场景就绪");
        Log("🎮 WASD 移动 | 空格等待 | H 喝药水 | R 重置");
    }

    private void SetupUI()
    {
        // 状态标签
        _statusLabel = new RichTextLabel();
        _statusLabel.Name = "StatusLabel";
        _statusLabel.Position = new Vector2(10, 10);
        _statusLabel.Size = new Vector2(1000, 40);
        _statusLabel.BbcodeEnabled = true;
        AddChild(_statusLabel);

        // 操作提示
        var hint = new Label();
        hint.Text = "WASD 移动 | 空格等待 | H 喝药水 | R 重置";
        hint.Position = new Vector2(10, 660);
        hint.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        AddChild(hint);

        // 日志标签
        _logLabel = new RichTextLabel();
        _logLabel.Name = "LogLabel";
        _logLabel.Position = new Vector2(700, 60);
        _logLabel.Size = new Vector2(500, 400);
        _logLabel.BbcodeEnabled = true;
        _logLabel.ScrollActive = true;
        AddChild(_logLabel);
    }

    private void InitGame()
    {
        Actor.Clear();
        CharEntity.ClearPositions();
        Generator.FullReset();
        GroundItemManager.Clear();
        IdentificationSystem.Initialize();

        // 生成地牢
        int seed = Random.Shared.Next();
        var generator = new LevelGenerator(1, seed);
        _data = generator.Generate();

        // 创建英雄
        _hero = new HeroEntity { Pos = _data.Entrance };
        Actor.Add(_hero);

        // 初始装备
        var dagger = new Dagger { Level = 1 };
        dagger.Identify();
        _hero.Inventory.TryAdd(dagger);
        _hero.EquipWeapon(dagger);
        _hero.Inventory.TryAdd(new Gold { Quantity = 100 });
        _hero.Inventory.TryAdd(new HealingPotion { Quantity = 3 });
        _hero.Inventory.TryAdd(new IdentifyScroll());
        _hero.Inventory.TryAdd(new MysteryMeat());

        // 放置怪物
        PlaceMobs();

        // 放置地面物品
        PlaceGroundItems();

        // 刷新实体列表
        RefreshEntities();
    }

    private void PlaceMobs()
    {
        if (_data == null) return;
        var entrance = _data.CellToPoint(_data.Entrance);

        for (int dx = -5; dx <= 5; dx++)
        {
            for (int dy = -5; dy <= 5; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = entrance.X + dx;
                int y = entrance.Y + dy;
                if (!_data.InsideMap(x, y)) continue;
                int cell = _data.PointToCell(new Point(x, y));
                if (_data.Passable[cell] && CharEntity.FindAt(cell) == null)
                {
                    var rat = new Rat { Pos = cell };
                    Actor.Add(rat);
                }
            }
        }
    }

    private void PlaceGroundItems()
    {
        if (_data == null) return;
        var entrance = _data.CellToPoint(_data.Entrance);

        TryPlaceItem(new Point(entrance.X + 2, entrance.Y), new HealingPotion { Quantity = 2 });
        TryPlaceItem(new Point(entrance.X + 2, entrance.Y + 1), new IdentifyScroll());
        TryPlaceItem(new Point(entrance.X + 1, entrance.Y + 2), new Gold { Quantity = 25 });
        TryPlaceItem(new Point(entrance.X - 2, entrance.Y), new StrengthPotion());
    }

    private void TryPlaceItem(Point p, Item item)
    {
        if (_data == null) return;
        if (!_data.InsideMap(p.X, p.Y)) return;
        int cell = _data.PointToCell(p);
        if (_data.Passable[cell] && CharEntity.FindAt(cell) == null)
            GroundItemManager.Drop(cell, item);
    }

    /// <summary>
    ///     刷新场景实体显示列表
    /// </summary>
    private void RefreshEntities()
    {
        _entities.Clear();
        if (_hero != null)
            _entities.Add((_hero.Pos, "🧙", new Color(0.2f, 0.6f, 1.0f)));

        foreach (var mob in Actor.All().OfType<MobEntity>().Where(m => m.IsAlive))
            _entities.Add((mob.Pos, "🐀", new Color(1.0f, 0.3f, 0.2f)));
    }

    /// <summary>
    ///     绘制地图（使用 _Draw 避免依赖 TileSet 资源）
    /// </summary>
    public override void _Draw()
    {
        if (_data == null) return;

        // 背景
        DrawRect(new Rect2(0, 60, _data.Width * TileSize, _data.Height * TileSize),
            new Color(0.1f, 0.1f, 0.1f));

        // 地形
        for (int i = 0; i < _data.Length; i++)
        {
            var p = _data.CellToPoint(i);
            var rect = new Rect2(p.X * TileSize, p.Y * TileSize + 60, TileSize, TileSize);

            Color color;
            if (_data.Passable[i])
            {
                // 可通行 → 地面色
                color = new Color(0.4f, 0.35f, 0.3f);
                if (i == _data.Entrance) color = new Color(0.3f, 0.6f, 0.3f); // 入口
                else if (i == _data.Exit) color = new Color(0.8f, 0.5f, 0.2f); // 出口
                else if (_data.Water[i]) color = new Color(0.2f, 0.4f, 0.8f);
                else if (_data.Flammable[i]) color = new Color(0.3f, 0.5f, 0.2f);
            }
            else
            {
                // 不可通行 → 墙壁
                color = new Color(0.5f, 0.5f, 0.5f);
            }

            DrawRect(rect, color);
        }

        // 地面物品
        foreach (var heap in GroundItemManager.AllHeaps)
        {
            var p = _data.CellToPoint(heap.Pos);
            var label = heap.TopItem switch
            {
                _ when heap.TopItem is Gold => "💰",
                _ when heap.TopItem is Potion => "🧪",
                _ when heap.TopItem is Scroll => "📜",
                _ when heap.TopItem is Food => "🍖",
                _ => "📦"
            };
            DrawString(ThemeDB.FallbackFont, new Vector2(p.X * TileSize + 2, p.Y * TileSize + 76),
                label, HorizontalAlignment.Left, TileSize - 4, 14, Color.FromHtml("#e8e8e8"));
        }

        // 实体（英雄/怪物）
        foreach (var (pos, glyph, _) in _entities)
        {
            var p = _data.CellToPoint(pos);
            DrawString(ThemeDB.FallbackFont, new Vector2(p.X * TileSize, p.Y * TileSize + 74),
                glyph, HorizontalAlignment.Left, TileSize, 16, Color.FromHtml("#e8e8e8"));
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
            HandleKey(key.Keycode);
    }

    private void HandleKey(Key key)
    {
        if (_hero == null || _data == null) return;

        switch (key)
        {
            case Key.R:
                GetTree().ReloadCurrentScene();
                return;
        }

        int targetPos = _hero.Pos;
        bool acted = false;

        switch (key)
        {
            case Key.W or Key.Up: targetPos = _hero.Pos - _data.Width; acted = TryMove(targetPos); break;
            case Key.S or Key.Down: targetPos = _hero.Pos + _data.Width; acted = TryMove(targetPos); break;
            case Key.A or Key.Left: targetPos = _hero.Pos - 1; acted = TryMove(targetPos); break;
            case Key.D or Key.Right: targetPos = _hero.Pos + 1; acted = TryMove(targetPos); break;
            case Key.Space:
                _hero.Spend(1f);
                acted = true;
                ProcessMobTurns();
                Log("⏳ 等待...");
                break;
            case Key.H:
                var potion = _hero.Inventory.Find<HealingPotion>();
                if (potion != null)
                {
                    potion.Detach(_hero.Inventory);
                    potion.Apply(_hero);
                    Log($"💚 喝治疗药水 (HP: {_hero.Hp}/{_hero.MaxHp})");
                    acted = true;
                    ProcessMobTurns();
                }
                else Log("没有治疗药水!");
                break;
        }

        if (acted)
        {
            RefreshEntities();
            PlaceHero();
            UpdateStatus();
            CheckDeath();
        }
    }

    private bool TryMove(int targetPos)
    {
        if (_hero == null || _data == null) return false;
        if (targetPos < 0 || targetPos >= _data.Length) return false;
        if (!_data.Passable[targetPos]) return false;

        // 攻击怪物
        var target = CharEntity.FindAt(targetPos);
        if (target is MobEntity mob && mob.IsAlive)
        {
            var result = CombatSystem.PerformAttack(_hero, mob);
            Log(result.Missed
                ? $"⚔️ 攻击 → 未命中!"
                : $"⚔️ 攻击 → {result.Damage} 伤害");
            if (!mob.IsAlive)
            {
                _killCount++;
                Log($"💀 击杀 Rat! (掉落金币) [击杀数: {_killCount}]");
                GroundItemManager.Drop(targetPos, new Gold { Quantity = 5 });
            }
            _hero.Spend(1f);
            ProcessMobTurns();
            return true;
        }

        // 拾取物品
        if (GroundItemManager.HasItems(targetPos))
        {
            var picked = GroundItemManager.PickUpAll(targetPos, _hero);
            foreach (var item in picked)
                Log($"📦 拾取 {item.Name}");
        }

        // 移动
        _hero.Move(targetPos);
        _hero.Spend(1f);
        ProcessMobTurns();
        return true;
    }

    private void ProcessMobTurns()
    {
        if (_hero == null || _data == null) return;

        foreach (var mob in Actor.All().OfType<MobEntity>().Where(m => m.IsAlive))
        {
            int dist = Math.Abs((mob.Pos % _data.Width) - (_hero.Pos % _data.Width)) +
                       Math.Abs((mob.Pos / _data.Width) - (_hero.Pos / _data.Width));

            if (dist <= 1)
            {
                var result = CombatSystem.PerformAttack(mob, _hero);
                if (result.Hit) Log($"💥 Rat 攻击你 → {result.Damage} 伤害");
            }
            else if (dist <= 10)
            {
                int dx = Math.Sign((_hero.Pos % _data.Width) - (mob.Pos % _data.Width));
                int dy = Math.Sign((_hero.Pos / _data.Width) - (mob.Pos / _data.Width));
                int newPos = mob.Pos;

                if (dx != 0)
                {
                    int cand = mob.Pos + dx;
                    if (_data.Passable[cand] && CharEntity.FindAt(cand) == null) newPos = cand;
                }
                else if (dy != 0)
                {
                    int cand = mob.Pos + dy * _data.Width;
                    if (_data.Passable[cand] && CharEntity.FindAt(cand) == null) newPos = cand;
                }

                if (newPos != mob.Pos)
                {
                    mob.Move(newPos);
                    mob.Spend(1f);
                }
            }
        }
        RefreshEntities();
    }

    private void PlaceHero()
    {
        if (_hero == null) return;
        RefreshEntities();
    }

    private void UpdateStatus()
    {
        if (_hero == null || _statusLabel == null) return;
        _statusLabel.Text = $"[b]HP: {_hero.Hp}/{_hero.MaxHp}[/b]  " +
                            $"STR: {_hero.Strength}  层: 1  " +
                            $"💰{InventorySystem.GetTotalGold(_hero)}  " +
                            $"武器: {(_hero.EquippedWeapon?.Name ?? "无")}  " +
                            $"击杀: {_killCount}  存活怪: {Actor.All().OfType<MobEntity>().Count(m => m.IsAlive)}";
    }

    private void CheckDeath()
    {
        if (_hero != null && !_hero.IsAlive)
        {
            Log("💀 你死了！按 R 重开");
        }
    }

    private int _killCount;

    private void Log(string msg)
    {
        if (_logLabel != null)
        {
            _logLabel.Text += msg + "\n";
            // 限制日志长度
            if (_logLabel.GetTotalCharacterCount() > 3000)
                _logLabel.Text = _logLabel.Text[(^1500)..];
        }
    }
}