using GFramework.Core.Abstractions.state;
using GFramework.Core.extensions;
using GFramework.Core.state;
using GFramework.Game.Abstractions.scene;
using GFramework.Game.Abstractions.ui;
using MyShatteredPixelDungeon.scripts.enums.scene;

namespace MyShatteredPixelDungeon.scripts.core.state.impls;

/// <summary>
///     游戏主状态，加载可玩的 Demo 场景
/// </summary>
public class GameState : ContextAwareStateBase
{
    public override void OnEnter(IState? from)
    {
        var uiRouter = this.GetSystem<IUiRouter>()!;
        var sceneRouter = this.GetSystem<ISceneRouter>()!;

        // 清除旧 UI
        uiRouter.Clear();

        // 加载可玩 Demo 场景
        sceneRouter.Replace(nameof(SceneKey.DemoScene));
    }

    public override bool CanTransitionTo(IState target) => true;
}