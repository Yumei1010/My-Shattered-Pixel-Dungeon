# 项目记忆

## 目标
- **全量复刻《破碎的像素地牢》(Shattered Pixel Dungeon)**
- **抽离出一套可复用的战旗游戏模板**（基于 Godot 4.6 + C# .NET 10 + GFramework 0.0.177）

## 当前状态
项目处于初始提交（d0aa88f），当前为 GFramework 空模板（My-Shattered-Pixel-Dungeon），尚未开始游戏逻辑开发。

### 已实现的基础架构
- **GFramework 架构**：`GameArchitecture` 安装 4 个模块（UtilityModule → SystemModule → ModelModule → StateModule）
- **DI/自动加载单例**：`GameEntryPoint` 作为 Godot 自动加载
- **状态机**：`AppState`（空状态，仅清除 UI/场景）
- **UI 路由**：`UiRouter`、`UiFactory`、`ISimpleUiPage`、`IUiPageBehaviorProvider`
- **场景路由**：`SceneRouter`
- **CQRS 基础设施**：
  - `audio/` — 音量控制命令（Master/Bgm/Sfx）+ `VolumeChangedEvent`
  - `graphics/` — 分辨率/全屏切换命令
  - `setting/` — 设置保存/重置/查询 + 语言切换
  - `game/` — `ExitGameCommand`
- **页面示例**：`scripts/menu/TemplatePage`（partial class 模式：核心 + Dependencies + Properties + Events + Signals）
- **组件**：`VolumeContainer`
- **资源注册**：纹理/场景/UI 页面配置注册表
- **存储**：读写存储工具、设置数据位置提供者
- **全局常量**、**UI 层级**、**枚举**（UiKey/SceneKey/TextureKey/InputPhase）
- **测试**：`TemplateSmokeTests.cs`

### 目录结构
```
scripts/
├── component/       # 可复用组件
├── constants/       # 全局常量
├── core/            # 架构、路由、状态、UI 基类
├── cqrs/            # CQRS 命令/事件/查询
├── data/            # 数据层
├── enums/           # 枚举
├── menu/            # TemplatePage（示例页面）
├── model/           # 领域模型
├── module/          # DI 模块
├── system/          # 系统
└── utility/         # 工具类
```

### 关键文件
- `global/GameEntryPoint.cs` — 自动加载入口
- `scripts/core/GameArchitecture.cs` — 架构安装
- `scripts/core/state/impls/AppState.cs` — 应用状态
- `scripts/menu/TemplatePage.cs` — 页面模板（参考实现）
- `project.godot` — 项目配置（960x540, GL Compatibility, C# 4.6）

## 已完成
- GFramework 模板安装完毕
- 项目初始提交

## 待办（按优先级）
1. **重命名项目**：`My-Shattered-Pixel-Dungeon` → `My-Shattered-Pixel-Dungeon`（.csproj, 命名空间, global usings, 目录）
2. **游戏核心系统**：地牢生成（房间/走廊/门）、回合制战斗、物品系统、怪物 AI、FOV/照明
3. **UI**：主菜单、游戏 HUD、背包、状态面板、地牢地图
4. **破碎的像素地牢特色**：升级系统、附魔、药水/卷轴识别、饥饿度、陷阱
5. **战旗模板抽象**：将通用 SRPG 逻辑（网格/回合/行动/属性）抽离为模板

## 已完成 (completed)
- 项目重命名完成：My-GFramework-Godot-Template → My-Shattered-Pixel-Dungeon
- 根命名空间: GFrameworkTemplate → MyShatteredPixelDungeon
- .csproj / .sln / 测试项目文件已重命名
- project.godot / release.yml / README / CONVENTIONS 已更新
- 构建通过，测试通过

