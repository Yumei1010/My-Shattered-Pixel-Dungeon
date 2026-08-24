# 05 - 物品系统

> 核心文件：`Item.java`, `Heap.java`, `Generator.java`, `EquipableItem.java`, `KindOfWeapon.java`

## 物品基类（Item）

```java
Item:
  image: int          // 精灵图索引
  icon: int           // 图标标识符（随机物品用）
  stackable: boolean  // 可堆叠
  quantity: int       // 数量
  level: int          // 等级（升级/强化用）
  levelKnown: boolean // 等级是否已知
  cursed: boolean     // 是否诅咒
  cursedKnown: boolean // 诅咒是否已知
  unique: boolean     // 唯一物品（复活保留）
  keptThoughLostInvent: boolean // 死亡保留
  bones: boolean      // 是否可出现在遗骸中
  defaultAction: String // 默认动作（如 "APPLY", "ZAP"）
  usesTargeting: boolean // 需要目标选择

  方法:
    actions(hero) → ArrayList<String>  // 可用操作列表
    doPickUp(hero) → boolean           // 拾取
    doDrop(hero)                       // 丢弃
    doThrow(hero)                      // 投掷
    collect(container) → boolean       // 放入容器
    detach(container) → Item           // 从容器移除
    detachAll(container) → Item        // 全部取出
    isIdentified() → boolean           // 是否已鉴定
    identify()                         // 鉴定
    setName() / toString()             // 显示名称
    info() → String                    // 描述信息
    storeInBundle / restoreFromBundle  // 序列化
```

## 物品子类体系

```
Item
  ├── EquipableItem (可装备)
  │   ├── KindOfWeapon (武器类)
  │   │   ├── Weapon
  │   │   │   ├── MeleeWeapon (近战, 12 种)
  │   │   │   ├── MissileWeapon (远程)
  │   │   │   │   ├── Dart / TippedDart (飞镖)
  │   │   │   │   └── Boomerang 等
  │   │   │   └── SpiritBow (猎人的精神弓)
  │   │   └── ...
  │   ├── Armor (护甲, 7 种: 布/皮/锁/鳞/板/职业)
  │   │   ├── Glyph (雕纹, 13 种)
  │   │   └── Curse (护甲诅咒, 6 种)
  │   ├── Wand (法杖, 13 种)
  │   ├── Ring (戒指, 12 种)
  │   └── Artifact (神器, 16 种)
  ├── Potion (药水, 12 种)
  │   ├── Brew (酿造, 7 种)
  │   ├── Elixir (灵药, 8 种)
  │   └── ExoticPotion (异域药水, 12 种)
  ├── Scroll (卷轴, 12 种)
  │   └── ExoticScroll (异域卷轴, 12 种)
  ├── Bomb (炸弹, 6 种)
  ├── Food (食物, 6 种)
  ├── Bag (背包, 5 种: 药水袋/卷轴袋/箭袋/宝石袋/万能袋)
  ├── Key (钥匙, 4 种: 铁/金/水晶/旧)
  ├── Plant.Seed (种子, 13 种)
  ├── Runestone (符文石, 8 种)
  ├── Spell (法术, 13 种)
  ├── Trinket (小饰品, 12 种)
  ├── Journal (日志: 指南页/文件页/区域传说)
  └── 特殊: Amulet, Ankh, Dewdrop, Gold, Torch, Stylus 等
```

## 物品生成（Generator）

