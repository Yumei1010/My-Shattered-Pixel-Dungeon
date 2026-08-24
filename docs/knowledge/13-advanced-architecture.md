# 13 - 架构设计进阶：事件频段 + 指令模式 + 行为树

> 三个议题的深度分析与设计方案，面向 GFramework 扩展

---

## 议题一：事件总线频段（Event Band / Channel）

### 问题分析

GFramework 现有的 `EventBus.Register<T>(handler)` 是**按类型唯一**的：

```csharp
// 注册一个 CharMovedEvent 处理器
this.RegisterEvent<CharMovedEvent>(e => { /* 所有移动事件 */ });
```

但在 SPD 中，同一事件类型可能来自不同"源"，订阅者需要区分：

```csharp
// 场景需求：UI 只关心英雄移动，Buff 系统关心所有移动
// 被迫方案一：创建 HeroMovedEvent / MobMovedEvent → 类型爆炸
// 被迫方案二：事件带 Source 字段，订阅者自己 if 过滤 → 运行时开销
```

具体场景：

| 事件类型 | 频段划分 | 订阅者 |
|---|---|---|
| `CharMovedEvent` | `hero` / `mob` / `ally` / `projectile` | 地图渲染全量，UI 仅 hero |
| `CharDamagedEvent` | `physical` / `magical` / `fire` / `poison` | 战斗日志全量，天赋仅 physical |
| `ItemEvent` | `pickup` / `drop` / `equip` / `sell` | 背包 UI 全量，成就仅 pickup |
| `TurnEvent` | `hero` / `mob` / `buff` / `blob` | 游戏循环全量，UI 仅 hero |

### 设计方案：频段化事件总线

```csharp
// 频段定义（静态字符串常量，避免魔法字符串）
public static class EventBands
{
    // 角色移动频段
    public const string Hero = "hero";
    public const string Mob = "mob";
    public const string Ally = "ally";
    public const string All = "all";  // 通配符

    // 伤害频段
    public const string Physical = "physical";
    public const string Magical = "magical";
    public const string Elemental = "elemental";  // 火/冰/电
    public const string Poison = "poison";
    public const string Burn = "burn";
}
```

#### 核心 API

```csharp
// 带频段的事件基类
public abstract record BandedEvent
{
    public string Band { get; init; } = EventBands.All;
}

// 具体事件
public sealed record CharMovedEvent : BandedEvent
{
    public required int EntityId { get; init; }
    public required int From { get; init; }
    public required int To { get; init; }
}

// 频段化事件总线接口
public interface IBandedEventBus
{
    // 发送事件（指定频段，覆盖事件本身 Band）
    void Send<T>(T e, string band) where T : BandedEvent;

    // 订阅特定频段
    IUnRegister Register<T>(Action<T> handler, string band) where T : BandedEvent;
    
    // 订阅所有频段（band = null 或 "all"）
    IUnRegister RegisterAll<T>(Action<T> handler) where T : BandedEvent;
}
```

#### 内部实现

```csharp
public class BandedEventBus : IBandedEventBus
{
    // 三级索引：Type → Band → List<Handler>
    private readonly Dictionary<Type, Dictionary<string, List<Delegate>>> _handlers = new();

    public void Send<T>(T e, string band) where T : BandedEvent
    {
        // 1. 通知特定频段订阅者
        if (_handlers.TryGetValue(typeof(T), out var bands)
            && bands.TryGetValue(band, out var specific))
        {
            foreach (var handler in specific) ((Action<T>)handler)(e);
        }
        // 2. 通知全频段订阅者
        if (bands != null && bands.TryGetValue(EventBands.All, out var all))
        {
            foreach (var handler in all) ((Action<T>)handler)(e);
        }
    }

    public IUnRegister Register<T>(Action<T> handler, string band) where T : BandedEvent
    {
        var dict = _handlers.GetOrAdd(typeof(T), _ => new());
        var list = dict.GetOrAdd(band, _ => new());
        list.Add(handler);
        return new DelegateUnRegister(() => list.Remove(handler));
    }
}
```

