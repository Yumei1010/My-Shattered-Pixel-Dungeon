# 06 - 怪物 AI 系统

> 核心文件：`Mob.java`（1511 行）

## 6 种 AI 状态

```java
Mob.state = SLEEPING | HUNTING | INVESTIGATING | WANDERING | FLEEING | PASSIVE
```

### SLEEPING（睡眠）
```java
act():
  ├── 检查是否有负面状态 → 唤醒
  ├── 检查视野内是否有敌对单位:
  │   └── 检测概率 = 1 / (distance + stealth)
  │   └── 如果检测通过 → 唤醒 → HUNTING 或 WANDERING
  └── 否则: spend(TICK) 继续睡
```

### WANDERING（游荡）
```java
act():
  if (敌人可见 && 检测通过):
    → 进入 HUNTING
  else:
    → 继续游荡（随机走向目标位置）
    → 每步 spend(1/speed())
```

### HUNTING（狩猎）
```java
act():
  if (敌人可见 && 可攻击):
    → 攻击
  else:
    ├── 检查是否被其他攻击者吸引（recentlyAttackedBy）
    ├── 如果敌人不可见:
    │   ├── 显示失联图标
    │   └── 进入 WANDERING
    ├── 向目标寻路:
    │   ├── 成功: 移动一步
    │   └── 失败: 尝试换目标 → 还是不行 → WANDERING
    └── 每步 spend(1/speed())
```

### INVESTIGATING（调查）
```java
act():
  // 比 WANDERING 更激进：目标位置不变
  // 到达目标位置前一刻 → 进入 WANDERING
  // 检测概率同 WANDERING
```

### FLEEING（逃跑）
```java
act():
  // 受 Terror/Dread 影响
  if (逃脱判断: 1+Random.Int(distance) >= 6 且敌人不可见):
    → 逃脱成功（特殊逻辑）
  // 向远离目标的方向移动
  getFurther(target)
  // 无路可逃且没有恐惧效果 → 回头战斗
```

### PASSIVE（被动）
```java
act():
  // 什么都不做，只是 spend(TICK)
  // 用于 NPC 和某些特殊怪物
```

## 敌人选择（chooseEnemy）

```go
chooseEnemy():
  // 优先级: 恐惧源 > 镇静标记 > 可视敌人中最接近的
  ├── 先检查恐惧/威慑来源
  ├── 检查镇静石标记目标
  ├── 判断是否需要新目标:
  │   ├── 无目标/目标已死 → 需新目标
  │   ├── Amok 且目标是英雄 → 需新目标
  │   ├── 被魅惑且目标是魅惑者 → 需新目标
  │   └── 盟友且目标是盟友 → 需新目标
  └── 选择目标:
      ├── Amok: 敌方怪 > 友方怪 > 英雄
      ├── 盟友: 敌方怪（不选被动/睡眠/游荡的）
      └── 敌方: 友方怪 > 英雄（最近优先）
```

## 寻路（getCloser）

```java
getCloser(target):
  ├── 如果相邻且可通行 → 直接走过去
  ├── 否则维护一个 PathFinder.Path:
  │   ├── 检查当前路径是否有效: 空/断/低效 → 重新寻路
  │   ├── 新路径: Dungeon.findPath(this, target, passable, fov, chars)
  │   ├── HUNTING 状态: 如果绕路超过 2 倍，尝试忽略角色阻塞
  │   └── 取路径第一步
  ├── move(step)
  └── spend(1/speed())
```

## 68 种怪物（按区域分布）

| 区域 | 怪物 | BOSS |
|---|---|---|
| 下水道 (1-5) | Rat, Snake, Slime, Swarm, Crab, Gnoll, GnollGuard | Goo |
| 监狱 (6-10) | Skeleton, Thief, GnollTrickster, GnollSapper, DM100, Bat, Guard, Necromancer, GnollGeomancer | Tengu |
| 洞穴 (11-15) | DM200, DM201, Spinner, Shaman, Golem, Monk, Senior, Warlock, Ghoul, GnollExile | DM-300 |
| 城市 (16-20) | Elemental, Succubus, RipperDemon, Wraith, Scorpio, Eye, Warlock, Monk, Golem, GnollGeomancer | DwarfKing |
| 地狱 (21-25) | DemonSpawner, RipperDemon, Eye, Scorpio, Succubus, Wraith, Warlock, Elemental, Ghoul | Yog-Dzewa |
| 特殊 | Mimic(3种), Piranha, Bee, Statue(2种), CrystalWisp, CrystalGuardian, CrystalSpire, FungalCore/Sentry/Spinner, VaultMob | |

## 怪物关键属性

```
Rat:       HP=10,  EXP=3,  ATK=1-4,  DEF=1,  lootChance=8%   (肉)
Snake:     HP=8,   EXP=3,  ATK=1-4,  DEF=4,  lootChance=0%
Slime:     HP=20,  EXP=4,  ATK=2-5,  DEF=1,  lootChance=12%  (肉)
Skeleton:  HP=20,  EXP=6,  ATK=2-6,  DEF=3,  lootChance=12%  (武器)
Thief:     HP=16,  EXP=6,  ATK=2-6,  DEF=2,  lootChance=33%  (戒指)
Bat:       HP=30,  EXP=7,  ATK=2-7,  DEF=3,  lootChance=17%  (药水)
DM-100:    HP=15,  EXP=5,  ATK=3-6,  DEF=4,  lootChance=0%
Gnoll:     HP=15,  EXP=4,  ATK=2-5,  DEF=2,  lootChance=12%  (武器)
Crab:      HP=15,  EXP=4,  ATK=2-5,  DEF=2,  lootChance=0%
```

## 掉落系统

```java
rollToDropLoot():
  if (hero.lvl > maxLvl + 2) return  // 等级碾压不掉落
  if (Random.Float() < lootChance()):
    createLoot() → 掉落
  // 财富戒指额外掉落
  // 幸运附魔额外掉落
  // 灵魂吸吮天赋额外效果
```

## 怪物重生

```java
MobSpawner extends Actor:
  // 由 Level.addRespawner() 创建
  // 每 50 秒（在 DARK 层为 33 秒）尝试生成一个新怪物
  // 从 MobSpawner.getMobRotation(depth) 获取旋转表
  // 生成位置在英雄视野外，距离 >= disLimit
```