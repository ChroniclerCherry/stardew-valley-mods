using System;
using System.Linq;
using GreenhouseUpgrades.Upgrades;

namespace GreenhouseUpgrades
{
    class Utils
    {
        public static Upgrade GetUpgradeByName(string UpgradeName, bool CaseSensitive = false)
        {
            return Main.Upgrades.FirstOrDefault(u => u.Name.Equals(UpgradeName, CaseSensitive? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }
    }
}
