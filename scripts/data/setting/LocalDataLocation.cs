using GFramework.Game.Abstractions.data;
using GFramework.Game.Abstractions.enums;

namespace GFrameworkTemplate.scripts.data.setting;

public sealed record LocalDataLocation : IDataLocation
{
    public string Key { get; init; } = "local";
    public StorageKinds Kinds { get; init; } = StorageKinds.Local;
    public string? Namespace { get; init; } = "";
    public IReadOnlyDictionary<string, string>? Metadata { get; init; } = null;
}
