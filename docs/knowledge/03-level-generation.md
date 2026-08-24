# 03 - 地牢生成系统

> 核心文件：`Level.java`, `RegularLevel.java`, `Room.java`, `builders/`, `painters/`

## 生成管线（Level.create()）

```java
Level.create()
  ├── Random.pushGenerator(Dungeon.seedCurDepth())  // 种子驱动
  ├── 非 BOSS 层:
  │   ├── 添加基础物品 (食物 + 力量药水 + 升级卷轴 + 附魔石等)
  │   └── 50% 概率选择层主题 (Feeling):
  │       NONE/CHASM/WATER/GRASS/DARK/LARGE/TRAPS/SECRETS
  ├── do { reset 容器 } while (!build())   // 生成失败则重试
  ├── buildFlagMaps()  // 构建 passable/losBlocking 等标志数组
  ├── cleanWalls()     // 清理贴墙的装饰
  ├── createMobs()     // 生成怪物
  ├── createItems()    // 生成物品
  └── Random.popGenerator()
```

## RegularLevel.build()（标准层）

```java
boolean build() {
    builder = builder();          // 随机选 Builder
    initRooms = initRooms();      // 创建房间池
    Random.shuffle(initRooms);
    do {
        rooms = builder.build(initRooms.clone());  // 用图算法连接
    } while (rooms == null);      // 生成失败重试
    return painter().paint(this, rooms);  // 用 Painter 绘制
}
```

## 房间池创建（initRooms()）

```java
initRooms():
  ├── EntranceRoom (入口房间)
  ├── ExitRoom (出口房间)
  ├── 标准房间 × standardRooms()  (按大小分级)
  │   └── setSizeCat 分配大小类别 (4类: S/M/L/XL)
  ├── ShopRoom (每 5 层出现一次: 6, 11, 16)
  ├── SpecialRoom × specialRooms() (23 种随机)
  └── SecretRoom × secretsForFloor(depth) (隐藏房间)
```

**房间数量：**
```
standardRooms: Sewer=7, Prison=8, Caves=9, City=10, Halls=11 (forceMax 时更多)
specialRooms: 通常 2-3 个
secrets: floor 1-5: 0, 6-10: 1, 11-15: 1, 16-20: 2, 21-25: 2
```

## Builder（房间连接算法）

```java
Builder 接口:
  build(ArrayList<Room> rooms) → 返回连接后的房间列表

实现:
  LoopBuilder        // 环状连接（默认首选）
  FigureEightBuilder // 8字形连接
  BranchesBuilder    // 分支状
  LineBuilder        // 线状（旧）
  GridBuilder        // 网格状（旧）

RegularLevel.builder():
  50% → LoopBuilder (curveExponent=2, 随机强度/偏移)
  50% → FigureEightBuilder
```

**连接流程（以 LoopBuilder 为例）：**
```java
LoopBuilder.build():
  ├── setupRooms(rooms)      // 分配房间位置
  ├── entrance.setPos(0,0)   // 入口固定在原点
  ├── mainPathRooms 主路径房间（入口→出口沿线）
  ├── 创建环: 从入口出发，按角度分布房间
  ├── 分配隧道数量（随机）
  ├── 创建分支房间 (branchTunnels)
  └── 最终 setEntrance/ExitPos 并验证连通性
```

**关键算法：**
- 房间作为图的节点（`Room implements Graph.Node`）
- 用距离/角度将房间沿路径分布
- 相邻房间通过 `Room.connected` 连接（`LinkedHashMap<Room, Door>`）
- 房间尺寸随机但受 `minWidth/maxWidth/minHeight/maxHeight` 限制
- `assignRoomArea()` 放置房间到地图上，检测重叠

## Painter（地形绘制）

```java
Painter 接口:
  paint(Level level, ArrayList<Room> rooms) → boolean

实现:
  SewerPainter   // 下水道（绿/水/苔藓）
  PrisonPainter  // 监狱（红/砖）
  CavesPainter   // 洞穴（棕/矿石）
  CityPainter    // 城市（灰/石板）
  HallsPainter   // 地狱（暗/岩浆）

Painter 核心方法:
  Painter.set(level, cell, terrain)   // 设置地形
  Painter.fill(level, rect, terrain)  // 填充矩形
  Painter.drawLine(level, from, to, terrain)  // 画线（走廊）
  Painter.drawInside(level, room, margin, terrain, value)  // 内部绘制
  Painter.fillEllipse(level, rect, terrain)  // 椭圆填充
  Painter.drawCircle(level, rect, terrain)   // 圆形
  Painter.rect(level, rect, terrain)         // 矩形边框
```

## 地形系统（Terrain）

