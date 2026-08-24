# 项目规划：My-Shattered-Pixel-Dungeon

> 全量复刻《破碎的像素地牢》(Shattered Pixel Dungeon) + 抽离战旗游戏模板
> 基于 Godot 4.6 + C# .NET 10 + GFramework 0.0.177

---

## 1. 项目愿景

### 使命

用 Godot 4.6 + C# 全量复刻《破碎的像素地牢》的全部核心玩法，并在过程中抽象出一套可复用的**战旗游戏模板**（Tactical RPG Template）。

### 成功标准

| 级别 | 标准 | 目标 |
|---|---|---|
| P0 | 可玩 | 完整 26 层地牢、5 个 BOSS、6 个职业、可通关 |
| P1 | 可复现 | 种子系统确保每层生成与行为可复现 |
| P2 | 可扩展 | 架构支持添加新怪物、新物品、新房间 |
| P3 | 可复用 | 战旗模板独立为 NuGet 包或代码库 |

### 非目标（明确不做）

- 多人联网
- 移动端适配（Android/iOS）
- Mod 支持
- 新内容创作（仅复刻原版，不做创新）

---

## 2. 技术栈

| 层 | 技术 | 版本 | 说明 |
|---|---|---|---|
| 引擎 | Godot | 4.6 (.NET) | GL Compatibility 渲染器，960x540 基础分辨率 |
| 运行时 | .NET | 10 | 最新 C# 特性 |
| 框架 | GFramework | 0.0.177 | CQRS/ECS 架构，NuGet: GeWuYou.GFramework |
| 测试 | xUnit | 2.9+ | 纯 C# 层可单测 |
| 序列化 | GFramework JsonSerializer | — | 存档使用 JSON 格式 |
| 构建 | dotnet CLI | — | Godot 导出模板 |

### 架构决策（ADR）

| 决策 ID | 决策 | 理由 |
|---|---|---|
| ADR-001 | 回合制用 CQRS 事件驱动，而非原版独立线程 | GFramework 原生支持，避免线程安全问题 |
| ADR-002 | 地牢生成用纯 C# 类（无 Godot 依赖） | 可单测，可复用到战旗模板 |
| ADR-003 | 实体用 Godot Node（简单 ECS），不用纯 ECS | 框架兼容性，Godot 树对调试友好 |
| ADR-004 | 输入→意图→解释器→命令的四层架构 | 解耦 UI 与逻辑，支持 AI/回放 |
| ADR-005 | 怪物 AI 用混合 FSM + 行为树 | 复杂行为可组合，简单怪物不增加复杂度 |
| ADR-006 | 事件总线扩展频段机制 | 避免类型爆炸，细粒度订阅 |
| ADR-007 | 存档使用 JSON 文件 | 可读性好，调试方便，与 GFramework 兼容 |
| ADR-008 | 场景用 Godot PackedScene 加载 | GFramework 原生支持，编辑器友好 |

---

## 3. 功能范围

### Phase 1：地牢引擎（核心地基）

| 模块 | 功能 | 优先级 | 依赖 |
|---|---|---|---|
| Terrain | 地形常量 + [Flags] 位标志 | P0 | — |
| PathFinder | A* 寻路（8 方向） | P0 | Terrain |
| ShadowCaster | 递归阴影投射 FOV | P0 | Terrain |
| Room | 房间基类（Rect + 邻居图） | P0 | — |
| StandardRoom | 空地/走廊/环形/柱子/洞穴~20 种 | P0 | Room |
| SpecialRoom | 商店/实验室/花园/献祭~10 种 | P1 | Room |
| EntranceRoom | 入口房间（5 种变体） | P0 | StandardRoom |
| ExitRoom | 出口房间（5 种变体） | P0 | StandardRoom |
| ConnectionRoom | 隧道/桥/走廊（6 种） | P0 | Room |
| Builder | 房间连接算法（Loop/FigureEight） | P0 | Room |
| Painter | 区域地形绘制 | P0 | Builder |
| LevelGenerator | 生成管线入口 | P0 | 以上全部 |
| SeedSystem | 种子驱动的可复现生成 | P0 | LevelGenerator |
| TileMapRenderer | Godot TileMap 渲染 | P0 | LevelGenerator |

