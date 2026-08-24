# GFramework 框架模板知识库

> 基于 GeWuYou.GFramework v0.0.177 + Godot 4.6 (.NET 10)
> 官方文档：https://gewuyou.github.io/GFramework

## 包结构

| NuGet 包 | 命名空间 | 说明 |
|---|---|---|
| `GeWuYou.GFramework` | (元包) | 聚合元包，一键引入所有能力 |
| `GeWuYou.GFramework.Core` | `GFramework.Core` | 核心：架构/命令/事件/查询/IOC/日志 |
| `GeWuYou.GFramework.Core.Abstractions` | `GFramework.Core.Abstractions` | Core 抽象接口定义 |
| `GeWuYou.GFramework.Game` | `GFramework.Game` | 游戏扩展：状态/设置/存储/UI |
| `GeWuYou.GFramework.Game.Abstractions` | `GFramework.Game.Abstractions` | Game 抽象接口定义 |
| `GeWuYou.GFramework.Godot` | `GFramework.Godot` | Godot 集成层 |
| `GeWuYou.GFramework.SourceGenerators` | `GFramework.SourceGenerators` | 源码生成器（[Log]/[ContextAware]） |
| `GeWuYou.GFramework.Godot.SourceGenerators` | `GFramework.Godot.SourceGenerators` | Godot 专用的源码生成器 |

## 核心架构体系

### 三层架构

```
┌─────────────────────────────────────────────────┐
│  Architecture（架构）                           │
│  ├── ISystem（系统）—— 业务逻辑                 │
│  ├── IModel（模型）—— 数据状态                   │
│  └── IUtility（工具）—— 基础设施                 │
├─────────────────────────────────────────────────┤
│  IEngine（引擎）—— Godot 引擎适配层              │
│  IEnvironment（环境）—— 运行环境隔离              │
│  IArchitectureConfiguration（配置）              │
└─────────────────────────────────────────────────┘
```

### 架构引导流程

```csharp
// 1. 创建架构实例
var arch = new GameArchitecture(configuration, environment);

// 2. 安装模块（Utility → System → Model → State）
arch.Initialize();  // 内部调用 InstallModules()

// 3. 绑定上下文（供 [ContextAware] 节点访问）
GameContext.Bind(typeof(GameArchitecture), arch.Context);

// 4. 在 [ContextAware] 节点中通过扩展方法访问
this.GetSystem<T>()    // 获取系统
this.GetModel<T>()     // 获取模型
this.GetUtility<T>()   // 获取工具
this.SendEvent<T>(e)   // 发送事件
this.SendCommand(c)    // 发送命令
this.SendQuery<T>()    // 发送查询
```

### 模块安装顺序（重要）

```
UtilityModule  → 先装工具（存储、序列化、注册表）
SystemModule   → 再装系统（路由、设置系统）
ModelModule    → 然后装模型（设置模型，依赖存储）
StateModule    → 最后装状态机（依赖前面的路由）
```

## 4 个模块说明

### UtilityModule（工具）

```csharp
public class UtilityModule : AbstractModule
{
    public override void Install(IArchitecture architecture)
    {
        // UI 注册表（管理 UiPageConfig → PackedScene 映射）
        architecture.RegisterUtility(new GodotUiRegistry());
        // 场景注册表（管理 SceneKey → PackedScene 映射）
        architecture.RegisterUtility(new GodotSceneRegistry());
        // 纹理注册表（管理 TextureKey → Texture2D 映射）
        architecture.RegisterUtility(new GodotTextureRegistry());
        // UI 工厂（根据 UiKey 实例化场景）
        architecture.RegisterUtility(new GodotUiFactory());
        // JSON 序列化器
        var jsonSerializer = new JsonSerializer();
        architecture.RegisterUtility(jsonSerializer);
        // 文件存储
        var storage = new GodotFileStorage(jsonSerializer);
        architecture.RegisterUtility(storage);
        // 设置数据仓库（持久化 ISettingsData）
        architecture.RegisterUtility(new UnifiedSettingsDataRepository(
            storage, jsonSerializer, new DataRepositoryOptions { ... }));
    }
}
```

