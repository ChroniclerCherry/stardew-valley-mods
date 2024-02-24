using LimitedCampfireCooking.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LimitedCampfireCooking
{
    class ModEntry : Mod
    {
        internal static ModConfig Config;
        internal ICCSApi CCS;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CCS = Helper.ModRegistry.GetApi<ICCSApi>("Cherry.CustomCraftingStations");
            if (CCS != null)
                Monitor.Log("Custom Crafting Station detected. Compatibility patch added.",LogLevel.Info);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            AllCookingRecipes = CraftingRecipe.cookingRecipes;
            LimitedCookingRecipes = AllCookingRecipes;

            if (!Config.EnableAllCookingRecipies)
            {
                LimitedCookingRecipes = new Dictionary<string, string>();
                foreach (var kvp in from KeyValuePair<string, string> kvp in AllCookingRecipes
                                    where Config.Recipes.Contains(kvp.Key)
                                    select kvp)
                {
                    LimitedCookingRecipes.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private Dictionary<string, string> AllCookingRecipes;
        private Dictionary<string, string> LimitedCookingRecipes;

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsWorldReady &&
                Game1.currentLocation != null &&
                Game1.activeClickableMenu == null)
            {
                if (Constants.TargetPlatform == GamePlatform.Android)
                {
                    if (e.Button != SButton.MouseLeft)
                        return;
                    if (e.Cursor.GrabTile != e.Cursor.Tile)
                        return;
                }
                else if (!e.Button.IsUseToolButton())
                    return;

                GameLocation loc = Game1.currentLocation;

                Vector2 tile = Helper.Input.GetCursorPosition().GrabTile;
                loc.Objects.TryGetValue(tile, out StardewValley.Object obj);

                if (obj != null && obj.Name == "Campfire" && obj.IsOn)
                {
                    Helper.Input.Suppress(e.Button);
                    Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
                    
                    CCS?.SetCCSCraftingMenuOverride(false);
                    CraftingRecipe.cookingRecipes = LimitedCookingRecipes;
                    Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
                    CraftingRecipe.cookingRecipes = AllCookingRecipes;
                }
            }
        }
    }
}
