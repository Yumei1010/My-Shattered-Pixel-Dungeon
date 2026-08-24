namespace GFrameworkTemplate.scripts.component.state_machine;

/// <summary>
///     状态契约
/// </summary>
public interface IState
{
    /// <summary>
    ///     每帧更新时调用的方法
    /// </summary>
    void Process(double delta);

    /// <summary>
    ///     进入状态时调用的方法
    /// </summary>
    void Enter();

    /// <summary>
    ///     退出状态时调用的方法
    /// </summary>
    void Exit();
}