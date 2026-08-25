using GFramework.Core.model;

namespace MyShatteredPixelDungeon.scripts.model;

/// <summary>
///     物品数据模型，负责管理游戏中的物品实例
///     持有物品生成器状态、已识别物品列表等全局数据
/// </summary>
public sealed class ItemModel : AbstractModel
{
    protected override void OnInit() { }

    /// <summary>已获得的物品实例（用于追踪收集统计）</summary>
    public HashSet<string> IdentifiedItems { get; } = new();

    /// <summary>已鉴定的物品类型</summary>
    public HashSet<string> DiscoveredItems { get; } = new();

    /// <summary>药水颜色映射（新游戏随机分配）</summary>
    public Dictionary<string, int> PotionColorMap { get; set; } = new();

    /// <summary>卷轴名称映射（新游戏随机分配）</summary>
    public Dictionary<string, int> ScrollLabelMap { get; set; } = new();

    /// <summary>戒指宝石映射（新游戏随机分配）</summary>
    public Dictionary<string, int> RingGemMap { get; set; } = new();

    /// <summary>
    ///     记录物品发现
    /// </summary>
    public void RecordDiscovery(string itemType)
    {
        IdentifiedItems.Add(itemType);
    }

    /// <summary>
    ///     记录物品鉴定
    /// </summary>
    public void RecordIdentification(string itemType)
    {
        DiscoveredItems.Add(itemType);
    }

    /// <summary>
    ///     重置所有识别状态（新游戏）
    /// </summary>
    public void ResetIdentification()
    {
        PotionColorMap.Clear();
        ScrollLabelMap.Clear();
        RingGemMap.Clear();
        IdentifiedItems.Clear();
        DiscoveredItems.Clear();
    }
}