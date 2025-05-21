using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace MultiplayerModChecker.Framework;

internal class SmapiGameVersionDifference
{
    public ISemanticVersion HostSmapiVersion { get; }
    public ISemanticVersion? FarmhandSmapiVersion { get; }

    [MemberNotNullWhen(true, nameof(FarmhandSmapiVersion))]
    public bool FarmhandHasSmapi => this.FarmhandSmapiVersion != null;

    public SmapiGameVersionDifference(ISemanticVersion hostSmapiVersion, ISemanticVersion? farmhandSmapiVersion)
    {
        this.HostSmapiVersion = hostSmapiVersion;
        this.FarmhandSmapiVersion = farmhandSmapiVersion;
    }
}
