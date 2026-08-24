# 01 - 整体架构

> 核心文件：`ShatteredPixelDungeon.java`, `Dungeon.java`, `SPDAction.java`, `SPDSettings.java`

## 游戏入口

```java
ShatteredPixelDungeon extends Game (libGDX)
  ├── create()  → 初始化 UI、按键绑定、音频
  ├── scene 切换 → switchScene(c) / switchNoFade(c)
  └── destroy() → GameScene.endActorThread()
```

### 场景切换

```java
ShatteredPixelDungeon.switchScene(Class<? extends PixelScene>)
  ├── 保存当前场景的窗口状态 (saveWindows)
  ├── 切换到新场景
  └── 恢复窗口状态 (restoreWindows)
```

**场景列表：**
- `WelcomeScene` — 欢迎页（首次启动）
- `TitleScene` — 标题画面
- `StartScene` — 新游戏/继续
- `HeroSelectScene` — 选职业
- `GameScene` — 主游戏场景（核心）
- `InterlevelScene` — 楼层切换过渡
- `AlchemyScene` — 炼金术场景
- `JournalScene` — 日志/图鉴
- `RankingsScene` — 排行榜
- `ChangesScene` — 更新日志
- `AboutScene` — 关于
- `SupporterScene` — 支持者
- `SurfaceScene` — 返回地面（胜利）
- `AmuletScene` — 获得护符

## 全局状态（Dungeon 静态类）

```java
Dungeon:
  ├── hero         → Hero 实例
  ├── level        → Level 实例（当前层）
  ├── depth        → 当前层数 (1-26)
  ├── branch       → 分支 (0=主线, 1=支线)
  ├── seed         → 全局种子 (long)
  ├── gold         → 金币
  ├── energy       → 能量
  ├── quickslot    → 快捷槽
  ├── challenges   → 挑战模式掩码
  ├── droppedItems → 掉落深渊的物品 (按层索引)
  ├── generatedLevels → 已生成层列表
  ├── LimitedDrops → 限次掉落追踪
  ├── chapters     → 到达过的章节（触发区域标题）
  └── version      → 游戏版本
```

**初始化流程：**
```
Dungeon.init()
  → 重置种子、Actor、随机数
  → 初始化 Scroll/Potion/Ring 的随机标签
  → 初始化 SpecialRoom/SecretRoom
  → 重置 Generator、Statistics、Notes
  → 创建 Hero 并初始化职业
```

**楼层切换：**
```
Dungeon.newLevel()  → 根据 depth 创建对应 Level 子类
Dungeon.switchLevel(level, pos)  → 放置英雄、初始化 Actor、更新 FOV
```

## 输入系统

```java
SPDAction extends GameAction:
  ├── 方向键: N, S, W, E, NW, NE, SW, SE
  ├── 功能: INVENTORY, WAIT, REST, EXAMINE
  ├── 标签: TAG_ATTACK, TAG_ACTION, TAG_LOOT, TAG_RESUME
  ├── 快捷: QUICKSLOT_1~6, BAG_1~5
  └── 其他: CYCLE, HERO_INFO, JOURNAL, ZOOM_IN, ZOOM_OUT

默认绑定:
  WASD / 方向键 → 移动
  SPACE / Num5 → 等待/拾取
  F / I → 背包
  E → 检查
  Z → 休息
  Q → 攻击标签
  Tab → 循环切换
  X → 动作标签
  C → 拾取标签
  R → 恢复标签
  H → 英雄信息
  J → 日志
  Enter → 拾取/确认
```

## 存档系统

```java
Dungeon.saveGame(save)  → 保存游戏状态到 Bundle → 文件
Dungeon.saveLevel(save) → 保存当前楼层地图到单独文件
Dungeon.saveAll()       → 同时保存游戏+楼层
Dungeon.loadGame(save)  → 读取游戏状态
Dungeon.loadLevel(save) → 读取楼层地图
```

**Bundle 序列化：** 自定义 Key-Value 序列化系统，支持嵌套 Bundle、数组、枚举反射

**存档位置：** `GamesInProgress.gameFile(save)` / `GamesInProgress.depthFile(save, depth, branch)`