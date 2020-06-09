using System.Collections.Generic;

namespace CustomCraftingStation
{
    public class ContentPack
    {
        public List<CraftingStation> CookingStations;
        public List<CraftingStation> CraftingStations;
    }

    public class CraftingStation
    {
        public string BigCraftable { get; set; } //A big craftable to interact with to open the menu
        public string TileData { get; set; } //Name of the tiledata used to interact with to open the menu
        public bool ExclusiveRecipes { get; set; } = true; //Removes the listed recipes from the vanilla crafting menus
        public string[] Recipes { get; set; } //list of recipe names

    }
}
