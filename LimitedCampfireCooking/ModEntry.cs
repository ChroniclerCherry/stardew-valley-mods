using System.Collections.Generic;
using System.Linq;
using LimitedCampfireCooking.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace LimitedCampfireCooking;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private static ModConfig Config;
    private ICustomCraftingStationsApi CustomCraftingStations;

    private Dictionary<string, string> AllCookingRecipes;
    private Dictionary<string, string> LimitedCookingRecipes;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.CustomCraftingStations = this.Helper.ModRegistry.GetApi<ICustomCraftingStationsApi>("Cherry.CustomCraftingStations");
        if (this.CustomCraftingStations != null)
            this.Monitor.Log("Custom Crafting Station detected. Compatibility patch added.", LogLevel.Info);
    }

    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.AllCookingRecipes = CraftingRecipe.cookingRecipes;
        this.LimitedCookingRecipes = this.AllCookingRecipes;

        if (!Config.EnableAllCookingRecipes)
        {
            this.LimitedCookingRecipes = new Dictionary<string, string>();
            foreach ((string key, string recipe) in this.AllCookingRecipes)
            {
                if (Config.Recipes.Contains(key))
                    this.LimitedCookingRecipes.Add(key, recipe);
            }
        }
    }

    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
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
            loc.Objects.TryGetValue(tile, out Object obj);

            if (obj is { Name: "Campfire", IsOn: true })
            {
                this.Helper.Input.Suppress(e.Button);
                Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);

                this.CustomCraftingStations?.SetCCSCraftingMenuOverride(false);
                CraftingRecipe.cookingRecipes = this.LimitedCookingRecipes;
                Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
                CraftingRecipe.cookingRecipes = this.AllCookingRecipes;
            }
        }
    }
}
