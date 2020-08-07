using System.Collections.Generic;
using BetterGreenhouse.Upgrades;
using Microsoft.Xna.Framework;

namespace BetterGreenhouse.data
{
    public class Config
    {
        public bool EnableDebugging { get; set; } = false;

        public Dictionary<UpgradeTypes, int> UpgradeCosts = new Dictionary<UpgradeTypes, int>()
        {
            {UpgradeTypes.SizeUpgrade,50000},
            {UpgradeTypes.AutoWaterUpgrade,50000},
            {UpgradeTypes.AutoHarvestUpgrade,50000}
        };

        public Vector2 JojaMartUpgradeCoordinates { get; set; } = new Vector2(22,25);
        public Vector2 CommunityCenterUpgradeCoordinates { get; set; } = new Vector2(14, 4);
    }
}
