using StardewModdingAPI;
using System.Collections.Generic;

namespace CustomCraftingStation
{
    public partial class ModEntry : Mod
    {
        private Dictionary<string, CraftingStation> _tileCraftingStations;
        private Dictionary<string, CraftingStation> _craftableCraftingStations;

        private List<string> _cookingRecipesToRemove;
        private List<string> _craftingRecipesToRemove;

        public override void Entry(IModHelper helper)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                Monitor.Log("Custom Crafting Stations does not currently support Android.",LogLevel.Error);
                return;
            }


            Helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
            Helper.Events.Display.MenuChanged += Display_MenuChanged;

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }
    }
}
