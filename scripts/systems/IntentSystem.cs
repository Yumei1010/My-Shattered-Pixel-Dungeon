using GFramework.Core.system;
using GFramework.Core.extensions;
using MyShatteredPixelDungeon.scripts.core.intent;
using MyShatteredPixelDungeon.scripts.cqrs.intent.@event;
using MyShatteredPixelDungeon.scripts.entities;

namespace MyShatteredPixelDungeon.scripts.systems;

/// <summary>
///     意图系统，监听 IntentGeneratedEvent 并解析为命令执行
/// </summary>
public sealed class IntentSystem : AbstractSystem
{
    protected override void OnInit()
    {
        this.RegisterEvent<IntentGeneratedEvent>(OnIntentGenerated);
    }

    private void OnIntentGenerated(IntentGeneratedEvent e)
    {
        if (e.Intent is not Intent intent) return;

        // 获取英雄实体（简化：从 Actor 静态类查找）
        var hero = Actor.All().OfType<HeroEntity>().FirstOrDefault();
        if (hero == null) return;

        // 解析意图为命令序列
        var commands = IntentInterpreter.Interpret(intent, hero, null);

        // 执行每个命令
        foreach (var cmd in commands)
        {
            this.SendCommand(cmd);
        }
    }
}