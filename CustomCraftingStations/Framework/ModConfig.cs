namespace CustomCraftingStations.Framework;

internal class ModConfig
{
    public bool GlobalCraftFromChest { get; set; } = false;
    public bool CraftFromFridgeWhenInHouse { get; set; } = true;
    public int CraftingFromChestsRadius { get; set; } = 0;
}
