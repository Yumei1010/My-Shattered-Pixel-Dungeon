# 项目记忆

## 目标
- 全量复刻《破碎的像素地牢》(Shattered Pixel Dungeon)
- 抽离出一套可复用的战旗游戏模板（基于 Godot 4.6 + C# .NET 10 + GFramework 0.0.177）

## 当前进度

### 阶段 0：基础设施 ✅
- 项目重命名、素材复刻、GPLv3 许可、知识库文档
- 原版架构分析（12 篇知识库文档）
- GFramework 模板分析、架构设计进阶（事件频段/指令模式/行为树）
- 完整开发规划文档

### 阶段 1：地牢引擎 ✅（27 文件，78 测试）
- `scripts/dungeon/` — 纯 C# 层（可单测）
  - Terrain.cs + TileFlags.cs — 38 种地形常量 + 8 种位标志
  - BArray.cs — boolean 数组工具
  - PathFinder.cs — BFS 距离地图寻路
  - ShadowCaster.cs — 递归阴影投射 FOV
  - GameMath.cs / Point.cs / PointF.cs / Rect.cs — 几何工具
  - Painter.cs — 地形绘制工具（Fill/DrawLine/Ellipse）
  - DungeonData.cs — 地牢生成数据容器
  - LevelGenerator.cs — 完整生成管线（种子→房间→连接→绘制→标志）
  - DeterministicRng.cs — 确定性随机数（基于 System.Random）
  - rooms/ — Room 基类 + 7 种房间（含 Entrance/Exit） + ConnectionRoom + SpecialRoom
  - builders/ — Builder 基类 + RegularBuilder + LoopBuilder（环状连接）

### 阶段 2：回合调度 + 角色系统 + UI ✅
- **TurnSystem** — 回合调度（CQRS 事件驱动）
- **Actor** — 基类（时间/优先级/ID/生命周期）
- **CharEntity** — 角色实体（HP/STR/移动/战斗/Buff容器/抵抗免疫）
- **HeroEntity** — 英雄实体（动作/输入等待）
- **MobEntity** — 怪物 AI（6 状态：Sleep/Wander/Hunt/Investigate/Flee/Passive）
- **Buff** — 基类 + 4 种 Buff（Hunger/Paralysis/Terror/Dread）
- **Rat** — 怪物示例
- **IntentSystem** — 指令模式（Intent→Interpreter→Command）
- **GameplayInputController** — 输入处理
- **GameCamera** — 相机跟随 + 滚轮缩放
- **GameLog** — 消息日志（RichTextLabel BBCode）
- **StatusPane** — 状态栏（HP/层数/金币）
- **Toolbar** — 动作按钮（攻击/等待/休息/背包）
- **CQRS 事件** — 8 个独立文件（combat + movement + intent）

### 用户已拼好的场景
- `scenes/ui/game_log/game_log.tscn`
- `scenes/ui/status_pane/status_pane.tscn`
- `scenes/ui/tool_bar/tool_bar.tscn`

### GFramework 扩展
- 已安装 `pi-router` 扩展（故障转移）
- 备用 API key 已配置到 auth.json

## 关键提交
```
83f71c0  feat(ui): 添加 HUD 场景文件
20b7c01  feat(ui): 添加 HUD 基础 UI 组件
23aebc9  feat(engine): 添加 IntentSystem 指令模式
7b742bc  feat(engine): 添加 MobEntity 怪物 AI
9f6f45e  feat(engine): 添加 Buff 系统
fafd549  refactor(dungeon): 清理代码质量问题
```

## 待办（阶段 3）
- 物品系统（ItemModel/Generator/HeapNode）
- 背包 UI
- 药水/卷轴体系
- 战斗系统完善
- 装备系统

## 关键文件
- `docs/development-plan.md` — 完整项目规划
- `docs/knowledge/` — 12 篇知识库文档
- `scripts/dungeon/` — 地牢引擎（纯 C# 可单测）
- `scripts/systems/TurnSystem.cs` — 回合调度
- `scripts/systems/IntentSystem.cs` — 指令模式
- `scripts/entities/` — Actor/CharEntity/HeroEntity/MobEntity/Buff

