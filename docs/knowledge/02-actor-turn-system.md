# 02 - Actor 回合调度系统

> 核心文件：`Actor.java`, `Char.java` (spend 部分)

## 核心概念

Actor 系统是**基于时间的优先级调度器**，在独立线程中运行。

```
时间单位: TICK = 1.0f
Actor.time = 当前时间（从 0 递增）
Actor.now = 当前正在处理的时刻
```

## 优先级链

```java
VFX_PRIO    = 100   // 视觉特效（粒子、动画）
HERO_PRIO   = 0     // 英雄（玩家）
BLOB_PRIO   = -10   // 区域效果（毒气、蛛网）
MOB_PRIO    = -20   // 怪物
BUFF_PRIO   = -30   // 计时状态
DEFAULT     = -100  // 无优先级时
```

**规则：** 时间相同时，优先级高的先行动

## 核心循环（Actor.process()）

```java
Actor.process()  // 在独立线程中运行
  loop:
    current = 全 Actor 中 time 最小 + 优先级最高的
    if current == null: 等待（线程阻塞）
    otherwise:
      now = current.time
      if current 是 Char 且 sprite 在移动: 等待移动完成
      acting.act()  → 执行行动
      if 行动完成且英雄存活: 继续循环
      else: 通知 GameScene 线程（释放锁），等待再次唤醒
```

**关键细节：**
- 精灵（sprite）移动时，Actor 线程会 `wait()` 直到移动完成
- 每次行动后检查 `Dungeon.hero.isAlive()`
- 没有行动者时，线程 `notify()` → `wait()` 等待玩家输入
- 游戏退出时 `keepActorThreadAlive = false`

## 时间管理

```java
protected void spend(float time)      // 消耗时间，受速度影响
protected void spendConstant(float time)  // 固定消耗时间，不受影响
protected void postpone(float time)   // 推后到指定时间点
public void spendToWhole()            // 向上取整到整数值
public float cooldown()               // 剩余时间 = this.time - now
```

**Char 的 spend 重写：**
```java
// Slow 时时间翻倍 (0.5x 速度)
// Chill 时按比例减速
// Speed 时时间减半 (2x 速度)
timeScale = 1f
if (buffs(Slow))     timeScale *= 0.5f
else if (buffs(Chill)) timeScale *= buff(Chill).speedFactor()
if (buffs(Speed))    timeScale *= 2.0f
spend(time / timeScale)
```

**Char 的 spendConstant 重写：**
```java
// 时间冻结效果（时之沙、迅捷蓟）会拦截时间消耗
if (timeFreeze)  → freeze.processTime(time)
if (timeBubble)  → bubble.processTime(time)
否则 → super.spendConstant(time)
```

## Actor 生命周期

```java
Actor.add(actor, time)    // 加入调度队列
Actor.remove(actor)       // 从调度队列移除
Actor.init()              // 初始化：添加英雄、怪物、Blob
Actor.clear()             // 清空所有 Actor
Actor.fixTime()           // 修正时间偏移（所有 Actor 减去最小时间）
Actor.next()              // 标记当前 Actor 完成
```

**Actor.init() 调用时机：** 每次加载新楼层时

## 主要 Actor 子类

```
Actor
  ├── Char (abstract)
  │   ├── Hero      → 玩家（HERO_PRIO, 0）
  │   ├── Mob       → 怪物/盟友（MOB_PRIO, -20）
  │   └── Buff      → 计时状态（BUFF_PRIO, -30）
  ├── Blob (abstract) → 区域效果（BLOB_PRIO, -10）
  └── MobSpawner    → 怪物重生（特殊优先级）
```

## 英雄行动循环（Hero.act()）

```java
Hero.act()
  ├── 更新 FOV
  ├── 检查可见怪物 (checkVisibleMobs)
  ├── 刷新 Buff 图标
  ├── 如果麻痹: spendAndNext(TICK)
  ├── 如果 curAction == null:
  │   ├── 如果休息: spendConstant(TIME_TO_REST)
  │   └── 否则: ready()（等待玩家输入）
  └── 如果有 curAction:
      ├── actMove()      → 移动
      ├── actAttack()    → 攻击
      ├── actPickUp()    → 拾取物品
      ├── actInteract()  → 与 NPC 交互
      ├── actOpenChest() → 开箱
      ├── actUnlock()    → 开锁
      ├── actBuy()       → 购买
      ├── actMine()      → 挖掘
      ├── actTransition()→ 楼层切换
      └── actAlchemy()   → 炼金
```

## 移动物理

**移动 = 1 步消耗 1/speed() 时间**

```java
// 眩晕效果（Vertigo）：移动时随机方向偏移
if (buff(Vertigo)) {
    step = pos + NEIGHBOURS8[random(8)]
    if (!passable) return  // 撞墙不走
}
// 开门/关门
if (map[pos] == OPEN_DOOR) Door.leave(pos)
pos = step
// 非英雄角色：根据 FOV 设置可见性
if (this != hero) sprite.visible = heroFOV[pos]
// 触发站入格子的效果
Dungeon.level.occupyCell(this)
```

## 关键时间常量

| 动作 | 时间消耗 |
|---|---|
| 移动 1 格 | 1 / speed() |
| 攻击 | 1f (可通过 attackDelay 调整) |
| 拾取物品 | 1f |
| 投掷物品 | 1f |
| 躺下 | 1f |
| 休息 | TIME_TO_REST |
| 唤醒 | 1f |
| TICK（基本单位） | 1f |