namespace CustomCraftingStations.Framework;

/// <summary>The mod settings model.</summary>
internal class ModConfig
{
    /// <summary>Whether crafting will pull ingredients from all chests everywhere. This overrides <see cref="CraftingFromChestsRadius" /> if true.</summary>
    public bool GlobalCraftFromChest { get; set; } = false;

    /// <summary>Whether crafting will pull ingredients from the fridge when in a farmhouse.</summary>
    public bool CraftFromFridgeWhenInHouse { get; set; } = true;

    /// <summary>The tile radius around the station from which to pull ingredients from chests. A value of 1 matches vanilla workbench behavior (i.e. chests must be directly adjacent).</summary>
    public int CraftingFromChestsRadius { get; set; } = 0;
}
