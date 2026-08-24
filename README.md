# My-Shattered-Pixel-Dungeon

基于 [GFramework](https://github.com/GeWuYou/GFramework) (v0.0.177) 与 Godot 4.6 (.NET) 的**《破碎的像素地牢》(Shattered Pixel Dungeon) 全量复刻项目**，目标是在完成游戏复刻的同时，抽离出一套可复用的战旗游戏模板。

## 项目定位

```
GFramework（上游 CQRS/ECS 框架，Apache 2.0）
    ↓
My-GFramework-Godot-Template（GFramework 起手骨架）
    ↓
My-Shattered-Pixel-Dungeon（基于原版架构分析与素材的完整复刻 + 战旗模板抽象）
```

- **复刻对象：** [Shattered Pixel Dungeon](https://github.com/00-Evan/shattered-pixel-dungeon)（libGDX/Java，GPLv3），作者 Evan Debenham
- **复刻范围：** 回合制地牢生成、战斗/物品系统、怪物 AI、FOV 照明、药水/卷轴识别、升级附魔、饥饿度陷阱等核心机制
- **附加目标：** 将通用 SRPG 逻辑（网格/回合/行动/属性）抽离为独立模板，供其他战旗游戏复用

## 技术栈

- **引擎：** Godot 4.6 (.NET)
- **运行时：** .NET 10
- **框架：** GFramework 0.0.177（NuGet: `GeWuYou.GFramework`）
- **语言：** C# (LangVersion preview)

## 命名规范

1. 变量名：首字母小写，驼峰命名法
2. 常量名：全大写，下划线分隔
3. 文件夹和文件名：全小写，下划线分隔（snake_case）
4. 命名空间：与目录层次一一对应，`namespace X.Y.Z;`（文件范围声明，无花括号）
5. 提交：`<type>(<scope>): <中文描述>`

详见 [CONVENTIONS.md](CONVENTIONS.md)。

## 文件夹结构

| 目录 | 用途 |
|---|---|
| `assets/` | 游戏资源（原版复刻素材 + 自有资源，详见 [assets/README.md](assets/README.md)） |
| `global/` | Godot 自动加载单例（GameEntryPoint、UiRoot、SceneRoot 等） |
| `resource/` | Godot 资源文件（音频总线布局、主题） |
| `scenes/` | 主场景 `main.tscn` 及组件场景 |
| `script_templates/` | Godot 自定义脚本模板（Controller / Page / Model） |
| `scripts/core/` | 框架核心：架构引导、状态机、UI/场景路由、配置资源 |
| `scripts/module/` | DI 模块安装（Model / System / Utility / State） |
| `scripts/cqrs/` | CQRS 命令/事件/查询，按域分目录 |
| `scripts/component/` | 可复用组件（VolumeContainer、IState 等） |
| `scripts/enums/` | 枚举定义（UiKey、SceneKey、TextureKey、InputPhase） |
| `scripts/constants/` | 全局常量（GameConstants、UiLayers） |
| `scripts/utility/` | 工具类（纹理注册表、存储接口） |
| `scripts/data/` | 数据层（设置位置提供者） |

## 当前骨架

| 层级 | 内容 |
|---|---|
| DI 引导 | `GameArchitecture` + 4 模块（Model / System / Utility / State） |
| 路由 | `UiRouter`、`SceneRouter`、`UiFactory` |
| 状态机 | `GameStateMachineSystem` + `AppState` 示例 |
| 全局节点 | `GameEntryPoint`、`UiRoot`、`SceneRoot`、`SceneTransitionManager` |
| CQRS 示例 | 音量控制、分辨率/全屏切换、设置存取、退出游戏 |
| 通用组件 | `VolumeContainer`、`IState` |
| 编码模板 | `script_templates/` 下 3 个 Godot 脚本模板 |
| 编码规范 | `CONVENTIONS.md` — 命名空间、CQRS、partial class、XML 注释等全套约束 |
| CI 审查 | TruffleHog 密钥扫描 + CodeQL 静态分析 + .NET 构建 + 自动版本标签 |

## 构建与测试

```bash
# 构建项目（需要 Godot .NET SDK 4.6）
dotnet build

# 运行全部测试
dotnet test
```

测试使用 xUnit，测试项目位于 `tests/` 目录下。

## 文档与参考

- [开发规划](docs/development-plan.md) — 完整项目规划、分阶段里程碑、目录结构、架构决策
- [原版架构知识库](docs/knowledge/README.md) — 12 篇原版代码分析 + GFramework 模板分析 + 架构设计
- 原版源码：[00-Evan/shattered-pixel-dungeon](https://github.com/00-Evan/shattered-pixel-dungeon)（本地镜像：`E:\project\GitHub\shattered-pixel-dungeon`）
- 原版文档：[Shattered Pixel Dungeon 官方博客](https://www.shatteredpixel.com/blog/)

## 许可证

本项目包含 GPLv3 素材，按 GPLv3 条款构成**组合作品**，整体许可结构如下：

### 游戏整体（组合作品，含 GPLv3 素材）
按 **GNU General Public License v3.0 (GPLv3)** 分发。

原因：游戏运行时源码会加载 `assets/` 下的复刻素材，二者构成不可分割的组合作品（combined work），受 GPLv3 传染性条款约束。GPLv3 全文见 [assets/GPL-3.0-LICENSE.txt](assets/GPL-3.0-LICENSE.txt)。

### 自有源码（`global/`、`scripts/`、`tests/` 等）
额外提供 **Apache License 2.0** 选项：由于 Apache 2.0 与 GPLv3 兼容，本项目自有代码可按 GPLv3 或 Apache 2.0 任一条款使用（单独取用源码、不包含 GPLv3 素材时适用 Apache 2.0）。Apache 2.0 全文见根目录 [LICENSE](LICENSE)。

### 复刻素材（`assets/` 目录下的原版资源）
`assets/` 中的美术、音频、字体、本地化文本等资源来自 **Shattered Pixel Dungeon**（Copyright (C) 2014-2026 Evan Debenham，基于 Pixel Dungeon Copyright (C) 2012-2015 Oleg Dolya），按 **GPLv3** 许可分发，**不可**单独按 Apache 2.0 使用。

- GPLv3 全文见 [assets/GPL-3.0-LICENSE.txt](assets/GPL-3.0-LICENSE.txt)
- 资源来源与分类清单见 [assets/README.md](assets/README.md)

### 自有资源
`assets/` 下的自有资源（如 `fonts/VonwaonBitmap-*.ttf`、`shader/`、`data/`）不受上述约束，为本项目所有。

### 非商用说明
本项目目前出于**学习与研究目的**复刻《破碎的像素地牢》。原版及复刻素材均为 GPLv3 许可，支持非商业使用与修改分发（需保持开源并保留版权声明）。如需商业发布，请勿直接使用原版资源，应重新绘制/制作自有素材。
