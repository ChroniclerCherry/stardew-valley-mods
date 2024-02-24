using StardewModdingAPI;

namespace MultiplayerModChecker.Framework
{
    public class SmapiGameVersionDifference
    {
        public bool FarmhandHasSmapi { get; set; }
        public ISemanticVersion HostSmapiVersion { get; set; }
        public ISemanticVersion FarmhandSmapiVersion { get; set; }
    }
}
