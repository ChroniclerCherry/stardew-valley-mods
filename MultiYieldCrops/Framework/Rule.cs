namespace MultiYieldCrops.Framework;

class Rule
{
    public string ExtraYieldItemType = "Object";
    public string ItemName;
    public int minHarvest = 1;
    public int maxHarvest = 1;
    public float maxHarvestIncreasePerFarmingLevel = 0;
    public string[] disableWithMods = null;
}
