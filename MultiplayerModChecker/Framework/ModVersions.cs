using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace MultiplayerModChecker.Framework;

internal class ModVersions
{
    public string ModName { get; }
    public string ModUniqueId { get; }
    public ISemanticVersion? HostModVersion { get; }
    public ISemanticVersion? FarmhandModVersion { get; }

    [MemberNotNullWhen(true, nameof(HostModVersion))]
    public bool DoesHostHave => this.HostModVersion != null;

    [MemberNotNullWhen(true, nameof(FarmhandModVersion))]
    public bool DoesFarmhandHave => this.FarmhandModVersion != null;

    public ModVersions(string modUniqueId, string modName, ISemanticVersion? hostModVersion, ISemanticVersion? farmhandModVersion)
    {
        this.ModUniqueId = modUniqueId;
        this.ModName = modName;
        this.HostModVersion = hostModVersion;
        this.FarmhandModVersion = farmhandModVersion;
    }
}