## 当前任务 (progress)
- 阶段 3 物品系统：基础框架搭建完成
- Item.cs: 物品基类（属性/方法/序列化）
- ItemContainer.cs: 背包容器（堆叠/容量/查找）
- EquipableItem.cs: 可装备物品基类
- Weapon.cs: 武器体系（近战/远程/飞镖）
- Armor.cs: 护甲体系（布/皮/锁/鳞/板 5 种）
- Wand.cs: 法杖基类 + 13 种法杖占位
- Ring.cs: 戒指基类 + 12 种戒指占位
- Potion.cs: 药水基类 + 12 种药水（含效果占位）
- Scroll.cs: 卷轴基类 + 12 种卷轴（含效果占位）
- Food.cs: 食物基类 + 7 种食物
- HeroEntity.cs: 新增 Inventory/装备/金币/属性修正
- InventorySystem.cs: 背包系统（拾取/丢弃/使用/装备）
- Generator.cs: 物品生成器（11 类别 + 概率权重）
- HeapNode.cs: 地面物品堆（Godot Node2D）
- 6 个 CQRS 事件 + 4 个 CQRS 命令
- ItemModel.cs: 物品数据模型（鉴定/识别状态）
- 构建通过，78 测试通过

## 备注 (notes)
- 阶段 3 物品系统基础框架已搭建完成，构建通过，78 测试通过
- 命名空间规范：Food/Potion/Scroll/Ring/Wand 使用 items.{type} 子命名空间
- HeapNode 已标注 [Log][ContextAware]
- inventory_page 目录保持 snake_case 规范
- 下一步：实现药水/卷轴实际效果，添加背包 UI 页面

## 当前任务 (progress)
- 背包 UI 页面 InventoryPage 完成（partial class 五文件模式）
- 物品列表展示（名称/数量/装备标记）
- 物品详情面板（RichTextLabel）
- 动作按钮（装备/卸下/使用/投掷）
- CQRS 事件订阅（InventoryChangedEvent 刷新列表）
- 场景文件 inventory_page.tscn
- 构建通过，78 测试通过

## 当前任务 (progress)
- 药水实际效果：治疗/活力/力量/经验/麻痹/净化 已实现（其余占位 TODO）
- 卷轴实际效果：鉴定/升级/移除诅咒/恐怖/复仇 已实现（其余占位 TODO）
- 构建通过，78 测试通过

## 当前任务 (progress)
- 药水颜色/卷轴名称随机识别系统完成（IdentificationSystem）
- 药水/卷轴显示名称（未鉴定显示颜色名/标签，已鉴定显示真名）
- 食物系统与饥饿度联动完成

## 当前任务 (progress)
- 新增 Buff: InvisibilityBuff/HasteBuff/FrostBuff
- HeroEntity.Speed() 受 HasteBuff(×2)/FrostBuff(×0.5) 影响
- 药水效果完善：隐身/火焰/冰冻/疾跑/净化/毒性瓦斯 等
- 卷轴效果完善：挑战/镜像/迷雾/觉醒/魔法映射/传送 等
- 添加 GroundItemManager 地面物品管理器（拾取/丢弃/自动拾取）
- 背包页面添加 Drop 丢弃动作
- 构建通过，78 测试通过

## 当前任务 (progress)
- 阶段 3 全部完成 ✅
- CombatSystem 战斗系统（攻击/伤害/死亡/掉落）
- Enchantment 13 种武器附魔
- Glyph 13 种护甲雕纹
- DemoPage 简易测试页面（生成物品/装备/背包/战斗/识别）
- 所有 .uid 文件对齐
- 构建通过，78 测试通过

## 当前任务 (progress)
- 可玩 Demo 场景完成 ✅
- DemoScene.cs: 地牢渲染(_Draw 免 TileSet)/英雄移动/战斗/拾取/怪物追击
- main.tscn 直接包含 DemoScene，IsDev 模式跳过状态机直接运行
- 操作: WASD 移动 | 空格等待 | H 喝药水 | R 重置
- GameState/AppState 状态机已注册但 Dev 模式不触发
- 构建通过，78 测试通过
