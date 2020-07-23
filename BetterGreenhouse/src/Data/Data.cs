using BetterGreenhouse.Upgrades;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetterGreenhouse.Data
{
    class Data
    {
        public Dictionary<UpgradeTypes, bool> UpgradesStatus { get; set; } = new Dictionary<UpgradeTypes, bool>();
        public int JunimoPoints { get; set; }
    }
}
