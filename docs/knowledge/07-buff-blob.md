# 07 - Buff 与 Blob 系统

> 核心文件：`Buff.java`, `Blob.java`, `actors/buffs/`, `actors/blobs/`

## Buff 系统

### 基类（Buff）

```java
Buff extends Actor:
  target: Char          // 所属角色
  type: buffType        // POSITIVE / NEGATIVE / NEUTRAL
  announced: boolean    // 是否已显示名称
  revivePersists: boolean // 复活后是否保留
  resistances: HashSet<Class<>>  // 抵抗的效果
  immunities: HashSet<Class<>>   // 免疫的效果

  // 优先级: BUFF_PRIO = -30（在回合最后一段执行）
  act():
    diactivate()  // 默认什么都不做，被调度时自动停用

  // 生命期方法
  attachTo(target) → boolean  // 附加到角色
  detach()                     // 从角色移除

  // 静态工具方法
  static affect(target, clazz) → T       // 附加（如已有则复用）
  static append(target, clazz) → T       // 附加（总是新建实例）
  static prolong(target, clazz, duration) → T  // 延长持续时间
  static detach(target, clazz)            // 移除
  static count(target, clazz, count) → T  // 计数型

  // UI 方法
  icon() → int           // BuffIndicator 图标索引
  tintIcon(image)        // 图标着色
  iconFadePercent() → float  // 淡化百分比（快过期时）
  iconTextDisplay() → String // 文字显示
  fx(on)                 // 视觉特效
  heroMessage() → String // 英雄消息
```

### Buff 类型

| 类型 | 数量 | 示例 |
|---|---|---|
| 伤害类 | 7 | Poison, Burning, Bleeding, Corrosion, Ooze, Bleeding, ToxicImbue |
| 控制类 | 10 | Paralysis, Sleep, Slow, Chill, Frost, Vertigo, Cripple, Daze, Amok, Terror, Dread, Charm |
| 增益类 | 12 | Haste, Speed, Bless, Barkskin, Invisibility, Levitation, Fury, Adrenaline, Stamina, ShieldBuff, Healing, Recharging |
| 资源类 | 6 | Hunger, Combo, Preparation, Momentum, MonkEnergy, LockedFloor |
| 系统类 | 6 | Regeneration, MagicalSleep, LostInventory, PinCushion, WellFed, AscensionChallenge |
| 地狱冠军 | 6 | Giant, Blessed, Shielded, Projecting, Burning, Lucky |

### 三种 Buff 子类

```java
// 1. 普通 Buff — 每回合执行 act()，实现复杂逻辑
Buff: Poison, Burning, Hunger, Combo

// 2. FlavourBuff — 简单计时，到期自动移除
FlavourBuff extends Buff:
  // 只需设置持续时间，act() 自动处理倒计时

// 3. CounterBuff — 计数型，累计到阈值触发
CounterBuff extends Buff:
  // 用 count() 增加计数，自定义处理逻辑
```

### 关键 Buff 实现

**Hunger（饥饿度）：** 
- 每回合 -1 饥饿值（从 600 开始，每分钟 -1）
- 饥饿值到 0 时开始每回合扣 HP
- 食物恢复 50-150 饥饿值
- 饱食时（WellFed）缓慢恢复 HP

**Poison（中毒）：**
- 每回合扣 2~5 点伤害（随时间递减）
- 持续 10~20 回合
- 通过抗毒药水治疗

**Burning（燃烧）：**
- 每回合扣 2~5 点伤害
- 站在水中立即熄灭
- 可点燃周围可燃物

**Paralysis（麻痹）：**
- 无法行动
- 被攻击时有一定概率解除（每击 50%）

## Blob 系统

### 基类（Blob）

```java
Blob extends Actor:
  pos: int              // 位置
  cur: int[]            // 每个格子的浓度
  volume: int           // 总体积
  area: int             // 覆盖面积
  level: Level          // 所属楼层

  act():                // 每回合扩散/衰减
    evolve(level)       // 扩散逻辑
    spend(TICK)

  // 子类需实现:
  abstract void evolve(level)  // 扩散算法

  // 静态方法:
  static volumeAt(pos, blobClass) → int  // 某格浓度
  static seed(pos, amount, blobClass)    // 播种
  static seed(Collection<Int>, amount, blobClass) // 批量播种
```

### 20 种 Blob

| Blob | 行为 | 效果 |
|---|---|---|
| ToxicGas | 扩散，每回合扣血 | 毒气伤害 |
| CorrosiveGas | 扩散，腐蚀 | 强腐蚀伤害 |
| Fire | 扩散，点燃 | 燃烧效果 |
| Freezing | 扩散，冻结 | 水变冰，冻结 |
| Electricity | 瞬间扩散 | 电击伤害 |
| ParalyticGas | 扩散 | 麻痹 |
| ConfusionGas | 扩散 | 混乱 |
| StormCloud | 扩散 | 雷暴（电+水） |
| Blizzard | 扩散 | 冰冻+减速 |
| StenchGas | 扩散 | 恶臭（怪物厌恶） |
| SmokeScreen | 扩散 | 阻挡视线 |
| Web | 静态 | 减速、定身 |
| Regrowth | 扩散 | 生长草地 |
| Inferno | 扩散 | 强燃烧 |
| Foliage | 扩散 | 自然生长 |
| GooWarn | 静态 | Goo 战前警告 |
| SacrificialFire | 静态 | 献祭火焰 |
| WellWater | 静态 | 井水（治疗/觉醒/感知） |
| Alchemy | 静态 | 炼金台 |
| VaultFlameTraps | 静态 | 宝库火焰陷阱 |

### Blob 扩散算法

```java
void evolve(Level level):
  // 对于每个有浓度的格子:
  // 1. 向 4 邻域扩散（按浓度比例）
  // 2. 自然衰减（按时间衰减）
  // 3. 受地形影响（水/火互斥，开放空间扩散更快）
  // 4. 更新 volume 和 area 统计
  // 5. 如果 volume == 0 → diactivate()
```

## Buff 与 Blob 的交互

```java
// 角色进入有 Blob 的格子时触发
Level.occupyCell(ch):
  if (Web) → Web.affectChar(ch)  // 定身
  if (SacrificialFire) → 标记献祭
  // 检测地形效果:
  if (WATER) → 熄灭火焰、洗掉粘液
  if (GRASS) → 触发天赋效果
  if (CHASM) → 掉落深渊
  // 触发陷阱
  pressCell(cell, hard)
```