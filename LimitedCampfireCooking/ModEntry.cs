using System.Collections.Generic;
using System.Linq;
using LimitedCampfireCooking.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace LimitedCampfireCooking;

class ModEntry : Mod
{
    internal static ModConfig Config;
    internal ICustomCraftingStationsApi CustomCraftingStations;

    public override void Entry(IModHelper helper)
    {
        Config = this.Helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
        helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
    }

    private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.CustomCraftingStations = this.Helper.ModRegistry.GetApi<ICustomCraftingStationsApi>("Cherry.CustomCraftingStations");
        if (this.CustomCraftingStations != null)
            this.Monitor.Log("Custom Crafting Station detected. Compatibility patch added.", LogLevel.Info);
    }

    private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.AllCookingRecipes = CraftingRecipe.cookingRecipes;
        this.LimitedCookingRecipes = this.AllCookingRecipes;

        if (!Config.EnableAllCookingRecipies)
        {
            this.LimitedCookingRecipes = new Dictionary<string, string>();
            foreach (var kvp in from KeyValuePair<string, string> kvp in this.AllCookingRecipes
                                where Config.Recipes.Contains(kvp.Key)
                                select kvp)
            {
                this.LimitedCookingRecipes.Add(kvp.Key, kvp.Value);
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

            Vector2 tile = this.Helper.Input.GetCursorPosition().GrabTile;
            loc.Objects.TryGetValue(tile, out StardewValley.Object obj);

            if (obj != null && obj.Name == "Campfire" && obj.IsOn)
            {
                this.Helper.Input.Suppress(e.Button);
                Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);

                this.CustomCraftingStations?.SetCCSCraftingMenuOverride(false);
                CraftingRecipe.cookingRecipes = this.LimitedCookingRecipes;
                Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
                CraftingRecipe.cookingRecipes = this.AllCookingRecipes;
            }
        }
    }
}
