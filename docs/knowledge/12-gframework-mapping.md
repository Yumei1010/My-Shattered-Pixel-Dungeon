# 12 - 原版 → GFramework 映射设计

> 本文档是核心蓝图：如何用 GFramework 的 CQRS/ECS 架构实现 Shattered Pixel Dungeon 的原版系统

## 总览映射表

| 原版概念 | 原版实现 | GFramework 映射 | 说明 |
|---|---|---|---|
| Actor 回合调度 | `Actor.process()` 独立线程 | CQRS 事件驱动（`TurnSystem`） | 每回合广播事件，各系统订阅处理 |
| Dungeon 全局状态 | `Dungeon` 静态类 | `DungeonModel`（IModel） | 存放 hero/level/depth/gold 等 |
| Char 角色 | `Char` abstract | `CharEntity`（Godot Node） | 带属性组件 |
| Buff 状态 | `Buff extends Actor` | `BuffComponent` | 挂在 CharEntity 上 |
| Blob 区域效果 | `Blob extends Actor` | `BlobSystem` | 地图网格数据 + 回合扩散 |
| Item 物品 | `Item` 继承树 | `ItemEntity` + `ItemModel` | 数据与表现分离 |
| Heap 物品堆 | `Heap` | `HeapNode`（Godot Node2D） | 地面渲染 |
| Level 生成 | `Level.create()` | `LevelGenerator`（纯 C# 类） | 可单测 |
| Builder/Painter | Builder + Painter 模式 | 原样移植为纯 C# 类 | 可单测 |
| Terrain 位掩码 | `Terrain` 静态类 | `Terrain` 静态类 + `[Flags] TileFlags` | 原样移植 |
| ShadowCaster FOV | 递归阴影投射 | `ShadowCaster` 纯 C# 类 | 原样移植 |
| PathFinder A* | `PathFinder` | `PathFinder` 纯 C# 类 | 原样移植 |
| GameScene 输入 | `GameScene` 处理输入 | CQRS 命令分发 | 输入 → 命令 → 事实 |
| UI 页面 | `StatusPane`/`Toolbar` 等 | `scripts/menu/` 下的 UI 页面 | UiRouter 管理 |
| 设置 | `SPDSettings` | GFramework `SettingsModel` | 已内置 |
| 存档 | Bundle 序列化 | GFramework `ISerializer` + `IDataRepository` | JSON 实现 |

## 回合调度设计（最核心）

### 原版模型

```
原版: Actor 独立线程，按 time + priority 排序循环执行
```

### GFramework 事件驱动模型

```
方案 A：同步回合事件（推荐）
  TurnSystem (ISystem):
    ProcessTurn() → 广播 ActorTurnEvent
      ├── HeroTurnHandler     → 处理英雄动作
      ├── MobTurnHandler      → 处理怪物 AI
      ├── BuffTurnHandler     → 处理 Buff 计时
      └── BlobTurnHandler     → 处理区域扩散
```

**关键差异：** 原版用"时间轴调度"（每 Actor 有自己的 time），GFramework 更适合"整回合调度"（所有 Actor 每回合轮流 act）。

**建议混合方案：**
```
1. 保留时间轴模型（Actor 基类带 time 字段）
2. TurnSystem 每帧检查：谁的时间到了 → 发出 ActRequestEvent
3. 行动者执行 → 广播 ActionPerformedEvent → 系统更新状态
4. 英雄行动需要输入 → 挂起直到收到 MoveCommand/AttackCommand
```

```csharp
// 回合事件设计
public sealed class TurnStartedEvent;                    // 新回合开始
public sealed class TurnEndedEvent;                      // 回合结束
public sealed class CharActRequestEvent { required CharEntity Char; }
public sealed class CharMovedEvent { required CharEntity Char; required int From; required int To; }
public sealed class CharAttackedEvent { required CharEntity Attacker; required CharEntity Target; }
public sealed class CharDamagedEvent { required CharEntity Target; required int Damage; required object Source; }
public sealed class CharDiedEvent { required CharEntity Char; required object Cause; }
public sealed class ItemPickedUpEvent { required CharEntity PickUper; required ItemEntity Item; }
```

## 地牢生成设计

