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

        private Config _config;

        public override void Entry(IModHelper helper)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                Monitor.Log("Custom Crafting Stations does not currently support Android.",LogLevel.Error);
                return;
            }

            _config = Helper.ReadConfig<Config>();

            if (!_config.DiableRemoveRecipesFromVanillaMenus)
            {
                Helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
                Helper.Events.Display.MenuChanged += Display_MenuChanged;
            }

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            /*TODO: if ever another mod comes out that uses the same method to make custom crafting pages,
             a patch may be necessary to run first to avoid overriding other mod changes
             CraftingPagePatch.Initialize(Monitor,this);
             */
        }
    }

    public class Config
    {
        public bool DiableRemoveRecipesFromVanillaMenus { get; set; }
    }
}
