namespace MultiYieldCrops.Framework;

internal class Rule
{
    public string ExtraYieldItemType = "Object";
    public string ItemName;
    public int MinHarvest = 1;
    public int MaxHarvest = 1;
    public float MaxHarvestIncreasePerFarmingLevel = 0;
    public string[] DisableWithMods = null;
}
