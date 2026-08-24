# 知识库：Shattered Pixel Dungeon 原版架构分析

> 基于 [00-Evan/shattered-pixel-dungeon](https://github.com/00-Evan/shattered-pixel-dungeon) 源码分析
> 本地镜像：`E:\project\GitHub\shattered-pixel-dungeon`

## 文档清单

| 文档 | 内容 | 行数 | 优先级 |
|---|---|---|---|
| [01-architecture.md](01-architecture.md) | 整体架构、场景切换、数据流 | 简洁 | ⭐⭐⭐ |
| [02-actor-turn-system.md](02-actor-turn-system.md) | Actor 回合调度、时间线、优先级 | 完整 | ⭐⭐⭐⭐⭐ |
| [03-level-generation.md](03-level-generation.md) | 地牢生成管线、Builder/Painter/Room | 完整 | ⭐⭐⭐⭐⭐ |
| [04-char-combat.md](04-char-combat.md) | 角色属性、战斗公式、攻击流程 | 完整 | ⭐⭐⭐⭐⭐ |
| [05-item-system.md](05-item-system.md) | 物品体系、Generator、子类清单 | 完整 | ⭐⭐⭐⭐⭐ |
| [06-mob-ai.md](06-mob-ai.md) | 怪物 AI 状态机、行为模式 | 完整 | ⭐⭐⭐⭐ |
| [07-buff-blob.md](07-buff-blob.md) | Buff 系统、Blob 区域效果 | 完整 | ⭐⭐⭐⭐ |
| [08-ui-system.md](08-ui-system.md) | UI 组件、场景、窗口系统 | 清单 | ⭐⭐⭐ |
| [09-rooms.md](09-rooms.md) | 房间变体清单（Standard/Special/Secret） | 清单 | ⭐⭐⭐ |
| [10-quick-reference.md](10-quick-reference.md) | 快速参考（常量、枚举、关键数值） | 速查 | ⭐⭐⭐⭐ |
| [11-gframework-template.md](11-gframework-template.md) | GFramework 框架模板完整分析（API/模块/规范） | 完整 | ⭐⭐⭐⭐⭐ |
| [12-gframework-mapping.md](12-gframework-mapping.md) | 原版 → GFramework 映射设计（核心蓝图） | 设计 | ⭐⭐⭐⭐⭐ |

## 核心文件行数

```
Hero.java         2637  ⭐ 英雄动作、天赋、装备
GameScene.java    1865  ⭐ 主游戏场景（输入、渲染、UI）
Level.java        1657  ⭐ 地牢基类（生成、FOV、交互）
Mob.java          1511  ⭐ 怪物 AI 状态机
Char.java         1415  ⭐ 角色基类（战斗、属性、Buff）
Talent.java       1219  ⭐ 天赋系统
Dungeon.java      1093  ⭐ 全局游戏状态
Generator.java     979  ⭐ 物品生成系统
RegularLevel.java  909  ⭐ 标准地牢生成
Armor.java         919  ⭐ 护甲/雕纹装备
Item.java          725  ⭐ 物品基类
```

## 核心依赖关系

```
Dungeon (static 全局状态)
  ├── Hero (extends Char)
  │   ├── Belongings (背包: 武器/护甲/戒指/背包)
  │   ├── HeroClass (6 职业)
  │   ├── HeroSubClass (12 子职业)
  │   └── Talent (天赋树)
  ├── Level (abstract)
  │   ├── RegularLevel → SewerLevel, PrisonLevel, CavesLevel, CityLevel, HallsLevel
  │   ├── BossLevel → SewerBossLevel, PrisonBossLevel, CavesBossLevel, CityBossLevel, HallsBossLevel
  │   └── DeadEndLevel, MiningLevel, VaultLevel, LastLevel, LastShopLevel
  │       ├── Room (Rect + 邻居/连接图)
  │       │   ├── StandardRoom (40+ 变体)
  │       │   ├── SpecialRoom (23 种)
  │       │   ├── SecretRoom (13 种)
  │       │   └── ConnectionRoom (6 种)
  │       ├── Builder (图算法连接房间)
  │       │   ├── LoopBuilder, FigureEightBuilder, BranchesBuilder, LineBuilder, GridBuilder
  │       └── Painter (填充地形/装饰)
  │           └── SewerPainter, PrisonPainter, CavesPainter, CityPainter, HallsPainter
  └── Actor (回合调度)
      ├── Char (abstract)
      │   ├── Hero (玩家)
      │   ├── Mob (abstract, 怪物 AI)
      │   │   ├── 68 种怪物
      │   │   └── NPC (Ghost, Wandmaker, Blacksmith, Imp, Shopkeeper 等)
      │   └── Buff (abstract, 计时状态)
      │       └── 39 种 Buff + 子类
      └── Blob (abstract, 区域效果)
          └── 20 种 Blob
```

## 种子系统

```java
// 每层种子 = 全局种子 + 深度偏移
long seedForDepth(int depth, int branch) {
    // depth 1-30, branch 0-999
    // 通过 pushGenerator → 随机跳转 → popGenerator 实现确定性生成
    Random.pushGenerator(seed);
    for (int i = 0; i < lookAhead; i++) Random.Long();
    long result = Random.Long();
    Random.popGenerator();
    return result;
}
```