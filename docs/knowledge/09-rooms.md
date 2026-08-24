# 09 - 房间变体清单

> 核心文件：`levels/rooms/`

## 房间基础结构

```java
Room extends Rect implements Graph.Node:
  neigbours: ArrayList<Room>     // 邻居房间
  connected: LinkedHashMap<Room, Door>  // 已连接房间（含门类型）
  distance: int                  // 距离
  price: int                     // 连接代价

  // 抽象方法
  minWidth() / maxWidth()        // 宽范围
  minHeight() / maxHeight()      // 高范围
  setSize() / setSizeCat()       // 设置大小
  paint(level)                   // 绘制房间内容

Door 类型:
  Door()  // 普通门
  ├── Door.Type.REGULAR          // 普通（木门）
  ├── Door.Type.HIDDEN           // 隐藏（密门）
  ├── Door.Type.LOCKED           // 上锁
  ├── Door.Type.BARRICADE        // 路障
  └── Door.Type.BASIC            // 基础（无门框）
```

## StandardRoom 变体（40+ 种）

| 房间 | 继承 | 特征 |
|---|---|---|
| `EmptyRoom` | StandardRoom | 空地，基数 |
| `RingRoom` | StandardRoom | 环形（中间空地 + 外圈走廊） |
| `CircleBasinRoom` | PatchRoom | 圆形凹陷（水/火） |
| `CirclePitRoom` | StandardRoom | 圆形坑 |
| `CircleWallRoom` | StandardRoom | 圆形墙（中间柱子） |
| `SewerPipeRoom` | StandardRoom | 下水道管（交叉管道） |
| `HallwayRoom` | StandardRoom | 走廊（长条形） |
| `StripedRoom` | StandardRoom | 条纹（交替地形） |
| `SegmentedRoom` | StandardRoom | 分段（分隔墙+门） |
| `StudyRoom` | StandardRoom | 书房（书架+桌子） |
| `PillarsRoom` | StandardRoom | 柱子（4 根柱子） |
| `CellBlockRoom` | StandardRoom | 牢房（小隔间） |
| `CaveRoom` | PatchRoom | 洞穴（不规则墙） |
| `CavesFissureRoom` | StandardRoom | 洞穴裂缝 |
| `ChasmRoom` | PatchRoom | 深渊裂缝 |
| `ChasmBridgeRoom` | StandardBridgeRoom | 深渊桥 |
| `FissureRoom` | StandardRoom | 裂缝 |
| `GrassyGraveRoom` | StandardRoom | 草地坟墓 |
| `MinefieldRoom` | StandardRoom | 雷区（陷阱密集） |
| `PatchRoom` | StandardRoom | 补丁（随机地形块） |
| `PlantsRoom` | StandardRoom | 植物房（高草+种子） |
| `PlatformRoom` | StandardRoom | 平台（间隔地板） |
| `RitualRoom` | PatchRoom | 仪式（圆形+符号） |
| `RuinsRoom` | PatchRoom | 废墟（破损墙） |
| `SkullsRoom` | StandardRoom | 骷髅头 |
| `StatuesRoom` | StandardRoom | 雕像群 |
| `StatueLineRoom` | StandardRoom | 雕像线 |
| `SuspiciousChestRoom` | StandardRoom | 可疑箱子 |
| `AquariumRoom` | StandardRoom | 水族馆 |
| `BurnedRoom` | PatchRoom | 烧毁（余烬） |
| `LibraryHallRoom` | StandardRoom | 图书馆大厅 |
| `LibraryRingRoom` | StandardRoom | 图书馆环形 |
| `SegmentedLibraryRoom` | StandardRoom | 分段图书馆 |
| `StandardBridgeRoom` | StandardRoom | 标准桥 |
| `RegionDecoLineRoom` | StatueLineRoom | 区域装饰线 |
| `RegionDecoPatchRoom` | PatchRoom | 区域装饰补丁 |
| `RegionDecoBridgeRoom` | StandardBridgeRoom | 区域装饰桥 |
| `WaterBridgeRoom` | StandardBridgeRoom | 水桥 |
| `ImpShopRoom` | ShopRoom | 矮人商店 |

## SpecialRoom 变体（23 种）

| 房间 | 出现 | 内容 |
|---|---|---|
| `ShopRoom` | 6, 11, 16 层 | 商店（NPC 商人） |
| `GardenRoom` | 任何 | 花园（高草+水井） |
| `LaboratoryRoom` | 3-4, 8-9, 13-14, 18-19 | 炼金实验室 |
| `LibraryRoom` | 任何 | 图书馆（卷轴架子） |
| `StatueRoom` | 任何 | 雕像挑战（雕像怪） |
| `SacrificeRoom` | 任何 | 献祭室（火焰+奖励） |
| `TreasuryRoom` | 任何 | 宝库（金币堆） |
| `ArmoryRoom` | 任何 | 武器库（武器+护甲） |
| `StorageRoom` | 任何 | 仓库（随机物品） |
| `PoolRoom` | 任何 | 水池 |
| `CryptRoom` | 任何 | 墓穴（骷髅+棺材） |
| `PitRoom` | 任何 | 深坑（掉到下层的物品） |
| `TrapsRoom` | 任何 | 陷阱房 |
| `ToxicGasRoom` | 任何 | 毒气房 |
| `MagicWellRoom` | 任何 | 魔法井 |
| `MagicalFireRoom` | 任何 | 魔法火 |
| `RunestoneRoom` | 任何 | 符文石房 |
| `WeakFloorRoom` | 任何 | 弱地板（会塌陷） |
| `SentryRoom` | 任何 | 岗哨 |
| `CrystalVaultRoom` | 任何 | 水晶宝库 |
| `CrystalChoiceRoom` | 任何 | 水晶选择 |
| `CrystalPathRoom` | 任何 | 水晶路径 |
| `DemonSpawnerRoom` | 21-24 | 恶魔生成器房 |

## SecretRoom 变体（13 种）

| 房间 | 内容 |
|---|---|
| `SecretArtilleryRoom` | 远程炮台 |
| `SecretChestChasmRoom` | 深渊箱子 |
| `SecretGardenRoom` | 秘密花园 |
| `SecretHoardRoom` | 藏宝室 |
| `SecretHoneypotRoom` | 蜜罐房 |
| `SecretLaboratoryRoom` | 秘密实验室 |
| `SecretLarderRoom` | 食物储藏室 |
| `SecretLibraryRoom` | 秘密图书馆 |
| `SecretMazeRoom` | 迷宫 |
| `SecretRunestoneRoom` | 符文石房 |
| `SecretSummoningRoom` | 召唤房 |
| `SecretWellRoom` | 秘密井 |
| `RatKingRoom` | 老鼠王 |

## ConnectionRoom 变体（7 种）

| 房间 | 继承 | 用途 |
|---|---|---|
| `TunnelRoom` | ConnectionRoom | 标准隧道 |
| `BridgeRoom` | TunnelRoom | 桥（带深渊） |
| `RingTunnelRoom` | TunnelRoom | 环形隧道 |
| `RingBridgeRoom` | RingTunnelRoom | 环形桥 |
| `PerimeterRoom` | ConnectionRoom | 周边走廊 |
| `WalkwayRoom` | PerimeterRoom | 走道 |
| `MazeConnectionRoom` | ConnectionRoom | 迷宫连接 |