```java
public static final int CHASM     = 0;   // 深渊
public static final int EMPTY     = 1;   // 空地
public static final int GRASS     = 2;   // 草地
public static final int EMPTY_WELL= 3;   // 空井
public static final int WALL      = 4;   // 墙壁
public static final int DOOR      = 5;   // 关着的门
public static final int OPEN_DOOR = 6;   // 开着的门
public static final int ENTRANCE  = 7;   // 入口
public static final int ENTRANCE_SP = 37; // 入口（特殊）
public static final int EXIT      = 8;   // 出口
public static final int EMBERS    = 9;   // 余烬
public static final int LOCKED_DOOR = 10; // 锁着的门
public static final int HERO_LKD_DR = 38; // 英雄锁的门
public static final int CRYSTAL_DOOR = 31; // 水晶门
public static final int PEDESTAL  = 11;  // 基座
public static final int WALL_DECO = 12;  // 装饰墙
public static final int BARRICADE = 13;  // 路障
public static final int EMPTY_SP  = 14;  // 空地（特殊）
public static final int HIGH_GRASS = 15; // 高草
public static final int FURROWED_GRASS = 30; // 犁过的草
public static final int SECRET_DOOR = 16; // 密门
public static final int SECRET_TRAP = 17; // 隐藏陷阱
public static final int TRAP      = 18;  // 可见陷阱
public static final int INACTIVE_TRAP = 19; // 失效陷阱
public static final int EMPTY_DECO= 20;  // 装饰空地
public static final int LOCKED_EXIT = 21; // 锁着的出口
public static final int UNLOCKED_EXIT = 22; // 开的出口
public static final int WELL      = 24;  // 井
public static final int BOOKSHELF = 27;  // 书架
public static final int ALCHEMY   = 28;  // 炼金台
public static final int CUSTOM_DECO_EMPTY = 32; // 自定义空地
public static final int CUSTOM_DECO = 23; // 自定义装饰
public static final int STATUE    = 25;  // 雕像
public static final int STATUE_SP = 26;  // 雕像（特殊）
public static final int REGION_DECO = 33; // 区域装饰
public static final int REGION_DECO_ALT = 34; // 区域装饰变体
public static final int MINE_CRYSTAL = 35; // 矿晶
public static final int MINE_BOULDER = 36; // 矿岩
public static final int WATER     = 29;  // 水

// 地形标志（位掩码）
public static final int PASSABLE    = 0x01;  // 可通过
public static final int LOS_BLOCKING = 0x02; // 阻挡视线
public static final int FLAMABLE    = 0x04;  // 可燃
public static final int SECRET     = 0x08;   // 隐藏
public static final int SOLID      = 0x10;   // 固体
public static final int AVOID      = 0x20;   // 回避（怪物避免走）
public static final int LIQUID     = 0x40;   // 液体
public static final int PIT        = 0x80;   // 深坑
```

## 标志数组（buildFlagMaps）

每个格子生成 8 个 boolean 数组，供路径和 FOV 使用：
```
passable    = flags & PASSABLE
losBlocking = flags & LOS_BLOCKING
flamable    = flags & FLAMABLE
secret      = flags & SECRET
solid       = flags & SOLID
avoid       = flags & AVOID
water       = flags & LIQUID
pit         = flags & PIT
```

边界强制：四周墙壁 solid + losBlocking，不可通行。

**openSpace（大单位空间）：** 非固体格子且有一个开放的角落（两个相邻格子都开放）→ 大型怪物可通行

## FOV 系统（Level.updateFieldOfView）

```java
updateFieldOfView(Char c, boolean[] fieldOfView):
  ├── 计算 cx, cy（角色坐标）
  ├── sighted = 非失明、非隐身、存活
  ├── 如果可见:
  │   ├── 处理可修改的阻挡数组（高草对特定单位透明）
  │   ├── 处理烟幕（SmokeScreen 阻挡视线）
  │   └── ShadowCaster.castShadow(cx, cy, width, fov, blocking, viewDistance)
  ├── 否则: 清空 FOV
  ├── 心灵视野 (MindVision): 看到怪物周围 3x3
  ├── 感知半径 (sense): 察觉视野（看到格子内容但无完整 FOV）
  └── 组合所有来源到 heroFOV
```

## 怪物重生

```java
Level.addRespawner():
  ├── 创建 MobSpawner Actor（延迟 respawnCooldown 时间）
  ├── 在 Actor 循环中周期触发
  └── respawnCooldown():
      ├── 拿到护符后: 5-25 秒（随怪物数量）
      ├── DARK 层: 2/3 × 50s
      └── 默认: 50s

Level.createMob(): 从 MobSpawner.getMobRotation(depth) 取旋转表
Level.spawnMob(disLimit): 在距英雄 disLimit 外随机位置生成
```

## 楼层结构

```
RegularLevel:
  ├── map[int]   → 每个格子的地形类型
  ├── mobs       → HashSet<Mob>
  ├── heaps      → SparseArray<Heap> (按位置索引物品堆)
  ├── blobs      → HashMap<Class<? extends Blob>, Blob>
  ├── plants     → SparseArray<Plant>
  ├── traps      → SparseArray<Trap>
  ├── customTiles → ArrayList<CustomTilemap> (自定义地形渲染)
  └── transitions → ArrayList<LevelTransition> (楼层出入口)
```