### Phase 2：回合调度 + 角色系统

| 模块 | 功能 | 优先级 | 依赖 |
|---|---|---|---|
| TurnSystem | 回合调度（CQRS 事件驱动） | P0 | GFramework |
| Actor 时间线 | `spend()` / `delay()` 时间管理 | P0 | TurnSystem |
| CharEntity | 角色实体（HP/STR/速度/Buff 容器） | P0 | TurnSystem |
| HeroEntity | 英雄实体（职业/天赋/背包/动作） | P0 | CharEntity |
| MobEntity | 怪物实体（AI 状态机） | P0 | CharEntity |
| BuffComponent | 状态组件（计时/效果） | P0 | CharEntity |
| IntentSystem | 指令模式（Intent→Interpreter→Command） | P0 | GFramework 命令 |
| InputController | 输入处理（鼠标/键盘/触屏） | P0 | IntentSystem |
| Basic UI | 状态栏/日志/工具栏 | P0 | GFramework UiRouter |
| Camera | 地图跟随/缩放 | P0 | — |

### Phase 3：物品 + 战斗

| 模块 | 功能 | 优先级 | 依赖 |
|---|---|---|---|
| ItemModel | 物品数据模型 | P0 | — |
| HeapNode | 地面物品堆（Node2D） | P0 | TileMap |
| InventoryModel | 背包数据 | P0 | GFramework Model |
| WeaponSystem | 近战/远程武器 | P0 | ItemModel |
| ArmorSystem | 护甲/雕纹 | P0 | ItemModel |
| CombatSystem | 战斗公式（命中/伤害/防御） | P0 | CharEntity |
| Generator | 物品生成概率表 | P0 | ItemModel |
| Backpack UI | 背包界面 | P0 | GFramework UiRouter |
| Equipment UI | 装备界面 | P1 | Backpack UI |
| QuickSlot | 快捷槽 | P1 | Backpack UI |

### Phase 4：内容填充

| 模块 | 功能 | 优先级 | 依赖 |
|---|---|---|---|
| Potion | 12 种药水（含异域/酿造/灵药） | P0 | ItemModel |
| Scroll | 12 种卷轴（含异域） | P0 | ItemModel |
| Wand | 13 种法杖 | P1 | CombatSystem |
| Ring | 12 种戒指 | P1 | ItemModel |
| Artifact | 16 种神器 | P2 | ItemModel |
| Identification | 药水颜色/卷轴名称随机识别 | P0 | ItemModel |
| Upgrade | 升级系统（SoU + 装备强化） | P0 | ItemModel |
| Enchantment | 13 种武器附魔 | P1 | WeaponSystem |
| Glyph | 13 种护甲雕纹 | P1 | ArmorSystem |
| Food | 食物/饥饿度系统 | P0 | BuffComponent |
| Trap | 30+ 种陷阱 | P1 | TileMap |
| Plant | 13 种植物 | P1 | TileMap |
| Blob | 20 种区域效果 | P2 | TurnSystem |
| Mob AI | 68 种怪物行为（简单 FSM 起步） | P0 | MobEntity |
| Boss AI | 5 个 BOSS 行为（行为树） | P1 | Mob AI |

### Phase 5：完整游戏流程

| 模块 | 功能 | 优先级 | 依赖 |
|---|---|---|---|
| 5 个区域 | Sewer/Prison/Caves/City/Halls 各 5 层 | P0 | LevelGenerator |
| BOSS 战 | Goo→Tengu→DM-300→DwarfKing→Yog | P1 | Boss AI |
| 楼层切换 | Interlevel 过渡动画 | P0 | SceneRouter |
| 存档系统 | 保存/读取/自动存档 | P0 | GFramework Storage |
| 6 个职业 | Warrior/Mage/Rogue/Huntress/Duelist/Cleric | P2 | HeroEntity |
| 天赋树 | 每个职业 8 个天赋（4 层） | P2 | HeroEntity |
| 炼金术 | Alchemy 配方系统 | P2 | ItemModel |
| NPC 任务 | Ghost/Wandmaker/Blacksmith/Imp | P2 | Mob AI |
| 游戏结束 | 死亡/复活/胜利 | P0 | StateMachine |
| 排行榜 | Rankings | P3 | 存档 |