#### 与 GFramework 集成

GFramework 的 `IEventBus` 接口不变，新增 `IBandedEventBus`：

```csharp
// 上下文扩展方法
public static class BandedEventExtensions
{
    public static void SendEvent<T>(this IContextAware self, T e, string band)
        where T : BandedEvent
    {
        self.GetService<IBandedEventBus>()!.Send(e, band);
    }

    public static IUnRegister RegisterEvent<T>(this IContextAware self, 
        Action<T> handler, string band) where T : BandedEvent
    {
        return self.GetService<IBandedEventBus>()!.Register(handler, band);
    }
}
```

#### 使用示例

```csharp
// 发送（英雄移动时指定频段）
SendEvent(new CharMovedEvent { EntityId = hero.Id, From = old, To = new }, EventBands.Hero);

// 订阅"仅英雄移动"
RegisterEvent<CharMovedEvent>(e => UpdateHeroIndicator(e.To), EventBands.Hero)
    .UnRegisterWhenNodeExitTree(this);

// 订阅"所有移动"
RegisterEvent<CharMovedEvent>(e => UpdateFogOfWar(e.To), EventBands.All)
    .UnRegisterWhenNodeExitTree(this);
```

#### 性能考量

| 操作 | 复杂度 | 说明 |
|---|---|---|
| 发送（无频段） | O(1) | 直接类型查找 |
| 发送（有频段） | O(1) + O(n) | 类型查找 + 频段查找 + 回调 |
| 注册 | O(1) | 字典插入 |
| 注销 | O(1) | 字典删除 |

1000 次事件发送/帧，3 级字典查找，成本 < 0.1ms。可接受。

---

## 议题二：指令模式（Intent/Command Pattern）

### 问题分析

原版 SPD 中，玩家输入的"意图"到"实际动作"之间有一个解释层：

```
玩家点击格子 (x, y)
  → GameScene 收到点击事件
  → Hero.handle(cell)  // 解释层（检查目标格的内容）
      → 如果目标格有敌人 → AttackAction
      → 如果目标格有物品 → PickUpAction
      → 如果目标格是楼梯 → TransitionAction
      → 如果目标格是空地 → MoveAction
```

GFramework 有 `AbstractCommand`（命令模式），但缺少"意图解释"层。用户指令模式是**意图 → 解释 → 命令**的三层架构。

### 三层指令模型

```
┌─────────────────────────────────────────────────────────────────────┐
│  Intent（意图）— 高层"想做什么"，不关心具体实现                     │
│  IntentInterpreter（解释器）— 根据上下文解析意图为具体命令序列      │
│  Command（命令）— 具体动作，执行游戏逻辑修改                        │
└─────────────────────────────────────────────────────────────────────┘
```

### 意图定义

```csharp
// 所有意图的基类
public abstract record Intent
{
    public int SourceCell { get; init; }
    public int? TargetCell { get; init; }
}

// 具体意图
public sealed record MoveIntent(int TargetCell) : Intent
{
    public int TargetCell { get; } = TargetCell;
}

public sealed record InteractIntent(int TargetCell) : Intent;

public sealed record UseItemIntent(int ItemSlot, int? TargetCell) : Intent;

public sealed record WaitIntent : Intent;
```

### 意图解释器

