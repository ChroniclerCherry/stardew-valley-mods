using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterGreenhouse.Upgrades;

namespace BetterGreenhouse.src
{
    class Utils
    {
        public static Upgrade GetUpgradeByName(string UpgradeName, bool CaseSensitive = false)
        {
            return State.Upgrades.FirstOrDefault(u => u.Name.Equals(UpgradeName, CaseSensitive? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }
    }
}