### Phase 6：战旗模板抽象

| 模块 | 功能 | 优先级 | 依赖 |
|---|---|---|---|
| 网格系统 | 通用网格/坐标/路径 | P3 | Phase 1 |
| 回合系统 | 通用回合调度 | P3 | Phase 2 |
| 属性系统 | 通用属性模板 | P3 | Phase 2 |
| 通用 UI | 菜单/状态栏/日志模板 | P3 | Phase 3-5 |
| 文档 | 使用说明 + 示例项目 | P3 | — |

---

## 4. 目录结构规划

> 最终目标目录结构，按阶段逐步实现

```
scripts/
├── core/                    # 框架核心（GFramework 扩展）
│   ├── GameArchitecture.cs  # 架构安装（已实现）
│   ├── band/                # 事件频段扩展
│   │   ├── EventBands.cs    # 频段常量定义
│   │   └── BandedEventBus.cs# 频段化事件总线
│   ├── controller/          # 输入控制器
│   │   └── GameInputController.cs
│   ├── ui/                  # UI 路由/工厂
│   │   ├── UiRouter.cs      # 已实现
│   │   ├── UiFactory.cs     # 已实现
│   │   └── ISimpleUiPage.cs # 已实现
│   ├── scene/               # 场景路由
│   │   └── SceneRouter.cs   # 已实现
│   ├── state/               # 状态实现
│   │   └── impls/           # 各游戏状态
│   └── resource/            # 配置资源
│
├── components/              # 可复用组件（Godot 节点）
│   ├── volume_container/    # 已实现
│   └── state_machine/       # 已实现
│
├── dungeon/                 # 地牢生成（纯 C#，可单测）
│   ├── Terrain.cs           # 地形常量
│   ├── TileFlags.cs         # [Flags] 位标志
│   ├── PathFinder.cs        # A* 寻路
│   ├── ShadowCaster.cs      # 递归阴影投射 FOV
│   ├── rooms/               # 房间体系
│   │   ├── Room.cs          # 房间基类
│   │   ├── standard/        # 标准房间实现
│   │   ├── special/         # 特殊房间实现
│   │   ├── secret/          # 秘密房间实现
│   │   └── connection/      # 连接房间实现
│   ├── builders/            # 构建器
│   │   ├── Builder.cs
│   │   ├── LoopBuilder.cs
│   │   └── FigureEightBuilder.cs
│   ├── painters/            # 绘制器
│   │   ├── Painter.cs
│   │   └── impls/           # 各区域 Painter
│   └── LevelGenerator.cs    # 生成管线入口
│
├── systems/                 # GFramework ISystem 实现
│   ├── TurnSystem.cs        # 回合调度系统
│   ├── CombatSystem.cs      # 战斗系统
│   ├── IntentSystem.cs      # 指令解释系统
│   ├── InventorySystem.cs   # 背包系统
│   ├── VisionSystem.cs      # FOV 系统
│   └── MobSpawnerSystem.cs  # 怪物重生系统
│
├── models/                  # GFramework IModel（数据层）
│   ├── DungeonModel.cs      # 游戏全局状态
│   ├── HeroModel.cs         # 英雄数据
│   └── ItemModel.cs         # 物品数据
│
├── entities/                # 游戏实体（Godot Node）
│   ├── CharEntity.cs        # 角色实体基类
│   ├── HeroEntity.cs        # 英雄实体
│   ├── MobEntity.cs         # 怪物实体
│   ├── ItemEntity.cs        # 物品实体
│   └── HeapNode.cs          # 物品堆节点
│
├── ai/                      # 怪物 AI（纯 C#）
│   ├── MobBrain.cs          # 顶层 FSM + 行为树容器
│   ├── MobStateMachine.cs   # 宏观状态机
│   ├── behavior_tree/       # 行为树引擎
│   │   ├── BTNode.cs        # 节点基类
│   │   ├── Composite.cs     # 组合节点（Selector/Sequence）
│   │   ├── Decorator.cs     # 装饰器（Inverter/Cooldown）
│   │   ├── Condition.cs     # 条件节点
│   │   └── Action.cs        # 动作节点
│   └── impls/               # 各怪物 AI 实现
│
├── cqrs/                    # CQRS 命令/事件
│   ├── audio/               # 音频（已实现）
│   ├── game/                # 游戏（已实现 ExitGameCommand）
│   ├── setting/             # 设置（已实现）
│   ├── graphics/            # 图形（已实现）
│   ├── combat/              # 战斗命令/事件
│   ├── movement/            # 移动命令/事件
│   ├── inventory/           # 背包命令/事件
│   └── dungeon/             # 地牢生成事件
│
├── menu/                    # UI 页面（被 UiRouter 管理）
│   ├── TemplatePage/        # 模板（已实现）
│   ├── TitlePage/           # 标题画面
│   ├── HeroSelectPage/      # 选职业
│   ├── GameHudPage/         # 游戏 HUD
│   ├── InventoryPage/       # 背包
│   ├── HeroPage/            # 英雄面板
│   └── SettingsPage/        # 设置
│
├── enums/                   # 枚举
│   ├── ui/UiKey.cs          # 已实现
│   ├── scene/SceneKey.cs    # 已实现
│   └── resources/TextureKey.cs  # 已实现
│
├── constants/               # 常量
│   ├── GameConstants.cs     # 已实现
│   └── UiLayers.cs          # 已实现
│
├── module/                  # DI 模块
│   ├── ModelModule.cs       # 已实现
│   ├── SystemModule.cs      # 已实现
│   ├── UtilityModule.cs     # 已实现
│   └── StateModule.cs       # 已实现
│
├── utility/                 # 工具
│   ├── GameUtil.cs          # 已实现
│   ├── GodotTextureRegistry.cs  # 已实现
│   └── storage/             # 存储接口
│
└── data/                    # 数据层
    └── setting/             # 设置数据（已实现）

tests/                       # 单元测试
├── TerrainTests.cs
├── PathFinderTests.cs
├── ShadowCasterTests.cs
├── LevelGeneratorTests.cs
├── CombatFormulaTests.cs
├── IntentInterpreterTests.cs
├── BehaviorTreeTests.cs
└── MobAITests.cs
```