```csharp
public interface IIntentInterpreter
{
    // 解释意图为命令序列
    IReadOnlyList<ICommand> Interpret(Intent intent, HeroEntity hero, DungeonLevel level);
}

public class HeroIntentInterpreter : IIntentInterpreter
{
    public IReadOnlyList<ICommand> Interpret(Intent intent, HeroEntity hero, DungeonLevel level)
    {
        return intent switch
        {
            MoveIntent mi => InterpretMove(mi, hero, level),
            InteractIntent ii => InterpretInteract(ii, hero, level),
            UseItemIntent ui => InterpretUseItem(ui, hero, level),
            WaitIntent _ => [new WaitCommand()],
            _ => []
        };
    }

    private IReadOnlyList<ICommand> InterpretMove(MoveIntent mi, HeroEntity hero, DungeonLevel level)
    {
        // 解释器核心逻辑：检查目标格的内容
        var targetCell = mi.TargetCell;
        
        // 1. 有敌人 → 攻击
        if (level.GetEnemyAt(targetCell) is { } enemy && hero.CanAttack(enemy))
            return [new AttackCommand(hero, enemy)];

        // 2. 有物品且可拾取 → 移动并拾取
        if (level.GetHeapAt(targetCell) is { } heap && !heap.IsEmpty)
            return [new MoveCommand(hero, targetCell), new PickUpCommand(hero, heap)];

        // 3. 是楼梯 → 切换楼层
        if (level.GetTransitionAt(targetCell) is { } transition)
            return [new TransitionCommand(hero, transition)];

        // 4. 有 NPC → 交互
        if (level.GetNpcAt(targetCell) is { } npc && hero.CanInteract(npc))
            return [new InteractCommand(hero, npc)];

        // 5. 空地 → 移动
        if (level.Terrain.Passable[targetCell])
            return [new MoveCommand(hero, targetCell)];

        // 6. 不可通行 → 解释失败
        return [];
    }
}
```

### 指令驱动架构

```
输入层（InputController）
  │  收到点击/按键事件
  ↓
意图生成（IntentFactory）
  │  生成高层意图（MoveIntent, AttackIntent, UseItemIntent）
  ↓
意图解释器（IntentInterpreter）
  │  检查游戏上下文，解析为具体命令序列
  ↓
命令执行器（CommandExecutor）
  │  执行命令，修改游戏状态
  ↓
事件广播（EventBus）
  │  广播世界变化事件（CharMovedEvent, CharDamagedEvent 等）
  ↓
UI 响应（订阅者）
  │  更新 UI 显示
```

### 完整示例：点击移动

```csharp
// 1. 输入层
public partial class GameInputController : Node
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            var cell = ScreenToCell(GetViewport().GetMousePosition());
            if (cell.HasValue)
                this.SendEvent(new CellClickedEvent(cell.Value));
        }
    }
}

// 2. 意图生成
public class CellClickedHandler : IEventHandler<CellClickedEvent>
{
    public void Handle(CellClickedEvent e)
    {
        var hero = DungeonModel.Instance.Hero;
        var intent = new MoveIntent(e.Cell);
        // 发送意图事件（不直接解释，让解释器系统处理）
        this.SendEvent(new IntentGeneratedEvent(intent));
    }
}

// 3. 意图解释（System）
public class IntentResolutionSystem : ISystem
{
    private readonly IIntentInterpreter _interpreter;

    public void OnIntentGenerated(IntentGeneratedEvent e)
    {
        var commands = _interpreter.Interpret(e.Intent, /* hero */, /* level */);
        foreach (var cmd in commands)
            this.SendCommand(cmd);
    }
}

// 4. 命令执行（TurnSystem 驱动）
public sealed class MoveCommand(HeroEntity hero, int target) : AbstractCommand
{
    protected override void OnExecute()
    {
        hero.Move(target);
        this.SendEvent(new CharMovedEvent { EntityId = hero.Id, From = hero.Pos, To = target }, 
            EventBands.Hero);
    }
}
```

### 与原版 SPD 的对应

| 原版 | 我们的设计 |
|---|---|
| `HeroAction.Move` | `MoveIntent` → `MoveCommand` |
| `HeroAction.Attack` | `InteractIntent`（目标为敌人）→ `AttackCommand` |
| `HeroAction.PickUp` | `InteractIntent`（目标为物品）→ `PickUpCommand` |
| `HeroAction.Interact` | `InteractIntent`（目标为 NPC）→ `InteractCommand` |
| `Hero.handle(cell)` | `HeroIntentInterpreter.Interpret()` |
| `HeroAction.LvlTransition` | `MoveIntent`（目标为楼梯）→ `TransitionCommand` |