### 纯 C# 层（可单测）

```
scripts/dungeon/            # 地牢生成（纯 C#，无 Godot 依赖）
├── terrain/
│   ├── Terrain.cs          # 地形常量（原样移植）
│   └── TileFlags.cs        # [Flags] 位标志枚举
├── path/
│   ├── PathFinder.cs       # A* 寻路（原样移植）
│   └── BArray.cs           # boolean 数组工具（原样移植）
├── fov/
│   └── ShadowCaster.cs     # 递归阴影投射 FOV（原样移植）
├── rooms/
│   ├── Room.cs             # 房间基类（Rect + 邻居图）
│   ├── StandardRoom.cs     # 标准房间基类
│   ├── SpecialRoom.cs      # 特殊房间基类
│   ├── SecretRoom.cs       # 秘密房间基类
│   └── impls/              # 各房间实现
├── builders/
│   ├── Builder.cs          # 构建器接口
│   ├── LoopBuilder.cs      # 环状构建器
│   └── FigureEightBuilder.cs  # 8 字形构建器
├── painters/
│   ├── Painter.cs          # 绘制器基类
│   └── impls/              # 各区域 Painter
└── LevelGenerator.cs       # 生成管线入口

scripts/levels/             # Godot 层
├── DungeonLevel.cs         # Level 的 Godot 节点（TileMap + 实体）
└── DungeonLevelView.cs     # 渲染视图
```

### 生成流程

```
LevelGenerator.Generate(depth, seed) → DungeonData
  ├── 1. 计算种子（seedForDepth）
  ├── 2. 创建房间池（入口/出口/标准/特殊/秘密）
  ├── 3. Builder 连接房间（图算法）
  ├── 4. Painter 填充地形
  ├── 5. 构建标志数组（passable/losBlocking 等）
  ├── 6. 生成怪物和物品
  └── 返回 DungeonData（纯数据）

DungeonLevel（Godot 节点）加载 DungeonData:
  ├── 创建 TileMap 渲染地形
  ├── 创建怪物实体
  ├── 创建物品堆节点
  └── 初始化 FOV
```

## 实体设计（ECS 风格）

```csharp
// CharEntity —— 角色实体（Godot Node2D）
public partial class CharEntity : Node2D, IController
{
    // 基础属性
    public int Pos { get; set; }           // 地图坐标（cell index）
    public int HP { get; set; }            // 生命值
    public int HT { get; set; }            // 最大生命
    public int Strength { get; set; }      // 力量
    public float BaseSpeed { get; set; }   // 基础速度
    public Alignment Alignment { get; set; } // 阵营
    public bool Flying { get; set; }       // 飞行
    public bool Rooted { get; set; }       // 定身

    // Buff 容器
    public List<BuffComponent> Buffs { get; } = new();

    // 组件挂载
    public CharSprite Sprite { get; private set; }
}

// BuffComponent —— 状态组件（Node）
public partial class BuffComponent : Node, IController
{
    public BuffType Type { get; init; }    // 正面/负面/中性
    public float Duration { get; set; }    // 持续时间
    public bool Announced { get; set; }
    public virtual void OnAct() { }        // 每回合执行
    public virtual void OnAttach(CharEntity target) { }
    public virtual void OnDetach() { }
}
```

## 物品设计

```csharp
// ItemModel —— 物品数据（纯 C#，可序列化）
public sealed class ItemModel
{
    public string Type { get; set; }       // 物品类型标识
    public int Image { get; set; }         // 图标索引
    public bool Stackable { get; set; }    // 可堆叠
    public int Quantity { get; set; }      // 数量
    public int Level { get; set; }         // 强化等级
    public bool Cursed { get; set; }       // 诅咒
    public bool Identified { get; set; }   // 已鉴定
}

// ItemEntity —— 物品实体（Node2D，用于地图上的物品堆）
public partial class ItemEntity : Node2D, IController
{
    public ItemModel Data { get; private set; }
    public int Cell { get; set; }
    public Sprite2D Icon { get; private set; }
}

// InventoryModel —— 背包数据（纯 C#）
public sealed class InventoryModel
{
    public List<ItemModel> Backpack { get; } = new();
    public ItemModel? Weapon { get; set; }
    public ItemModel? Armor { get; set; }
    public ItemModel? Ring1 { get; set; }
    public ItemModel? Ring2 { get; set; }
    public ItemModel? Misc { get; set; }
}
```