```java
Generator.Category:
  WAND,                // 法杖
  RING,                // 戒指
  ARTIFACT,            // 神器
  WEAPON,              // 近战武器
  ARMOR,               // 护甲
  MISSILE,             // 远程武器
  POTION,              // 药水
  SCROLL,              // 卷轴
  WAND,                // 法杖
  SEED,                // 种子
  FOOD,                // 食物
  GOLD,                // 金币
  POTION_EXOTIC,       // 异域药水
  SCROLL_EXOTIC,       // 异域卷轴
  BOMB,                // 炸弹
  TRINKET,             // 小饰品
  STONE,               // 符文石
  SPELL,               // 法术
  DART,                // 飞镖

// 每个 Category 有:
  chance: float        // 基础生成概率
  classSigma: int      // 数量标准差
  defaultItem: Class   // 默认物品
  items: ArrayList<Class<? extends Item>>  // 可能生成的物品列表

Generator.random(Category)       // 随机生成一个物品
Generator.randomUsingDefaults(Category)  // 使用默认概率
Generator.fullReset()            // 重置所有生成状态
```

## 装备属性

### 武器（Weapon）

```java
Weapon:
  STRReq: int          // 力量需求
  RCH: int             // 攻击范围（近战=1, 长柄=2）
  ACC: float           // 命中修正
  DLY: float           // 攻击速度
  damageMin/Max: int   // 伤害范围（含等级修正）
  enchantment: Enchantment // 附魔
  curse: Curse         // 诅咒

  Enchantment (13 种):
    Blazing, Chilling, Shocking, Grim, Kinetic, Lucky,
    Blooming, Vampiric, Elastic, Projecting, Blocking, Corrupting, Stunning

  Curse (6 种):
    Dazzling, Displacing, Exhausting, Fragile, Sacrificial, Wayward
```

### 护甲（Armor）

```java
Armor:
  STRReq: int          // 力量需求
  DR: int              // 减伤值
  DRMax: int           // 最大减伤
  glyph: Glyph         // 雕纹

  Glyph (13 种):
    AntiMagic, Brimstone, Bulk, Stone, Thorns, Viscosity,
    Camouflage, Entanglement, Flow, Obfuscation, Potential,
    Repulsion, Swiftness

  Curse (6 种):
    Anchoring, Bane, Bulwark, Displacement, Metamorphism, Overgrowth
```

### 法杖（Wand）

```java
Wand:
  curCharges: int      // 当前充能
  maxCharges: int      // 最大充能
  curChargesKnown: boolean

  13 种: MagicMissile, Fireblast, Frost, Lightning, BlastWave,
         Disintegration, Corrosion, Corruption, Regrowth,
         PrismaticLight, Transfusion, LivingEarth, Warding
```

### 戒指（Ring）

```java
Ring:
  buff: Buff 类         // 关联的 Buff（如 RingOfHaste → Haste）
  12 种: Accuracy, Arcana, Elements, Energy, Evasion, Force,
         Furor, Haste, Might, Sharpshooting, Tenacity, Wealth
```

## 地面物品堆（Heap）

```java
Heap:
  pos: int            // 位置
  items: ArrayList<Item>  // 物品列表
  type: Type          // 堆类型
  seen: boolean       // 是否已被看到
  haunted: boolean    // 怨灵（被诅咒物品）

  Type:
    HEAP,             // 普通堆
    FOR_SALE,         // 待售（商店）
    CHEST,            // 箱子
    LOCKED_CHEST,     // 锁着的箱子
    CRYSTAL_CHEST,    // 水晶箱
    TOMB,             // 坟墓
    SKELETON,         // 骷髅
    REMAINS,          // 遗骸
    MIMIC             // 宝箱怪
```

## 物品消耗时间

```java
// 物品动作时间常量
TIME_TO_THROW   = 1.0f    // 投掷
TIME_TO_PICK_UP = 1.0f    // 拾取
TIME_TO_DROP    = 1.0f    // 丢弃
```

## 药水/卷轴随机识别系统

```java
// 每次新游戏随机分配药水颜色和卷轴名称
Scroll.initLabels()  // 随机分配卷轴标识
Potion.initColors()  // 随机分配药水颜色
Ring.initGems()      // 随机分配戒指宝石

ItemStatusHandler:
  // 维护一个"未识别"物品池，每次使用/鉴定后移除
  // 对应关系存储在存档中，跨层不变
```