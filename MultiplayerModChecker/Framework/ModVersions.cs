using StardewModdingAPI;

namespace MultiplayerModChecker.Framework;

internal class ModVersions
{
    public string ModName { get; set; }

    public string ModUniqueId { get; set; }
    public bool DoesHostHave { get; set; } = false;
    public bool DoesFarmhandHave { get; set; } = false;
    public ISemanticVersion HostModVersion { get; set; }
    public ISemanticVersion FarmhandModVersion { get; set; }
}