---

## 5. 分阶段开发计划

### 阶段 0：基础设施（已完成 ✅）

| 任务 | 状态 | 提交 |
|---|---|---|
| 项目重命名 | ✅ | `e0d5235` |
| 素材复刻入库 | ✅ | `bea62c2` |
| GPLv3 许可声明 | ✅ | `75f0d82` |
| 原版架构知识库 | ✅ | `ac0a638` |
| GFramework 模板知识库 | ✅ | `a3753b2` |
| 架构设计文档 | ✅ | `d84f0fe` |

### 阶段 1：地牢引擎（预计 2-3 周）

**目标：生成一个可探索的程序化地牢**

| 周 | 任务 | 交付物 |
|---|---|---|
| 1 | Terrain + TileFlags + PathFinder + ShadowCaster | 纯 C# 类，单元测试通过 |
| 1 | Room 基类 + 5 种基础 StandardRoom | 纯 C# 类，单元测试通过 |
| 2 | Builder（LoopBuilder + FigureEightBuilder） | 纯 C# 类，可生成房间连接图 |
| 2 | Painter 基础 + LevelGenerator 管线 | 纯 C# 类，可生成完整地图数据 |
| 2 | TileMap 渲染器 | Godot 场景显示生成的地牢 |
| 3 | 种子系统 + FOV 渲染 | 可复现生成，迷雾渲染正确 |
| 3 | 10 种 StandardRoom + 5 种 SpecialRoom | 房间多样性 |

