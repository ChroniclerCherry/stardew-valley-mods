using BetterGreenhouse.Upgrades;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetterGreenhouse.Data
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