### 指令模式的优点

1. **UI 与逻辑解耦**：UI 只发意图，不关心具体实现
2. **可扩展**：添加新动作只需增加新的 Intent 和解释规则
3. **可测试**：解释器可以纯单元测试（输入意图 + 上下文 → 期望命令序列）
4. **可录制**：意图序列可录制为"回放"（AI 决策、教程、回放）
5. **可组合**：多个意图可以组合成"宏"（如：拾取并装备）

---

## 议题三：行为树与分层有限状态机

### 问题分析

原版 SPD 的怪物 AI 使用**单层 FSM**（6 种状态内联实现）：

```
Mob.state = SLEEPING / HUNTING / WANDERING / FLEEING / PASSIVE / INVESTIGATING
```

**单层 FSM 的局限性：**
1. **状态爆炸**：复杂怪物（Tengu 1142 行、DwarfKing 811 行）需要大量特殊逻辑，挤在 act() 中
2. **行为复用难**：巡逻逻辑、追击逻辑在多种怪物之间无法复用
3. **条件耦合**：状态转移条件与行为逻辑混合在一起
4. **扩展性差**：添加新行为需要修改现有状态

### 推荐方案：混合架构（分层 FSM + 行为树）

```
MobBrain
  ├── 顶层 FSM（宏观生命周期）
  │   ├── SLEEPING  → 子节点：睡眠行为树
  │   ├── PATROL    → 子节点：巡逻行为树
  │   ├── CHASE     → 子节点：追击行为树
  │   ├── COMBAT    → 子节点：战斗行为树
  │   ├── FLEE      → 子节点：逃跑行为树
  │   └── SPECIAL   → 子节点：Boss 特殊技能行为树
  └── 行为树（微观决策，每帧评估）
```

### 顶层 FSM（宏观状态机）

```csharp
// 宏观状态接口
public interface IMobState
{
    string Name { get; }
    void OnEnter(MobEntity mob);
    void OnExit(MobEntity mob);
    MobStateType Evaluate(MobEntity mob);  // 返回下一个状态
    // 行为树将在此状态下执行
}

// 状态类型
public enum MobStateType
{
    Sleep,     // 睡眠
    Patrol,    // 巡逻（原 WANDERING）
    Chase,     // 追击（原 HUNTING，但未发现敌人时）
    Combat,    // 战斗（原 HUNTING，且可攻击）
    Flee,      // 逃跑（原 FLEEING）
    Special,   // 特殊技能（Boss 专用）
    Passive,   // 被动（原 PASSIVE）
}

// 宏观状态机
public class MobStateMachine
{
    public MobStateType Current { get; private set; }
    private readonly Dictionary<MobStateType, IMobState> _states = new();

    public void Update(MobEntity mob)
    {
        var current = _states[Current];
        // 1. 评估是否需要切换状态
        var next = current.Evaluate(mob);
        if (next != Current)
        {
            current.OnExit(mob);
            Current = next;
            _states[next].OnEnter(mob);
        }
        // 2. 当前状态的 Update 由行为树驱动
    }
}
```

### 行为树（微观决策引擎）