**验证：** `dotnet run` 看到地牢地图，可复现种子，FOV 正确

### 阶段 2：回合调度 + 角色（预计 2 周）

**目标：英雄在地牢中移动，回合制运行**

| 周 | 任务 | 交付物 |
|---|---|---|
| 1 | TurnSystem + Actor 时间线 | 回合调度事件驱动 |
| 1 | CharEntity + HeroEntity | 角色实体可移动 |
| 1 | IntentSystem（输入→意图→解释器→命令） | 点击移动、攻击 |
| 2 | Camera + 基础 UI（状态栏） | 跟随英雄，显示 HP/层数 |
| 2 | MobEntity + 简单 AI（Rat/Snake） | 怪物可移动、追击 |
| 2 | 基础战斗系统（攻击/伤害/死亡） | 可击杀怪物 |

**验证：** 英雄可在地牢中移动，遇到怪物可攻击，怪物会追击

### 阶段 3：物品 + 内容（预计 3-4 周）

**目标：拾取物品、装备、药水识别、升级**

| 周 | 任务 | 交付物 |
|---|---|---|
| 1 | ItemModel + Generator + HeapNode | 物品生成、地面拾取 |
| 1 | Backpack UI + 装备系统 | 背包界面、装备/卸下 |
| 2 | 药水体系（12 种）+ 药水颜色识别 | 药水使用、效果 |
| 2 | 卷轴体系（12 种）+ 卷轴名称识别 | 卷轴使用、效果 |
| 2 | 升级系统（SoU + 装备强化） | 装备升级、力量需求 |
| 3 | 食物/饥饿度/HP 恢复 | 饥饿系统、回血 |
| 3 | 法杖（5 种基础）+ 法杖充能 | 魔法攻击 |
| 3 | 戒指（5 种基础） | 被动属性加成 |
| 4 | 陷阱（10 种）+ 植物（5 种） | 环境交互 |

**验证：** 可拾取、使用药水/卷轴、升级装备，与陷阱交互

### 阶段 4：怪物 + BOSS（预计 3-4 周）

**目标：5 个区域 × 5 层，15 种怪物，BOSS 战**

| 周 | 任务 | 交付物 |
|---|---|---|
| 1 | 行为树引擎 + 简单 FSM | 怪物 AI 框架 |
| 1 | 下水道怪物（Rat/Snake/Slime/Crab/Gnoll） | 5 种怪物 |
| 1 | 监狱怪物（Skeleton/Thief/Guard/Bat） | 4 种怪物 |
| 2 | 洞穴怪物 + 城市怪物（8 种） | 8 种怪物 |
| 2 | 地狱怪物（6 种） | 6 种怪物 |
| 2 | Goo Boss 战 | 首个 BOSS |
| 2 | Tengu Boss 战 | 二阶段 BOSS |
| 3 | DM-300 Boss 战 + Dwarf King Boss 战 | 两个 BOSS |
| 3 | Yog-Dzewa Boss 战 | 最终 BOSS |
| 3 | 楼层切换 + 5 个区域 Painter | 完整 26 层生成 |
| 4 | 怪物重生系统 + 难度平衡 | 可玩性 |

**验证：** 可打通 26 层，打败 Yog-Dzewa

### 阶段 5：完整游戏（预计 3-4 周）

**目标：6 个职业、天赋树、存档、通关**

| 周 | 任务 | 交付物 |
|---|---|---|
| 1 | 存档系统（保存/读取/自动存档） | 游戏可存档 |
| 1 | 死亡/复活/游戏结束 | 完整游戏循环 |
| 2 | 6 个职业基础 | 职业差异化 |
| 2 | 天赋树（每个职业 8 个天赋） | 技能选择 |
| 3 | 6 个子职业 + 护甲技能 | 进阶选择 |
| 3 | 炼金术系统 | 配方合成 |
| 3 | NPC 任务（Ghost/Wandmaker/Blacksmith/Imp） | 支线任务 |
| 4 | 挑战模式 + 每日挑战 | 可重复性 |
| 4 | 平衡性调整 + Bug 修复 | 稳定版本 |

