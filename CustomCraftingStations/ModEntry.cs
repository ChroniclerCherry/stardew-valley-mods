using System;
using System.Collections.Generic;
using System.Linq;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using CustomCraftingStations.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CustomCraftingStations;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages content loaded from Custom Crafting Stations content packs.</summary>
    private ContentManager ContentManager;

    private bool OpenedNonCustomMenu;

    /// <summary>The mod settings.</summary>
    private ModConfig Config;

    private Type CookingSkillMenu;

    public static bool MenuOverride = true;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        if (Constants.TargetPlatform == GamePlatform.Android)
        {
            this.Monitor.Log("Custom Crafting Stations does not currently support Android.", LogLevel.Error);
            return;
        }

        I18n.Init(helper.Translation);
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        helper.Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new CustomCraftingStationsApi();
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.ContentManager = new ContentManager(this.Helper.ContentPacks.GetOwned(), this.Monitor);
    }

    /// <inheritdoc cref="IDisplayEvents.RenderingActiveMenu" />
    private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        if (!Context.IsWorldReady || this.OpenedNonCustomMenu || !MenuOverride)
            return;

        this.OpenedNonCustomMenu = true;

        IClickableMenu activeMenu = Game1.activeClickableMenu;

        IClickableMenu instance = activeMenu switch
        {
            CraftingPage => activeMenu,
            GameMenu gameMenu => gameMenu.pages[GameMenu.craftingTab],
            _ => activeMenu?.GetType() == this.CookingSkillMenu
                ? activeMenu
                : null
        };

        if (instance is not (null or CustomCraftingMenu))
            this.OpenAndFixMenu(instance);
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is (null or CustomCraftingMenu or CraftingPage or GameMenu) || e.OldMenu.GetType() == this.CookingSkillMenu)
        {
            this.OpenedNonCustomMenu = false;
            if (e.NewMenu == null)
                MenuOverride = true;
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.CookingSkillMenu = Type.GetType("CookingSkill.NewCraftingPage, CookingSkill");

        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForCustomCraftingStations(),
            get: () => this.Config,
            set: config => this.Config = config
        );
    }

    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.CanPlayerMove || !e.Button.IsActionButton())
            return;

        // get tile position
        Vector2 grabTile = e.Cursor.GrabTile;
        int tileX = (int)grabTile.X;
        int tileY = (int)grabTile.Y;

        // check object stations
        if (Game1.currentLocation.Objects.TryGetValue(grabTile, out SObject obj) && obj.bigCraftable.Value)
        {
            if (this.ContentManager.CraftableCraftingStations.TryGetValue(obj.Name, out CraftingStationConfig config))
            {
                this.OpenCraftingMenu(config, e.Cursor.GrabTile);
                this.Helper.Input.Suppress(e.Button);
                return;
            }
        }

        // check tile property stations
        if (Game1.currentLocation.doesTileHaveProperty(tileX, tileY, "Action", "Buildings")?.Split(' ', 2) is ["CraftingStation", not null] property)
        {
            if (this.ContentManager.TileCraftingStations.TryGetValue(property[1], out CraftingStationConfig config))
            {
                this.OpenCraftingMenu(config, e.Cursor.GrabTile);
                this.Helper.Input.Suppress(e.Button);
            }
        }
    }

    private void OpenCraftingMenu(CraftingStationConfig station, Vector2 grabTile)
    {
        List<IInventory> chests = this.GetChests(grabTile);

        Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);

        bool isCooking = station.CookingRecipes.Count > 0;
        var recipes = isCooking
            ? station.CookingRecipes
            : station.CraftingRecipes;

        Game1.activeClickableMenu = new CustomCraftingMenu((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, chests, recipes, isCooking);
    }

    private void OpenAndFixMenu(IClickableMenu instance)
    {
        bool isCooking = this.Helper.Reflection.GetField<bool>(instance, "cooking").GetValue();
        IReflectedMethod layoutRecipes = this.Helper.Reflection.GetMethod(instance, "layoutRecipes");

        var pagesOfCraftingRecipes = this.Helper.Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(instance, "pagesOfCraftingRecipes");
        pagesOfCraftingRecipes.SetValue(new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>());

        List<string> knownRecipes = isCooking
            ? this.ContentManager.DefaultCookingRecipes.ToList()
            : Game1.player.craftingRecipes.Keys.Where(this.ContentManager.DefaultCraftingRecipes.Contains).ToList();

        layoutRecipes.Invoke(knownRecipes);
    }

    private List<IInventory> GetChests(Vector2 grabTile)
    {
        List<IInventory> chests = new List<IInventory>();
        IEnumerable<GameLocation> locations = Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations();

        // get fridge
        if (this.Config.CraftFromFridgeWhenInHouse && Game1.currentLocation is FarmHouse house)
            chests.Add(house.fridge.Value.Items);

        // get chests in radius
        if (this.Config.GlobalCraftFromChest)
        {
            if (!this.Config.CraftFromFridgeWhenInHouse) // so we don't add this twice
                chests.Add((Game1.getLocationFromName("FarmHouse") as FarmHouse)?.fridge.Value.Items);

            foreach (GameLocation location in locations)
            {
                foreach (SObject obj in location.objects.Values)
                {
                    if (obj is Chest chest)
                        chests.Add(chest.Items);
                }
            }
        }
        else
        {
            int radius = this.Config.CraftingFromChestsRadius;
            if (radius > 0)
            {
                GameLocation location = Game1.currentLocation;

                for (int x = -radius; x < radius; x++)
                {
                    for (int y = -radius; y < radius; y++)
                    {
                        Vector2 tile = new(grabTile.X + x, grabTile.Y + y);
                        if (!location.objects.ContainsKey(tile)) continue;

                        if (location.objects[tile] is Chest chest)
                            chests.Add(chest.Items);
                    }
                }
            }
        }

        return chests;
    }
}
