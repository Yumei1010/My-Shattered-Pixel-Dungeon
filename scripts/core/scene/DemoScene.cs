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
    private int _killCount;

    // 角色精灵（真实素材）
    private Sprite2D? _heroSprite;
    private readonly List<Sprite2D> _mobSprites = new();

    // 素材帧尺寸
    private const int HeroTileW = 16;
    private const int HeroTileH = 32;
    private const int MobTileW = 16;
    private const int MobTileH = 16;
    private const int TileSize = 16;

    // 地形 TileMap
    private TileMap? _tileMap;

    public override void _Ready()
    {
        SetupUI();
        InitGame();
        CreateEntitySprites();
        RenderTerrain();
        ConfigureCamera();
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

    }

    private void PlaceMobs()
    {
        if (_data == null) return;
        var entrance = _data.CellToPoint(_data.Entrance);

        // 只生成少量怪物（4 只），分散在距入口 7~15 格的远处
        const int targetCount = 4;
        int spawned = 0;
        const int maxAttempts = 500;

        for (int attempt = 0; spawned < targetCount && attempt < maxAttempts; attempt++)
        {
            // 随机方向 + 随机较远距离
            int ang = Random.Shared.Next(360);
            double rad = ang * Math.PI / 180.0;
            int dist = Random.Shared.Next(7, 16);
            int x = entrance.X + (int)Math.Round(Math.Cos(rad) * dist);
            int y = entrance.Y + (int)Math.Round(Math.Sin(rad) * dist);

            if (!_data.InsideMap(x, y)) continue;
            int cell = _data.PointToCell(new Point(x, y));
            if (_data.Passable[cell] && CharEntity.FindAt(cell) == null)
            {
                var rat = new Rat { Pos = cell };
                Actor.Add(rat);
                spawned++;
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
    /// <summary>
    ///     创建英雄/怪物精灵（从真实素材 spritesheet 取第一帧）
    /// </summary>
    private void CreateEntitySprites()
    {
        if (_data == null || _hero == null) return;

        _heroSprite = MakeSprite("res://assets/sprites/warrior.png", HeroTileW, HeroTileH);
        _heroSprite.ZIndex = 10;
        AddChild(_heroSprite);

        foreach (var s in _mobSprites) s.QueueFree();
        _mobSprites.Clear();

        foreach (var mob in Actor.All().OfType<MobEntity>().Where(m => m.IsAlive))
        {
            var sprite = MakeSprite("res://assets/sprites/rat.png", MobTileW, MobTileH);
            sprite.ZIndex = 10;
            AddChild(sprite);
            _mobSprites.Add(sprite);
        }

        SyncSprites();
    }

    /// <summary>
    ///     从 spritesheet 创建精灵（Region 显示第 0 帧）
    /// </summary>
    private static Sprite2D MakeSprite(string path, int fw, int fh)
    {
        var tex = GD.Load<Texture2D>(path);
        return new Sprite2D { Texture = tex, RegionEnabled = true, RegionRect = new Rect2(0, 0, fw, fh) };
    }

    /// <summary>
    ///     同步所有角色精灵到各自地图位置
    /// </summary>
    private void SyncSprites()
    {
        if (_data == null || _hero == null) return;

        var hp = _data.CellToPoint(_hero.Pos);
        if (_heroSprite != null)
        {
            _heroSprite.Position = new Vector2(hp.X * TileSize, hp.Y * TileSize + 60);
            _heroSprite.Offset = new Vector2(0, -HeroTileH + TileSize);
        }

        int idx = 0;
        foreach (var mob in Actor.All().OfType<MobEntity>().Where(m => m.IsAlive))
        {
            if (idx >= _mobSprites.Count) break;
            var mp = _data.CellToPoint(mob.Pos);
            var sprite = _mobSprites[idx++];
            sprite.Position = new Vector2(mp.X * TileSize, mp.Y * TileSize + 60);
            sprite.Offset = new Vector2(0, -MobTileH + TileSize);
        }
        QueueRedraw();
    }

    /// <summary>
    ///     相机跟随英雄并缩放到视野
    /// </summary>
    private void ConfigureCamera()
    {
        var cam = new Camera2D();
        var p = _data!.CellToPoint(_data.Entrance);
        cam.Position = new Vector2(p.X * TileSize, p.Y * TileSize + 60);
        cam.Zoom = new Vector2(1f, 1f);
        AddChild(cam);
    }

    /// <summary>
    ///     绘制地图（使用 _Draw 避免依赖 TileSet 资源）
    /// </summary>
    /// <summary>
    ///     用真实地形图集(tiles_sewers.png)渲染 TileMap
    /// </summary>
    private void RenderTerrain()
    {
        if (_data == null) return;

        var ts = new TileSet { TileSize = new Vector2I(TileSize, TileSize) };
        var atlas = new TileSetAtlasSource
        {
            Texture = GD.Load<Texture2D>("res://assets/environment/tiles_sewers.png"),
            TextureRegionSize = new Vector2I(TileSize, TileSize)
        };
        int sid = ts.AddSource(atlas, 0);

        _tileMap = new TileMap { TileSet = ts, Position = new Vector2(0, 60) };
        AddChild(_tileMap);

        for (int i = 0; i < _data.Length; i++)
        {
            var p = _data.CellToPoint(i);
            var cell = new Vector2I(p.X, p.Y);

            Vector2I atlasCoord = _data.Passable[i]
                ? GroundTile(_data, i)      // 可通行 → 地面类
                : new Vector2I(4, 0);       // 墙
            _tileMap.SetCell(0, cell, sid, atlasCoord);
        }
    }

    /// <summary>
    ///     挑选可通行格子的地面 tile（地形图集的行 0，列随 terrain）
    /// </summary>
    private static Vector2I GroundTile(DungeonData d, int i)
    {
        int t = d.Map[i];
        if (d.Water[i]) return new Vector2I(0, 5);      // 水纹
        if (i == d.Entrance) return new Vector2I(1, 0); // 入口地面
        if (i == d.Exit) return new Vector2I(6, 0);     // 出口
        if (d.Flammable[i]) return new Vector2I(2, 0);  // 草地
        if (t < 16) return new Vector2I(t, 0);          // 直接按 terrain 映射行0
        return new Vector2I(1, 0);                      // 默认地面
    }

    /// <summary>
    ///     格子 → 世界矩形（含地图偏移）
    /// </summary>
    private Rect2 CellRect(int cell)
    {
        var p = _data!.CellToPoint(cell);
        return new Rect2(p.X * TileSize, p.Y * TileSize + 60, TileSize, TileSize);
    }

    public override void _Draw()
    {
        if (_data == null) return;

        // 背景
        DrawRect(new Rect2(0, 60, _data.Width * TileSize, _data.Height * TileSize),
            new Color(0.1f, 0.1f, 0.1f));

        // 入口/出口高亮（覆盖在 TileMap 地形之上）
        if (_data.Entrance >= 0) DrawRect(CellRect(_data.Entrance), new Color(0.3f, 0.7f, 0.3f, 0.35f));
        if (_data.Exit >= 0) DrawRect(CellRect(_data.Exit), new Color(0.9f, 0.6f, 0.25f, 0.35f));

        // 地面物品（用不同颜色小方块区分类型）
        foreach (var heap in GroundItemManager.AllHeaps)
        {
            var p = _data.CellToPoint(heap.Pos);
            var rect = new Rect2(p.X * TileSize + 3, p.Y * TileSize + 63, 10, 10);
            Color itemColor = heap.TopItem switch
            {
                Gold => new Color(1f, 0.85f, 0.2f),      // 金币-黄
                Potion => new Color(0.3f, 0.9f, 0.4f),    // 药水-绿
                Scroll => new Color(0.9f, 0.9f, 0.85f),   // 卷轴-白
                Food => new Color(0.7f, 0.4f, 0.2f),      // 食物-棕
                _ => new Color(0.6f, 0.6f, 0.8f)           // 其他-蓝灰
            };
            DrawRect(rect, itemColor);
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
            SyncSprites();
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
            else if (dist <= 5)
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
        SyncSprites();
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