**验证：** 可从种子开始，选择职业，完整通关 26 层

### 阶段 6：战旗模板抽象（预计 2-3 周）

**目标：将通用 SRPG 逻辑抽出为独立模板**

| 周 | 任务 | 交付物 |
|---|---|---|
| 1 | 识别可复用模块（网格/回合/属性/行动） | 模块清单 |
| 1 | 将纯 C# 层抽离为独立项目 | 独立 NuGet 包 |
| 2 | 将通用 UI 抽离为模板 | 模板代码 |
| 2 | 编写文档 + 示例项目 | 使用说明 |
| 3 | 示例战旗游戏（简单 Demo） | 验证模板可用性 |

**验证：** 可用模板在 1 小时内搭起一个简单的战旗游戏原型

---

## 6. 架构设计决策

### 6.1 回合调度

```
原版: Actor 独立线程 + 时间轴 + 优先级
我们: TurnSystem (ISystem) + 事件驱动 + 整回合调度
```

**为什么选事件驱动而不是独立线程：**
- GFramework 没有线程模型，独立线程与架构上下文冲突
- 事件驱动更简单，不需要处理线程同步
- Godot 的 Node 生命周期不是线程安全的

**折中方案：** 保留时间轴概念（`Actor.time`），但用事件驱动替代线程阻塞：

```
TurnSystem.ProcessTurn()
  → 广播 TurnStartedEvent
  → 找出时间最早的 Actor → 广播 CharActRequestEvent
  → 如果是英雄且无输入 → 等待 IntentCommand
  → 如果是怪物 → 执行 Mob AI → 广播 ActionPerformedEvent
  → 处理 Buff → 广播 BuffProcessedEvent
  → 处理 Blob → 广播 BlobProcessedEvent
  → 广播 TurnEndedEvent
  → FixTime() 调整时间轴
```

### 6.2 地牢生成

```
纯 C# 层（scripts/dungeon/）:
  LevelGenerator.Generate(depth, seed) → DungeonData
    1. 计算种子
    2. 创建房间池
    3. Builder 连接
    4. Painter 绘制
    5. 构建标志数组
    6. 生成怪物/物品
    7. 返回 DungeonData

Godot 层:
  DungeonLevel (Node2D) 加载 DungeonData
    1. 创建 TileMap 渲染
    2. 创建怪物实体
    3. 创建物品堆
    4. 初始化 FOV
```

### 6.3 指令模式

```
输入 → Intent → IntentInterpreter → Command[] → Execute
```

- `Intent`：纯数据，记录玩家"想做什么"
- `IntentInterpreter`：纯逻辑，根据上下文解析意图为命令序列
- `Command`：GFramework `AbstractCommand`，执行实际逻辑

### 6.4 怪物 AI

```
MobBrain
  ├── 顶层 FSM: Sleep → Patrol → Chase → Combat → Flee
  └── 行为树: Selector/Sequence/Condition/Action 组合
```

- 简单怪物（Rat, Snake）只用 FSM + 简单行为树
- 复杂 Boss（Tengu, DwarfKing）用完整行为树
- 行为树节点可复用，纯 C# 实现

### 6.5 事件频段

```csharp
// 扩展 GFramework EventBus，不修改框架源码
RegisterEvent<T>(handler, band)  // 订阅特定频段
SendEvent<T>(e, band)            // 发送到特定频段
```

---

## 7. 测试策略

### 测试金字塔

```
        ╱╲
       ╱ E2E ╲
      ╱────────╲
     ╱ 集成测试  ╲
    ╱──────────────╲
   ╱  单元测试（核心） ╲
  ╱────────────────────╲
```

### 纯 C# 层（可单测，无需 Godot）