### SystemModule（系统）

```csharp
public class SystemModule : AbstractModule
{
    public override void Install(IArchitecture architecture)
    {
        architecture.RegisterSystem(new UiRouter());      // UI 路由
        architecture.RegisterSystem(new SceneRouter());   // 场景路由
        architecture.RegisterSystem(new SettingsSystem()); // 设置应用系统
    }
}
```

### ModelModule（模型）

```csharp
public class ModelModule : AbstractModule
{
    public override void Install(IArchitecture architecture)
    {
        var repo = architecture.Context.GetUtility<ISettingsDataRepository>()!;
        architecture.RegisterModel(
            new SettingsModel<ISettingsDataRepository>(
                new SettingDataLocationProvider(), repo)
                .Also(it =>
                {
                    it.RegisterApplicator(new GodotAudioSettings(it, new AudioBusMap()));
                    it.RegisterApplicator(new GodotGraphicsSettings(it));
                    it.RegisterApplicator(new GodotLocalizationSettings(it, new LocalizationMap()));
                })
        );
    }
}
```

### StateModule（状态）

```csharp
public class StateModule : AbstractModule
{
    public override void Install(IArchitecture architecture)
    {
        architecture.RegisterSystem(new GameStateMachineSystem().Also(it =>
        {
            it.Register(new AppState());  // 注册应用状态
        }));
    }
}
```

## 核心 API 速查

### 架构上下文（IArchitectureContext）

```csharp
// 获取服务
GetSystem<T>()      // 获取系统实例
GetModel<T>()       // 获取模型实例
GetUtility<T>()     // 获取工具实例

// 通信
SendEvent<TEvent>()                // 发送无参数事件
SendEvent<TEvent>(TEvent e)        // 发送带参数事件
RegisterEvent<TEvent>(Action<TEvent> handler)  // 注册事件监听
UnRegisterEvent<TEvent>(Action<TEvent> handler) // 取消监听

SendCommand(ICommand)              // 发送同步命令
SendCommand<TInput>(ICommand<TInput>)  // 发送带输入命令
SendCommandAsync(IAsyncCommand)    // 发送异步命令

SendQuery<TResult>(IQuery<TResult>)    // 发送查询
SendQueryAsync<TResult>(IAsyncQuery<TResult>)  // 发送异步查询
```

### 上下文感知扩展（ContextAwareBase）

```csharp
// 在标注 [ContextAware] 的节点中可用
this.GetSystem<T>()
this.GetModel<T>()
this.GetUtility<T>()
this.SendEvent<T>(e)
this.SendCommand(c)
this.SendQuery<T>()
this.RegisterEvent<T>(handler)
```

### 日志（[Log] 特性）

```csharp
[Log]  // 自动生成 _log 字段
public partial class MyClass
{
    void Method()
    {
        _log.Debug("message");
        _log.Info("message");
        _log.Warn("message");
        _log.Error("message");
        _log.Fatal("message");
    }
}
```

## UI 路由系统

### 页面注册流程

```
1. Godot 编辑器中创建 UiPageConfig 资源
   ├── UiKey → 枚举值（如 UiKey.TemplatePage）
   └── Scene → 场景文件（PackedScene）

2. GameEntryPoint 中配置 UiPageConfigs 数组
   └── 在 _Ready() 中注册到 GodotUiRegistry

3. 通过 UiRouter.Push(UiKeyStr) 推入页面
```

### 页面生命周期

```csharp
ISimpleUiPage（默认空实现）：
  OnEnter(param)   // 进入页面
  OnShow()         // 页面显示
  OnResume()       // 页面恢复（从暂停中）
  OnPause()        // 页面暂停（覆盖新页面时）
  OnHide()         // 页面隐藏
  OnExit()         // 页面退出

IUiPageBehaviorProvider：
  GetPage() → IUiPageBehavior  // 获取页面行为实例
```