```csharp
// ---- 节点基类 ----
public abstract class BTNode
{
    public abstract BTStatus Execute(MobEntity mob, float delta);
}

public enum BTStatus { Success, Failure, Running }

// ---- 组合节点 ----
public class Selector : BTNode  // 顺序执行，直到成功
{
    private readonly BTNode[] _children;
    public Selector(params BTNode[] children) => _children = children;

    public override BTStatus Execute(MobEntity mob, float delta)
    {
        foreach (var child in _children)
        {
            var result = child.Execute(mob, delta);
            if (result != BTStatus.Failure) return result;
        }
        return BTStatus.Failure;
    }
}

public class Sequence : BTNode  // 顺序执行，全部成功才成功
{
    private readonly BTNode[] _children;
    public Sequence(params BTNode[] children) => _children = children;

    public override BTStatus Execute(MobEntity mob, float delta)
    {
        foreach (var child in _children)
        {
            var result = child.Execute(mob, delta);
            if (result != BTStatus.Success) return result;
        }
        return BTStatus.Success;
    }
}

// ---- 装饰器 ----
public class Inverter : BTNode  // 结果取反
{
    private readonly BTNode _child;
    public Inverter(BTNode child) => _child = child;
    public override BTStatus Execute(MobEntity mob, float delta)
        => _child.Execute(mob, delta) switch
        {
            BTStatus.Success => BTStatus.Failure,
            BTStatus.Failure => BTStatus.Success,
            _ => BTStatus.Running
        };
}

public class Cooldown : BTNode  // 冷却时间装饰器
{
    private readonly BTNode _child;
    private readonly float _cooldown;
    private float _lastRun = float.MinValue;
    public Cooldown(BTNode child, float seconds) => (_child, _cooldown) = (child, seconds);
    
    public override BTStatus Execute(MobEntity mob, float delta)
    {
        if (mob.WorldTime - _lastRun < _cooldown) return BTStatus.Failure;
        _lastRun = mob.WorldTime;
        return _child.Execute(mob, delta);
    }
}

// ---- 条件节点 ----
public class Condition : BTNode
{
    private readonly Func<MobEntity, bool> _predicate;
    public Condition(Func<MobEntity, bool> predicate) => _predicate = predicate;
    public override BTStatus Execute(MobEntity mob, float delta)
        => _predicate(mob) ? BTStatus.Success : BTStatus.Failure;
}

// ---- 动作节点 ----
public class MoveTowardEnemy : BTNode
{
    public override BTStatus Execute(MobEntity mob, float delta)
    {
        if (mob.Target == null) return BTStatus.Failure;
        if (mob.MoveToward(mob.Target.Position)) return BTStatus.Success;
        return BTStatus.Running;
    }
}

public class AttackEnemy : BTNode
{
    public override BTStatus Execute(MobEntity mob, float delta)
    {
        if (mob.Target == null || !mob.CanAttack(mob.Target)) return BTStatus.Failure;
        mob.Attack(mob.Target);
        return BTStatus.Success;
    }
}
```

### 组合示例：Rat（老鼠）的 AI

```csharp
// 老鼠的行为树
public class RatBrain : MobBrain
{
    protected override BTNode BuildCombatTree()
    {
        // 战斗状态：能攻击则攻击，否则追击
        return new Selector(
            new AttackEnemy(),           // 能攻击则攻击
            new MoveTowardEnemy()        // 否则靠近
        );
    }

    protected override BTNode BuildPatrolTree()
    {
        // 巡逻状态：有敌人则追击，否则随机走动
        return new Selector(
            new Sequence(
                new Condition(m => m.HasVisibleEnemy),
                new MoveTowardEnemy()
            ),
            new RandomWander()           // 没有敌人随机走
        );
    }
}
```

### 组合示例：Tengu（Boss）的 AI