## 命令/事件设计

```csharp
// ---- 玩家动作命令 ----
public sealed class MoveCommand(MoveCommandInput input) : AbstractCommand<MoveCommandInput>;
public sealed class AttackCommand(AttackCommandInput input) : AbstractCommand<AttackCommandInput>;
public sealed class PickUpCommand(PickUpCommandInput input) : AbstractCommand<PickUpCommandInput>;
public sealed class InteractCommand(InteractCommandInput input) : AbstractCommand<InteractCommandInput>;
public sealed class UseItemCommand(UseItemCommandInput input) : AbstractCommand<UseItemCommandInput>;
public sealed class EquipCommand(EquipCommandInput input) : AbstractCommand<EquipCommandInput>;
public sealed class ThrowCommand(ThrowCommandInput input) : AbstractCommand<ThrowCommandInput>;
public sealed class WaitCommand : AbstractCommand;
public sealed class RestCommand : AbstractCommand;

// ---- 游戏事件 ----
public sealed class TurnAdvancedEvent;                    // 回合推进
public sealed class CharMovedEvent { required int From; required int To; }
public sealed class CharAttackedEvent { required CharEntity Attacker; required CharEntity Target; }
public sealed class CharDamagedEvent { required int Damage; required string Source; }
public sealed class CharDiedEvent { required CharEntity Char; }
public sealed class MobActedEvent { required CharEntity Mob; }
public sealed class LevelGeneratedEvent { required DungeonData Data; }
public sealed class ItemPickedUpEvent { required ItemModel Item; }
public sealed class HeroLeveledUpEvent { required int Level; }
public sealed class FovUpdatedEvent;
```

## 输入系统映射

```
原版: SPDAction (枚举动作) → GameScene 处理 → Hero.handle()
我们: Godot InputEvent → InputController → CQRS 命令 → TurnSystem
```

```
输入流程:
  键盘/鼠标输入
    → GlobalInputController（自动加载单例）
    → 根据当前 UI 状态分发（游戏场景 vs 菜单）
    → 游戏场景: 生成 MoveCommand / AttackCommand / WaitCommand
    → TurnSystem 处理命令 → 更新世界 → 广播事件 → UI 响应
```

## 状态机设计

```
AppState          → 应用初始状态（清除 UI/场景）
TitleState        → 标题画面
GameState         → 游戏中（DungeonLevel 场景 + HUD）
PausedState       → 暂停（菜单覆盖）
InventoryState    → 背包（弹窗覆盖）
DiedState         → 死亡
VictoryState      → 胜利
```

```
状态 → UI 映射:
  TitleState.OnEnter → UiRouter.Push(MainMenuPage)
  GameState.OnEnter  → SceneRouter.ChangeScene(MainScene) + UiRouter.Push(GameHudPage)
  PausedState.OnEnter → UiRouter.Push(PauseMenuPage)
```

## 开发里程碑对应的 GFramework 模块

| 里程碑 | 主要 GFramework 模块 |
|---|---|
| M1: 地牢生成 | `dungeon/` 纯 C# 类 + 单元测试 |
| M2: 回合+角色 | `TurnSystem`, `CharEntity`, CQRS 事件 |
| M3: 物品+战斗 | `InventoryModel`, `ItemEntity`, 战斗命令 |
| M4: 内容填充 | Buff/Blob 组件, 药水卷轴命令 |
| M5: 完整流程 | 状态机扩展, 存档集成 |
| M6: 模板抽象 | 抽取通用 SRPG 模块到独立层 |

## 测试策略

```
纯 C# 层（可单测）:
  ✓ LevelGenerator 生成测试（种子可复现）
  ✓ PathFinder 寻路测试
  ✓ ShadowCaster FOV 测试
  ✓ Terrain 标志测试
  ✓ 战斗公式测试
  ✓ 怪物 AI 测试

Godot 层（集成测试）:
  ○ CharEntity 组件测试
  ○ CQRS 命令/事件流测试
  ○ UI 页面冒烟测试
```