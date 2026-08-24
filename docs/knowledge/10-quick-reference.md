# 10 - 快速参考

> 开发实现时随手查阅

## 地形常量索引

| ID | 常量 | 标志 | 说明 |
|---|---|---|---|
| 0 | CHASM | AVOID+PIT | 深渊 |
| 1 | EMPTY | PASSABLE | 空地 |
| 2 | GRASS | PASSABLE+FLAMABLE | 草地 |
| 4 | WALL | LOS_BLOCKING+SOLID | 墙壁 |
| 5 | DOOR | PASSABLE+LOS_BLOCKING+FLAMABLE+SOLID | 关着的门 |
| 6 | OPEN_DOOR | PASSABLE+FLAMABLE | 开着的门 |
| 7 | ENTRANCE | PASSABLE | 入口楼梯 |
| 8 | EXIT | PASSABLE | 出口楼梯 |
| 15 | HIGH_GRASS | PASSABLE+LOS_BLOCKING+FLAMABLE | 高草 |
| 18 | TRAP | AVOID | 可见陷阱 |
| 29 | WATER | PASSABLE+LIQUID | 水 |

## 关键数值

```
Actor.TICK = 1f
Char.baseSpeed = 1
Hero.MAX_LEVEL = 30
Hero.STARTING_STR = 10
Level.TIME_TO_RESPAWN = 50
Level.viewDistance = 8 (darkness 挑战 = 2)
ShadowCaster.MAX_DISTANCE = 20
```

## 5 个区域

| 区域 | 层数 | BOSS | 瓦片资源 |
|---|---|---|---|
| 下水道 (Sewers) | 1-5 | Goo | tiles_sewers.png |
| 监狱 (Prison) | 6-10 | Tengu | tiles_prison.png |
| 洞穴 (Caves) | 11-15 | DM-300 | tiles_caves.png |
| 城市 (City) | 16-20 | Dwarf King | tiles_city.png |
| 地狱 (Halls) | 21-25 | Yog-Dzewa | tiles_halls.png |

## 6 个职业

| 职业 | HP | STR | 天赋池 | 特殊能力 |
|---|---|---|---|---|
| Warrior | 30 | 11 | 8 | 升级护甲 |
| Mage | 20 | 10 | 8 | 法杖充能 |
| Rogue | 25 | 10 | 8 | 隐身斗篷 |
| Huntress | 22 | 10 | 8 | 精神弓 |
| Duelist | 25 | 11 | 8 | 武器技能 |
| Cleric | 22 | 10 | 8 | 圣书法术 |

## 物品生成概率

```
Generator.Category.chance:
  POTION     = 0.50
  SCROLL     = 0.45
  WEAPON     = 0.25
  ARMOR      = 0.20
  FOOD       = 0.15
  GOLD       = 0.70
  WAND       = 0.08
  RING       = 0.07
  ARTIFACT   = 0.05
  MISSILE    = 0.35
  SEED       = 0.30
  BOMB       = 0.10
  STONE      = 0.12
  TRINKET    = 0.05
  DART       = 0.20
  SPELL      = 0.10
```

## 限次掉落

```
LimitedDrops:
  STRENGTH_POTIONS: 2/层组 (每 5 层 2 瓶)
  UPGRADE_SCROLLS:  3/层组 (每 5 层 3 张)
  ARCANE_STYLI:     1/层组
  ENCH_STONE:       1 个 (第 2 章后)
  INT_STONE:        1 个 (1-3 层)
  TRINKET_CATA:     1 个 (1-3 层)
  LAB_ROOM:         1 间/层组
```

## 战斗公式速查

```
命中判定:  Random.Float(acuStat) >= Random.Float(defStat)
  命中率 ≈ acuStat / (acuStat + defStat) * 50%

伤害计算:
  raw_damage = damageRoll()  // 武器伤害骰
  dr = Random.NormalIntRange(0, armorDR)
  final_damage = max(raw_damage - dr, 0)

Buff 时间:
  affect(target, clazz, duration)  // 不重复附加
  prolong(target, clazz, duration) // 延长（postpone）
  count(target, clazz, count)      // 计数器
```

## 怪物难度

```
EXP 上限 = max(hero.lvl, mob.maxLvl)  // 超过上限不获得经验
掉落上限 = mob.maxLvl + 2              // 超过不掉落物品
```

## 资源路径

原版资源在 `assets/` 目录下的对应关系：
```
原版路径                          → 本项目路径
core/src/main/assets/sprites/    → assets/sprites/
core/src/main/assets/environment/ → assets/environment/
core/src/main/assets/interfaces/ → assets/interfaces/
core/src/main/assets/effects/    → assets/effects/
core/src/main/assets/music/      → assets/music/
core/src/main/assets/sounds/     → assets/sounds/
core/src/main/assets/splashes/   → assets/splashes/
core/src/main/assets/fonts/      → assets/fonts/
core/src/main/assets/messages/   → assets/messages/
```

## 代码文件索引

```
核心文件                        行数  关键内容
Dungeon.java                  1093  全局状态、存档、种子
Level.java                    1657  地牢基类、FOV、交互
RegularLevel.java              909  标准地牢生成
Char.java                     1415  角色基类、战斗
Hero.java                     2637  英雄动作、天赋
Mob.java                      1511  怪物 AI 状态机
Item.java                      725  物品基类
Generator.java                 979  物品生成
GameScene.java                1865  主游戏场景
Actor.java                     400  回合调度
Terrain.java                    --  地形常量（小型）
ShadowCaster.java              166  FOV 算法
Ballistica.java                 --  弹道（小型）
ConeAOE.java                    --  锥形范围（小型）
```

## 本项目实现映射

```
原版概念              → 本项目的实现
Actor 回合调度       → CQRS 事件驱动 (ActorTickEvent)
Dungeon 全局状态     → DungeonModel (GFramework model)
Character              → CharEntity (GFramework entity)
Buff                   → BuffComponent
Item                   → ItemEntity
Level 生成            → LevelGenerator 纯类
GameScene 输入        → CQRS 命令 (MoveCommand, AttackCommand...)
Terrain               → Terrain 静态类 + [Flags] TileFlags
Bundle 存档           → GFramework serializer 替代
```