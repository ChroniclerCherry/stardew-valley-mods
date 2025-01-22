using System.Collections.Generic;

namespace ToolUpgradeCosts.Framework;

internal class ModConfig
{
    public Dictionary<UpgradeMaterials, Upgrade> UpgradeCosts { get; set; } = new()
    {
        [UpgradeMaterials.Copper] = new Upgrade
        {
            Cost = 2000,
            MaterialName = "Copper Bar",
            MaterialStack = 5
        },
        [UpgradeMaterials.Steel] = new Upgrade
        {
            Cost = 5000,
            MaterialName = "Iron Bar",
            MaterialStack = 5
        },
        [UpgradeMaterials.Gold] = new Upgrade
        {
            Cost = 10000,
            MaterialName = "Gold Bar",
            MaterialStack = 5
        },
        [UpgradeMaterials.Iridium] = new Upgrade
        {
            Cost = 25000,
            MaterialName = "Iridium Bar",
            MaterialStack = 5
        }
    };
}
