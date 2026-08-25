using GFramework.Core.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using MyShatteredPixelDungeon.scripts.core;
using Godot;

namespace MyShatteredPixelDungeon.scripts.ui.game_log;

/// <summary>
///     游戏消息日志，显示在屏幕左下角
///     对应原版 com.shatteredpixel.shatteredpixeldungeon.ui.GameLog
/// </summary>
[Log]
[ContextAware]
public partial class GameLog : Control
{
    /// <summary>消息显示控件</summary>
    private RichTextLabel MessageLog => GetNode<RichTextLabel>("%MessageLog");

    /// <summary>最大消息数</summary>
    private const int MaxMessages = 50;

    public override void _Ready()
    {
        MessageLog.BbcodeEnabled = true;
        MessageLog.ScrollActive = false;
    }

    /// <summary>
    ///     添加消息
    /// </summary>
    public void AddMessage(string text, Color color)
    {
        var lines = MessageLog.Text.Split('\n');
        if (lines.Length >= MaxMessages)
        {
            // 移除最旧的消息
            MessageLog.Text = string.Join('\n', lines[^MaxMessages..]);
        }

        MessageLog.Text += $"[color=#{color.ToHtml()}]{text}[/color]\n";
        MessageLog.ScrollToLine(MessageLog.GetLineCount() - 1);
    }

    /// <summary>
    ///     添加普通消息（白色）
    /// </summary>
    public void Info(string text) => AddMessage(text, Colors.White);

    /// <summary>
    ///     添加警告消息（黄色）
    /// </summary>
    public void Warn(string text) => AddMessage(text, Colors.Yellow);

    /// <summary>
    ///     添加危险消息（红色）
    /// </summary>
    public void Danger(string text) => AddMessage(text, Colors.Red);

    /// <summary>
    ///     添加物品消息（青色）
    /// </summary>
    public void Item(string text) => AddMessage(text, Colors.Cyan);

    /// <summary>
    ///     清空日志
    /// </summary>
    public void Clear() => MessageLog.Text = "";
}