### 5 种 UI 层

```csharp
UiLayer:
  UiLayer.Page        // 页面层
  UiLayer.Overlay     // 覆盖层（弹窗）
  UiLayer.Modal       // 模态层
  UiLayer.Toast       // 提示层
  UiLayer.Topmost     // 最顶层
```

## 场景路由系统

```csharp
SceneRouterBase:
  Root: Node?           // 场景根节点
  ChangeScene(key)     // 切换场景
  LoadScene(key)       // 加载场景
  Unload()             // 卸载场景
```

## 设置系统

```csharp
// 设置模型
SettingsModel<TRepo>:
  GetData<T>() → T                     // 获取设置数据
  RegisterApplicator<T>(T applier)     // 注册应用器
  GetApplicator<T>() → T               // 获取应用器
  InitializeAsync()                    // 异步初始化
  SaveAsync()                          // 保存

// 设置应用器示例
GodotAudioSettings(model, busMap)      // 音频设置
GodotGraphicsSettings(model)           // 图形设置
GodotLocalizationSettings(model, map)  // 本地化设置

// 设置数据
AudioSettings  { MasterVolume, BgmVolume, SfxVolume }
GraphicsSettings { Resolution, Fullscreen }
LocalizationSettings { Language }
```

## 状态机系统

```csharp
GameStateMachineSystem:
  Register(state)       // 注册状态
  ChangeState<T>()      // 切换状态
  CurrentState          // 当前状态

IState:
  OnEnter(from)         // 进入状态
  OnExit(to)            // 退出状态
  CanTransitionTo(target) → bool  // 是否允许切换

// 示例：AppState
public class AppState : ContextAwareStateBase
{
    public override void OnEnter(IState? from)
    {
        var uiRouter = this.GetSystem<IUiRouter>()!;
        uiRouter.Clear();
        this.GetSystem<ISceneRouter>()!.Unload();
    }

    public override bool CanTransitionTo(IState target) => true;
}
```

## Partial Class 五文件模式

```
MyPage.cs                   核心：类声明、_Ready()、公开方法
MyPage.Dependencies.cs      Godot 节点引用（GetNode<T>("%Name")）
MyPage.Properties.cs        字段、属性、UiKeyStr
MyPage.Events.cs            CQRS 事件订阅（RegisterEvent()）
MyPage.Signals.cs           Godot 信号连接（ConnectSignal()）
```

### _Ready() 标准结构

```csharp
public override void _Ready()
{
    _ = ReadyAsync();        // 1. 异步初始化（await 架构就绪）
    ConnectPageSignals();    // 2. 绑定 Godot 信号
    RegisterEvents();        // 3. 注册 CQRS 事件
}
```

## CQRS 通信模式

### 事件（Event）—— 广播通知

```csharp
// 定义（不可变，required + init）
public sealed class VolumeChangedEvent
{
    public required string Channel { get; init; }
    public required float Volume { get; init; }
}

// 标记事件（无数据）
public sealed class SomeEvent;

// 发送
this.SendEvent(new VolumeChangedEvent { Channel = "Master", Volume = 0.5f });

// 订阅
this.RegisterEvent<VolumeChangedEvent>(e => { /* 处理 */ })
    .UnRegisterWhenNodeExitTree(this);  // 节点退出时自动注销
```

### 命令（Command）—— 执行操作

