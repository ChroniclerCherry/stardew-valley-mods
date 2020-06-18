using System;
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

        //private IRemoteFridgeApi remoteFridgeApi { get; set; }

        public override void Entry(IModHelper helper)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                Monitor.Log("Custom Crafting Stations does not currently support Android.",LogLevel.Error);
                return;
            }

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
            Helper.Events.Display.MenuChanged += Display_MenuChanged;

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        public Type CookingSkillMenu;

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            CookingSkillMenu = Type.GetType("CookingSkill.NewCraftingPage, CookingSkill");
        }
    }
}
