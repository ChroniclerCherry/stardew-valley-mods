using System.Collections.Generic;

namespace CustomCraftingStations.Framework;

internal class CraftingStationConfig
{
    public string BigCraftable { get; set; } //A big craftable to interact with to open the menu
    public string TileData { get; set; } //Name of the tiledata used to interact with to open the menu
    public bool ExclusiveRecipes { get; set; } = true; //Removes the listed recipes from the vanilla crafting menus
    public List<string> CraftingRecipes { get; set; } = new List<string>(); //list of recipe names
    public List<string> CookingRecipes { get; set; } = new List<string>();//list of recipe names
}