```csharp
// 带输入的命令（异步）
public sealed class ChangeMasterVolumeCommand(ChangeMasterVolumeCommandInput input)
    : AbstractAsyncCommand<ChangeMasterVolumeCommandInput>(input)
{
    protected override async Task OnExecuteAsync(ChangeMasterVolumeCommandInput input)
    {
        var model = this.GetModel<ISettingsModel>()!;
        model.GetData<AudioSettings>().MasterVolume = input.Volume;
        await this.GetSystem<ISettingsSystem>()!.Apply<GodotAudioSettings>();
    }
}

// 命令输入（可写，class 非 struct）
public sealed class ChangeMasterVolumeCommandInput : ICommandInput
{
    public float Volume { get; set; }
}

// 无输入命令（同步）
public sealed class ExitGameCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        GameUtil.GetTree().Quit();
    }
}

// 发送命令
this.SendCommand(new ChangeMasterVolumeCommand(new ChangeMasterVolumeCommandInput
    { Volume = 0.5f }));
```

### 查询（Query）—— 获取数据

```csharp
public sealed class GetCurrentSettingsQuery : AbstractQuery<SettingsView>
{
    protected override SettingsView OnDo()
    {
        var model = this.GetModel<ISettingsModel>()!;
        return new SettingsView
        {
            Audio = model.GetData<AudioSettings>(),
            Graphics = model.GetData<GraphicsSettings>(),
            Localization = model.GetData<LocalizationSettings>()
        };
    }
}

// 使用
var result = this.SendQuery(new GetCurrentSettingsQuery());
```

## 源码生成器能力

### [Log] 特性

```csharp
using GFramework.SourceGenerators.Abstractions.logging;

[Log]  // 自动生成：
// private static readonly ILogger _log = LoggerFactory.GetLogger("ClassName");
// _log.Debug() / _log.Info() / _log.Warn() / _log.Error() / _log.Fatal()
public partial class MyClass { }
```

### [ContextAware] 特性

```csharp
using GFramework.SourceGenerators.Abstractions.rule;

[ContextAware]  // 自动注入架构上下文，使扩展方法可用：
// this.GetSystem<T>() / this.GetModel<T>() / this.GetUtility<T>()
// this.SendEvent<T>() / this.SendCommand() / this.SendQuery<T>()
public partial class MyClass { }
```

### 规则

- `[Log]` 和 `[ContextAware]` 必须成对使用
- `[Log]` 在前，`[ContextAware]` 在后
- 两者都标注在实现 `IController` 的 Godot 节点上

## 文件结构规范

```
scripts/
├── component/       # 可复用组件（含接口和实现）
├── entities/        # 领域实体与核心组件
├── system/          # GFramework ISystem 实现
├── menu/            # UI 页面（被 UiRouter 管理）
├── cqrs/            # CQRS 命令/事件/命令输入
│   └── <domain>/
│       ├── command/
│       │   └── input/
│       └── event/
├── enums/           # 枚举定义（按域分子目录）
├── model/           # 领域模型（纯数据结构）
├── core/            # 架构核心（状态机、路由）
├── module/          # GFramework 模块安装
├── constants/       # 全局常量
├── data/            # 可持久化数据类与提供者
└── utility/         # 通用工具与存储接口
```

## 粒子级规范

| 规则 | 要求 |
|---|---|
| 命名空间 | 文件范围声明 `namespace X.Y.Z;`（无花括号） |
| 目录名 | snake_case（全小写+下划线） |
| 事件 | `public sealed class`，属性 `{ get; init; }` + `required` |
| 命令 | `public sealed class`，属性 `{ get; set; }` + `required` |
| 命令输入 | `sealed class : ICommandInput`（**禁止 struct**） |
| Godot 节点 | `public partial class`（不 sealed），[Log] + [ContextAware] 成对 |
| 节点引用 | `GetNode<T>("%Name")` 用 % 唯一名称，接口类型优先 |
| XML 注释 | 中文，接口/事件/命令/公开方法必须有 `<summary>` |
| UI 页面 | 不提 I* 接口（由 UiRouter 管理） |
| _Ready() | 只做调用链：`ReadyAsync()` → `ConnectSignal()` → `RegisterEvent()` |
| GlobalUsings | 只包含 System/Collections/Linq/Tasks，不添加 Godot/GFramework |
| 提交格式 | `<type>(<scope>): <中文描述>`，原子操作 |