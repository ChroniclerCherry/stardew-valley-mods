using System.Collections.Generic;
using GreenhouseUpgrades.Upgrades;

namespace GreenhouseUpgrades.Data
{
    class Data
    {
        public Dictionary<UpgradeTypes, UpgradeData> UpgradesStatus { get; set; } = new Dictionary<UpgradeTypes, UpgradeData>();
        public int JunimoPoints { get; set; }
    }

    public class UpgradeData
    {
        public bool Unlocked { get; set; }
        public bool Active { get; set; }

    }
}
