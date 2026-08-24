# CLAUDE.md

本文档为 Claude Code (claude.ai/code) 在基于本框架模板构建的项目中工作时提供指导。

项目编码规范详见 [CONVENTIONS.md](CONVENTIONS.md)，涵盖命名空间、文件结构、CQRS 约定、XML 注释标准、修饰符规范等。以下为关键要点速查与架构概览。

## 构建与测试

```bash
# 构建项目（需要 Godot .NET SDK 4.6）
dotnet build

# 运行全部测试
dotnet test
```

测试使用 xUnit，测试项目位于 `tests/` 目录下。

## 关键约束速查

以下为 CONVENTIONS.md 的核心规则，开发时务必遵守：

- **命名空间** 与目录一对一映射，使用文件范围声明 `namespace X.Y.Z;`（无大括号）；事件的 C# 关键字 `event` 在命名空间中转义为 `@event`
- **事件** 全部 `public sealed class`；属性全部 `{ get; init; }` + `required`；无数据事件用分号声明 `public sealed class FooEvent;`
- **命令** 全部 `public sealed class`，继承 `AbstractCommand(Async)`；命令输入全部 `sealed class : ICommandInput`（**禁止 struct**）
- **Godot 节点** 全部 `public partial class`（不 sealed）；标注 `[Log]` + `[ContextAware]`（成对，`[Log]` 在前）
- **节点引用** 在 `.Dependencies.cs` 中用 `GetNode<T>("%NodeName")`，使用接口类型（若存在）
- **UI 页面** 不需要提取 `I*` 接口（由 UiRouter 管理，不被其他组件消费）
- **XML 注释** 中文；接口/公开方法/事件/命令必须有 `<summary>`；私有方法按需
- **提交** 格式 `<type>(<scope>): <中文描述>`，每次提交为逻辑独立的原子操作

## 提交规则

- 每次提交必须是逻辑上独立的原子操作。
- **遇到复杂变更时必须分析**：如果一次对话的修改混杂了不同功能、bug 修复或优化，必须主动分析其原子性。
- **按组件或职责拆分**：例如，对 API 格式的调整与对 UI 样式的修改应分开提交。
- **主动建议**：分析后，生成一个包含多个提交的"群组提案"，而不是把所有东西一股脑儿塞进一个提交。

## 架构

**技术栈：** Godot 4.6 + C# (.NET 10) + GFramework (0.0.177) — NuGet 上的 CQRS/ECS 框架。

**DI 引导：** `global/GameEntryPoint`（自动加载单例）创建 `GameArchitecture`，安装 4 个模块：

| 模块 | 职责 |
|---|---|
| `ModelModule` | 注册设置模型及其应用器（音频/图形/本地化） |
| `SystemModule` | 注册 UiRouter、SceneRouter、SettingsSystem |
| `UtilityModule` | 注册工具：UI/场景/纹理注册表、存储、序列化、工厂 |
| `StateModule` | 注册 `GameStateMachineSystem` 及状态 |

**状态 → UI 映射：** 每个状态实现 `ContextAwareStateBase`，在 `OnEnter` 中清除之前的 UI/场景，并通过 `UiRouter.Push()` 推入对应的 UI 页面。参见 `AppState` 作为模板示例。

**UI 页面** 继承 `Control`，实现 `IUiPageBehaviorProvider` + `ISimpleUiPage`。采用 partial class 模式：

| Partial 文件 | 用途 |
|---|---|
| `*.cs` | 核心：`_Ready()` 调用 `ReadyAsync()` → `ConnectSignal()` → `RegisterEvent()` |
| `*.Dependencies.cs` | Godot 节点引用（`%NodeName`）、`ReadyAsync()` 初始化逻辑 |
| `*.Properties.cs` | 字段、属性、`UiKeyStr` |
| `*.Events.cs` | 通过 `RegisterEvent()` 订阅 CQRS 事件 |
| `*.Signals.cs` | Godot 信号 → CQRS 事件桥接（`ConnectSignal()`） |

**Entity partial 类** 遵循相同模式：`Entity.cs`、`Entity.Dependencies.cs`、`Entity.Properties.cs`、`Entity.Events.cs`、`Entity.Signals.cs`。

## 核心模式

### CQRS 通信
组件通过 GFramework 事件通信，而非 Godot 信号：
- **发送：** `this.SendEvent(new SomeEvent { ... })`（触发所有已注册的处理程序）
- **订阅：** 在 `RegisterEvent()` 内使用 `this.RegisterEvent<SomeEvent>(e => { ... })`，并以 `.UnRegisterWhenNodeExitTree(this)` 链式调用确保节点退出时自动注销
- 事件位于 `scripts/cqrs/<domain>/event/`，命名空间 `...cqrs.<domain>.@event`
- 命令位于 `scripts/cqrs/<domain>/command/`，命令输入位于 `scripts/cqrs/<domain>/command/input/`
- 命令发送：`this.SendCommand(new SomeCommand(input))`

### 日志与上下文
- `[Log]` 特性通过 GFramework 源代码生成器自动生成静态 `Log` 属性
- `[ContextAware]` 特性自动注入 GFramework 架构上下文
- 两者**成对使用**，`[Log]` 在前

## 框架模块清单

| 模块 | 文件 | 注册内容 |
|---|---|---|
| ModelModule | `scripts/module/ModelModule.cs` | `SettingsModel` + Audio/Graphics/Localization 应用器 |
| SystemModule | `scripts/module/SystemModule.cs` | `UiRouter`、`SceneRouter`、`SettingsSystem` |
| UtilityModule | `scripts/module/UtilityModule.cs` | UI/场景/纹理注册表、存储、JSON 序列化、设置仓储 |
| StateModule | `scripts/module/StateModule.cs` | `GameStateMachineSystem` + `AppState` |

## 示例 CQRS 域

框架保留了以下通用 CQRS 域作为参考：

- `scripts/cqrs/audio/command/` — 音量控制命令（Master/Bgm/Sfx）
- `scripts/cqrs/graphics/` — 分辨率和全屏切换命令
- `scripts/cqrs/setting/` — 设置保存/重置/查询命令
- `scripts/cqrs/game/command/ExitGameCommand.cs` — 退出游戏命令

## 目录结构约定

```
scripts/
├── component/       # 可复用组件（VolumeContainer 等）
├── constants/       # 全局常量
├── core/            # 框架核心（架构、路由、状态、UI 基类）
├── cqrs/            # CQRS 命令/事件/查询（按域划分）
├── data/            # 数据层（设置数据位置提供者等）
├── enums/           # 枚举（UI Key、场景 Key、纹理 Key 等）
├── model/           # 领域模型（按业务域划分）
├── module/          # DI 模块
├── system/          # 系统（按业务域划分）
└── utility/         # 工具类
```
