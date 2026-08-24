# 08 - UI 系统

> 核心文件：`GameScene.java`, `scenes/`, `ui/`, `windows/`, `tiles/`

## 场景层级

```
libGDX Game
  └── PixelScene (所有场景基类)
      ├── WelcomeScene    — 欢迎页
      ├── TitleScene      — 标题
      ├── StartScene      — 开始菜单
      ├── HeroSelectScene — 选职业
      ├── GameScene       — 主游戏 ⭐
      ├── InterlevelScene — 楼层过渡
      ├── AlchemyScene    — 炼金
      ├── JournalScene    — 日志/图鉴
      ├── RankingsScene   — 排行榜
      └── 其他
```

## GameScene（主游戏场景）

```
GameScene:
  ├── DungeonTilemap     — 地形渲染
  ├── DungeonWallsTilemap — 墙渲染
  ├── DungeonTerrainTilemap — 地形特性
  ├── FogOfWar           — 战争迷雾
  ├── CharLayer          — 角色渲染
  ├── HeapLayer          — 物品堆渲染
  ├── StatusPane         — 英雄状态栏
  ├── Toolbar            — 动作工具栏
  ├── AttackIndicator    — 攻击指示器
  ├── BuffIndicator      — Buff 图标
  ├── GameLog            — 消息日志
  ├── Compass            — 指南针
  └── CellSelector       — 单元格选择

  输入处理:
    onMouseClick → CellSelector.onSelect(cell)
    onKeyDown → 处理 SPDAction
    ready() → 等待玩家输入（Actor 线程唤醒）
```

## UI 组件清单（ui/）

| 组件 | 职责 |
|---|---|
| StatusPane | 英雄状态栏（HP/饥饿/等级/层数/金币） |
| Toolbar | 动作工具栏（攻击/等待/搜索/背包/快捷槽） |
| AttackIndicator | 攻击目标指示 |
| BuffIndicator | Buff 图标行 |
| GameLog | 消息日志 |
| InventoryPane | 背包面板（桌面版） |
| InventorySlot | 背包格子 |
| ItemSlot | 物品图标槽 |
| QuickSlotButton | 快捷槽按钮 |
| Compass | 指南针（指向出口） |
| LootIndicator | 战利品指示 |
| DangerIndicator | 危险指示 |
| ResumeIndicator | 恢复指示 |
| BossHealthBar | BOSS 血条 |
| HealthBar | 血条 |
| CharHealthIndicator | 角色血条指示 |
| TargetHealthIndicator | 目标血条 |
| ActionIndicator | 行动指示器 |
| HeroIcon | 英雄头像 |
| IconButton | 图标按钮 |
| RedButton | 红色按钮 |
| StyledButton | 样式按钮 |
| CheckBox | 复选框 |
| OptionSlider | 滑条 |
| ScrollingListPane | 滚动列表 |
| ScrollingGridPane | 滚动网格 |
| ScrollPane | 滚动面板 |
| Window | 弹窗基类 |
| Toast | 提示消息 |
| Tooltip | 悬停提示 |
| KeyDisplay | 按键显示 |
| Banner | 横幅（楼层标题） |
| BadgesList/Grid | 徽章列表 |
| TalentsPane | 天赋面板 |
| TalentButton | 天赋按钮 |
| TalentIcon | 天赋图标 |
| TitleBackground | 标题背景 |
| BusyIndicator | 忙碌指示 |
| CurrencyIndicator | 货币指示（金币/能量） |
| CustomNoteButton | 自定义笔记按钮 |
| ItemJournalButton | 物品日志按钮 |
| MenuPane | 菜单面板 |
| RadialMenu | 径向菜单 |
| RightClickMenu | 右键菜单 |

## 窗口系统（windows/）

| 窗口 | 触发场景 |
|---|---|
| WndBag | 背包（按类别筛选） |
| WndHero | 英雄面板（属性/天赋） |
| WndHeroInfo | 英雄详情 |
| WndSettings | 设置 |
| WndGame | 游戏菜单（暂停） |
| WndOptions | 选项 |
| WndOptionsCondensed | 压缩选项 |
| WndMessage | 消息 |
| WndError | 错误 |
| WndStory | 剧情 |
| WndQuest | 任务 |
| WndList | 列表 |
| WndTabbed | 标签页 |
| WndTitledMessage | 标题消息 |
| WndTextInput | 文本输入 |
| WndResurrect | 复活（Ankh） |
| WndRanking | 排行榜 |
| WndScoreBreakdown | 得分明细 |
| WndGameInProgress | 游戏进行中 |
| WndChallenges | 挑战模式 |
| WndKeyBindings | 按键绑定 |
| WndInfoItem | 物品信息 |
| WndInfoCell | 格子信息 |
| WndInfoMob | 怪物信息 |
| WndInfoBuff | Buff 信息 |
| WndInfoTrap | 陷阱信息 |
| WndInfoPlant | 植物信息 |
| WndInfoSubclass | 子职业信息 |
| WndInfoTalent | 天赋信息 |
| WndInfoArmorAbility | 护甲技能信息 |
| WndUseItem | 使用物品 |
| WndTradeItem | 交易物品 |
| WndUpgrade | 升级（铁匠） |
| WndBlacksmith | 铁匠任务 |
| WndImp | 矮人任务 |
| WndSadGhost | 幽灵任务 |
| WndWandmaker | 法杖匠任务 |
| WndChooseSubclass | 选择子职业 |
| WndChooseAbility | 选择技能 |
| WndJournal | 日志 |
| WndJournalItem | 日志条目 |
| WndDocument | 文档 |
| WndQuickBag | 快捷背包 |
| WndDailies | 每日挑战 |
| WndHardNotification | 强提醒 |
| WndVictoryCongrats | 胜利祝贺 |
| WndSupportPrompt | 支持提示 |
| WndCombo | 连击 |
| WndMonkAbilities | 和尚技能 |
| WndClericSpells | 牧师法术 |
| WndEnergizeItem | 充能物品 |
| WndInfoSubclass | 子职业 |
| WndBadge | 徽章 |
| WndKeyBindings | 按键 |
| WndScoreBreakdown | 得分 |

## 瓦片渲染（tiles/）

| 类 | 职责 |
|---|---|
| DungeonTilemap | 地形瓦片渲染（选择正确瓦片图） |
| DungeonWallsTilemap | 墙壁遮挡渲染 |
| DungeonTerrainTilemap | 地形特性（高草/陷阱等） |
| DungeonTileSheet | 瓦片图集定义（16x16 网格布局） |
| FogOfWar | 战争迷雾（可见/已探索/未探索） |
| GridTileMap | 网格显示 |
| WallBlockingTilemap | 墙壁阻挡层 |
| RaisedTerrainTilemap | 高地形渲染 |
| CustomTilemap | 自定义瓦片接口 |
| TerrainFeaturesTilemap | 地形特性渲染 |

## 渲染流程

```
1. DungeonTilemap 渲染基础地形
2. DungeonWallsTilemap 渲染墙（带高度感）
3. TerrainFeaturesTilemap 渲染高草/陷阱
4. CharLayer 渲染角色精灵
5. HeapLayer 渲染物品堆
6. FogOfWar 覆盖迷雾
7. UI 层（StatusPane/Toolbar 等）
```

## 像素字体与渲染

- 基础分辨率 960x540（2x 像素）
- 使用 `pixel_font.ttf` 或 `pixel_font.png` 位图字体
- 所有 UI 元素使用 9-patch（chrome.png）
- 渲染使用 libGDX Scene2D 自绘（非场景图）