## 备注 (notes)
- ## 原版 Shattered Pixel Dungeon 架构分析
- ### 核心架构（libGDX, Java）
- ShatteredPixelDungeon** (extends Game) — 主入口，场景切换
- PixelScene** — 所有场景的基类（GameScene, TitleScene, StartScene, WelcomeScene 等）
- SPDAction** — 输入动作系统，键盘/手柄绑定
- SPDSettings** — 设置持久化
- ### 回合系统（Actor）
- Actor** — 抽象基类，含 `act()` 方法，基于时间的优先级调度
- TICK = 1f（基本时间单位）
- 优先级链：VFX(100) > HERO(0) > BLOB(-10) > MOB(-20) > BUFF(-30) > DEFAULT(-100)
- `process()` — 主循环，在独立线程中运行
- `add()` / `remove()` / `init()` / `clear()` / `fixTime()`
- Char** extends Actor — 角色基类（HP, STR, 移动, 攻击, buff 系统）
- Buff** extends Actor — 计时效果（饥饿, 中毒, 燃烧, 麻痹 等）
- Blob** extends Actor — 区域效果（毒气, 蛛网, 电击 等）
- ### 地牢生成（Level）
- Level** — 抽象基类，持有 map[], rooms, mobs, heaps, traps, plants, blobs
- `create()` — 种子驱动的生成流程
- `build()` — 由子类实现（RegularLevel / BossLevel）
- `buildFlagMaps()` — 构建 passable/losBlocking 等标志数组
- RegularLevel** — 标准程序化地牢
- `initRooms()` — 创建入口/出口/标准/特殊/秘密房间池
- Builder 模式（LoopBuilder, FigureEightBuilder, BranchesBuilder, LineBuilder, GridBuilder）
- Painter 模式（SewerPainter, PrisonPainter, CavesPainter, CityPainter, HallsPainter）
- Room** — 矩形房间带邻居/连接图
- StandardRoom（40+ 变体：Empty, Ring, CircleBasin, SewerPipe 等）
- SpecialRoom（Shop, Sacrifice, Statue, Pit 等）
- SecretRoom（隐藏房间）
- EntranceRoom / ExitRoom（入口/出口，各有 20+ 变体）
- Terrain** — 地形常量和标志（PASSABLE, LOS_BLOCKING, FLAMABLE, SOLID, AVOID, LIQUID, PIT）
- Traps** — 陷阱系统（20+ 种）
- Patch** — 区域生成辅助工具
- ### 物品系统
- Item** — 基类（stackable, level, cursed, actions）
- Heap** — 地面物品堆
- Generator** — 物品生成，按 Category 分类和概率控制
- Generator.Category** — 分类枚举
- 子类体系：Weapon→MeleeWeapon/MissileWeapon, Armor, Wand, Ring, Potion, Scroll, Artifact, Spell, Bomb, Food, Trinket 等
- ### 物理/力学
- ShadowCaster** — 递归阴影投射 FOV 算法（基于 roguebasin）
- Ballistica** — 弹道轨迹计算
- ConeAOE** — 锥形范围效果
- PathFinder** — A* 寻路
- ### UI 系统
- GameScene** — 主游戏场景，处理输入、FOV 更新、UI 层
- CellSelector** — 单元格选择
- StatusPane** — 英雄状态（HP, 饥饿, 等级, 层数, 金币）
- Toolbar** — 动作按钮（攻击/等待/搜索/背包）
- QuickSlotButton** — 快捷槽
- BuffIndicator** — Buff 图标
- GameLog** — 消息日志
- InventoryPane** / **InventorySlot** — 背包 UI
- Window** / **WndBag** / **WndHero** / **WndSettings** 等 — 窗口系统
- ### 数据流
- 1. 游戏开始 → Dungeon.init() → Dungeon.newLevel() → Level.create() → Actor 循环
- 2. 玩家输入 → GameScene → Hero 动作 → Hero.act() → Actor.process()
- 3. 楼层切换 → InterlevelScene → Dungeon.switchLevel() → Actor.init()
- 4. 存档 → Dungeon.saveAll() → saveGame() + saveLevel()
- 5. 种子系统 → 每层种子 seedForDepth(depth, branch) → 确定性地板生成

## 已完成 (completed)
- 全量复刻原版素材（452 文件，38MB）到 assets/（sprites/environment/interfaces/effects/music/sounds/splashes/fonts/messages）
- 添加 GPLv3 许可声明：assets/GPL-3.0-LICENSE.txt + assets/README.md
- README 更新：项目定位（复刻+战旗模板）、双许可证说明（源码 Apache 2.0 / 素材 GPLv3）