| 测试模块 | 测试内容 | 文件 |
|---|---|---|
| Terrain | 标志位正确性、发现方法 | `TerrainTests.cs` |
| PathFinder | 寻路正确性、边界情况 | `PathFinderTests.cs` |
| ShadowCaster | FOV 正确性、性能 | `ShadowCasterTests.cs` |
| LevelGenerator | 种子可复现、房间连通性、地形完整性 | `LevelGeneratorTests.cs` |
| Room | 大小设置、邻居连接、门类型 | `RoomTests.cs` |
| Builder | 房间连接算法正确性 | `BuilderTests.cs` |
| CombatFormula | 命中/伤害/防御公式正确性 | `CombatFormulaTests.cs` |
| IntentInterpreter | 意图→命令序列正确性 | `IntentInterpreterTests.cs` |
| BehaviorTree | 节点执行逻辑、组合逻辑 | `BehaviorTreeTests.cs` |
| MobAI | 简单怪物决策逻辑 | `MobAITests.cs` |

### Godot 层（集成测试）

| 测试模块 | 测试内容 |
|---|---|
| TurnSystem | 回合调度事件流 |
| CharEntity | 移动/攻击/伤害流程 |
| ItemSystem | 拾取/装备/使用流程 |
| UI 页面 | 页面渲染、交互 |

---

## 8. 风险与对策

| 风险 | 概率 | 影响 | 对策 |
|---|---|---|---|
| GFramework 版本不稳定 | 低 | 中 | 锁定版本 0.0.177，不追新 |
| Godot 4.6 C# 兼容性问题 | 低 | 中 | 使用稳定版，紧跟官方更新 |
| 行为树性能 (60+ 怪物) | 中 | 低 | 懒评估、节点池、分层调度 |
| 原版 40+ 房间变体实现时间 | 高 | 中 | MVP 只做 20 种，后续按需添加 |
| 6 个职业 + 天赋平衡 | 中 | 高 | 先做 Warrior 和 Mage，其他后续 |
| 存档格式变更 | 低 | 中 | 设计时预留版本字段，支持迁移 |
| 素材合规（GPLv3） | 低 | 低 | 已建立双许可结构，清晰声明 |

---

## 9. 战旗模板抽象目标

### 可复用模块识别

从游戏逻辑中识别出与"破碎的像素地牢"无关的通用 SRPG 模块：

| 模块 | 通用性 | 应用场景 |
|---|---|---|
| 网格系统 | ⭐⭐⭐⭐⭐ | 任何网格游戏 |
| 回合调度 | ⭐⭐⭐⭐⭐ | 任何回合制游戏 |
| 路径寻路 | ⭐⭐⭐⭐⭐ | 任何网格游戏 |
| FOV 系统 | ⭐⭐⭐⭐ | 需要视野的游戏 |
| 指令模式 | ⭐⭐⭐⭐⭐ | 任何需要输入驱动的游戏 |
| 行为树 | ⭐⭐⭐⭐ | 任何需要 AI 的游戏 |
| 属性系统 | ⭐⭐⭐⭐⭐ | 任何 RPG |
| 背包系统 | ⭐⭐⭐⭐ | 任何 RPG |
| 事件频段 | ⭐⭐⭐⭐⭐ | 任何使用事件总线的项目 |
| 通用 UI | ⭐⭐⭐ | 有 UI 的项目 |

### 抽象策略

```
项目代码 (My-Shattered-Pixel-Dungeon)
  └── 游戏逻辑（依赖 GFramework + 模板）
      ├── 地牢特定：LevelGenerator, Room 变体, Painter, 怪物行为
      └── 通用 SRPG：GridSystem, TurnSystem, IntentSystem, BehaviorTree, PropertySystem
          └── 抽离为独立模板项目
              └── 依赖 GFramework
```

### 输出格式

| 输出 | 格式 | 说明 |
|---|---|---|
| 通用代码库 | `GeWuYou.SrpgTemplate`（NuGet） | 可选，NuGet 发布 |
| 模板代码目录 | `scripts/template/` | 项目内模板目录 |
| 示例项目 | 简单战旗 Demo | 验证模板可用性 |