```csharp
// Tengu 的行为树（原版 1142 行！）
public class TenguBrain : MobBrain
{
    // 阶段 1：投掷炸弹
    // 阶段 2：闪现 + 召唤烟雾
    // 阶段 3：狂暴

    protected override BTNode BuildCombatTree()
    {
        // 战斗行为树
        return new Sequence(
            new Condition(m => m.IsAlive),
            new Selector(
                // 优先级 1：阶段 3 特殊技能
                new Sequence(
                    new Condition(m => m.HP <= m.MaxHP * 0.33f),
                    new Cooldown(new TenguBerserkAttack(), 3f)
                ),
                // 优先级 2：阶段 2 特殊技能
                new Sequence(
                    new Condition(m => m.HP <= m.MaxHP * 0.66f),
                    new Selector(
                        new Cooldown(new TenguTeleport(), 5f),
                        new Cooldown(new TenguSmokeBomb(), 8f)
                    )
                ),
                // 优先级 3：阶段 1 投掷炸弹
                new Cooldown(new TenguThrowBomb(), 2f),
                // 优先级 4：基础攻击
                new AttackEnemy()
            )
        );
    }
}
```

### 原版 6 状态 → 混合架构映射

| 原版状态 | 混合架构 | 说明 |
|---|---|---|
| `SLEEPING` | 顶层 FSM: Sleep + 子行为树 | 睡眠检测、唤醒逻辑 |
| `WANDERING` | 顶层 FSM: Patrol + 子行为树 | 随机走动、区域巡逻 |
| `HUNTING`（未发现敌人） | 顶层 FSM: Chase + 子行为树 | 追击（追声音/气味） |
| `HUNTING`（可攻击） | 顶层 FSM: Combat + 子行为树 | 战斗决策 |
| `FLEEING` | 顶层 FSM: Flee + 子行为树 | 逃跑、逃脱 |
| `PASSIVE` | 顶层 FSM: Passive | 不动（NPC） |
| 特殊 Boss 技能 | 顶层 FSM: Special + 子行为树 | Boss 专属技能 |

### 与 GFramework 集成

```csharp
// MobBrain 作为 MonoBehaviour 组件（不继承 GFramework StateMachine）
public partial class MobBrain : Node
{
    private MobStateMachine _fsm;
    private BTNode _currentTree;
    
    public void Initialize(MobEntity mob)
    {
        _fsm = new MobStateMachine();
        _fsm.Register(new SleepState(this));
        _fsm.Register(new PatrolState(this));
        // ...
        
        // 行为树只构建一次
        _currentTree = BuildBehaviorTree();
        _fsm.ChangeTo(MobStateType.Sleep);
    }

    // 每回合调用（由 TurnSystem 驱动）
    public void ExecuteTurn(MobEntity mob)
    {
        _fsm.Update(mob);
        _currentTree.Execute(mob, 0);
    }
}
```

### 两种状态机的关系

```
GFramework StateMachine（游戏级）
  └── GameState（Title / Game / Paused / Died）
      → 控制游戏流程

MobStateMachine（实体级，非 GFramework）
  └── MobStateType（Sleep / Patrol / Chase / Combat / Flee）
      → 控制怪物行为

BehaviorTree（实体级，决策级）
  └── BTNode 组合（Selector / Sequence / Condition / Action）
      → 单帧决策评估
```

### 设计决策

| 决策 | 选项 | 选择 | 理由 |
|---|---|---|---|
| 顶层 FSM 是否复用 GFramework `StateMachine` | 是 / 否 | **否** | GFramework 的 StateMachine 是全局的（游戏状态），实体 FSM 应独立轻量 |
| 行为树节点是否为 Godot Node | 是 / 否 | **否（纯 C#）** | 便于单元测试，不依赖 Godot |
| 条件节点 vs 装饰器 | 都支持 | **都支持** | 条件用于"是否"，装饰器用于"如何" |
| 行为树可视化 | 需要 / 不需要 | **需要** | 复杂 AI 调试必需，后期实现 |

### 注意事项

1. **性能**：行为树每帧评估，但不需要每帧都执行完整树。可以用 `dirty` 标记或缓存结果
2. **行为树与 FSM 的状态同步**：FSM 切换状态时，行为树应重置（`OnEnter` 时重建/重置）
3. **行为树节点池**：避免每帧创建新节点，使用对象池复用
4. **调试**：行为树节点状态应有可视化输出（当前执行节点、状态、条件值）