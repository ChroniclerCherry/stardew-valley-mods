using System;
using System.Collections.Generic;
using System.Linq;
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
    private bool OpenedNonCustomMenu;

    private Dictionary<string, CraftingStationConfig> TileCraftingStations;
    private Dictionary<string, CraftingStationConfig> CraftableCraftingStations;

    private List<string> CookingRecipesToRemove;
    private List<string> CraftingRecipesToRemove;

    private ModConfig Config;
    private Type CookingSkillMenu;

    private List<string> ReducedCookingRecipes { get; set; }
    private List<string> ReducedCraftingRecipes { get; set; }

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
        //register content packs
        this.RegisterContentPacks();

        //remove exclusive recipes
        this.ReducedCookingRecipes = new List<string>();
        this.ReducedCraftingRecipes = new List<string>();

        foreach (string recipeId in CraftingRecipe.craftingRecipes.Keys)
        {
            if (!this.CraftingRecipesToRemove.Contains(recipeId))
                this.ReducedCraftingRecipes.Add(recipeId);
        }

        foreach (string recipeId in CraftingRecipe.cookingRecipes.Keys)
        {
            if (!this.CookingRecipesToRemove.Contains(recipeId))
                this.ReducedCookingRecipes.Add(recipeId);
        }
    }

    /// <inheritdoc cref="IDisplayEvents.RenderingActiveMenu" />
    private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (this.OpenedNonCustomMenu)
        {
            return;
        }

        if (!MenuOverride) return;
        this.OpenedNonCustomMenu = true;

        IClickableMenu activeMenu = Game1.activeClickableMenu;

        IClickableMenu instance = activeMenu switch
        {
            CraftingPage => activeMenu,
            GameMenu gameMenu => gameMenu.pages[GameMenu.craftingTab],
            _ => activeMenu is not null && activeMenu.GetType() == this.CookingSkillMenu
                ? activeMenu
                : null
        };

        if (instance is not (null or CustomCraftingMenu)) this.OpenAndFixMenu(instance);
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu == null ||
            e.OldMenu is CustomCraftingMenu
            || e.OldMenu is CraftingPage
            || e.OldMenu is GameMenu
            || e.OldMenu.GetType() == this.CookingSkillMenu)
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
    }

    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.CanPlayerMove)
            return;

        if (!e.Button.IsActionButton())
            return;

        Vector2 grabTile = e.Cursor.GrabTile;

        Game1.currentLocation.Objects.TryGetValue(grabTile, out SObject obj);
        if (obj != null && obj.bigCraftable.Value)
        {
            if (this.CraftableCraftingStations.ContainsKey(obj.Name))
            {
                this.OpenCraftingMenu(this.CraftableCraftingStations[obj.Name], e.Cursor.GrabTile);
                this.Helper.Input.Suppress(e.Button);
                return;
            }
        }

        //No relevant BigCraftable found so check for tiledata
        string tileProperty =
            Game1.currentLocation.doesTileHaveProperty((int)grabTile.X, (int)grabTile.Y, "Action", "Buildings");
        if (tileProperty == null)
            return;

        string[] properties = tileProperty.Split(' ');
        if (properties[0] != "CraftingStation")
            return;

        if (this.TileCraftingStations.ContainsKey(properties[1]))
        {
            this.OpenCraftingMenu(this.TileCraftingStations[properties[1]], e.Cursor.GrabTile);
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private void RegisterContentPacks()
    {
        this.TileCraftingStations = new Dictionary<string, CraftingStationConfig>();
        this.CraftableCraftingStations = new Dictionary<string, CraftingStationConfig>();
        this.CookingRecipesToRemove = new List<string>();
        this.CraftingRecipesToRemove = new List<string>();

        foreach (IContentPack pack in this.Helper.ContentPacks.GetOwned())
        {
            if (!pack.HasFile("content.json"))
            {
                this.Monitor.Log($"{pack.Manifest.UniqueID} is missing a content.json", LogLevel.Error);
                continue;
            }

            ContentPack contentPack = pack.ModContent.Load<ContentPack>("content.json");

            this.RegisterCraftingStations(contentPack.CraftingStations);
        }
    }

    private void RegisterCraftingStations(List<CraftingStationConfig> craftingStations)
    {
        if (craftingStations == null)
            return;
        foreach (CraftingStationConfig station in craftingStations)
        {
            int numRecipes = station.CraftingRecipes.Count;
            for (int i = numRecipes - 1; i >= 0; i--)
            {
                if (!CraftingRecipe.craftingRecipes.Keys.Contains(station.CraftingRecipes[i]))
                {
                    this.Monitor.Log($"The recipe for {station.CraftingRecipes[i]} could not be found.");
                    station.CraftingRecipes.RemoveAt(i);
                }
            }

            numRecipes = station.CookingRecipes.Count;
            for (int i = numRecipes - 1; i >= 0; i--)
            {
                if (!CraftingRecipe.cookingRecipes.Keys.Contains(station.CookingRecipes[i]))
                {
                    this.Monitor.Log($"The recipe for {station.CookingRecipes[i]} could not be found.");
                    station.CookingRecipes.RemoveAt(i);
                }
            }

            if (station.ExclusiveRecipes)
            {
                this.CraftingRecipesToRemove.AddRange(station.CraftingRecipes);
                this.CookingRecipesToRemove.AddRange(station.CookingRecipes);
            }

            if (station.TileData != null)
            {
                if (this.TileCraftingStations.Keys.Contains(station.TileData))
                {
                    this.Monitor.Log(
                        $"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.",
                        LogLevel.Error);
                }
                else
                {
                    if (station.TileData != null) this.TileCraftingStations.Add(station.TileData, station);
                }
            }

            if (station.BigCraftable == null) continue;
            if (this.CraftableCraftingStations.Keys.Contains(station.BigCraftable))
            {
                this.Monitor.Log(
                    $"Multiple mods are trying to use the BigCraftable {station.BigCraftable}; Only one will be applied.",
                    LogLevel.Error);
            }
            else
            {
                if (station.BigCraftable != null) this.CraftableCraftingStations.Add(station.BigCraftable, station);
            }
        }
    }

    private void OpenCraftingMenu(CraftingStationConfig station, Vector2 grabTile)
    {
        List<IInventory> chests = this.GetChests(grabTile);

        Vector2 centeringOnScreen =
            Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2,
                600 + IClickableMenu.borderWidth * 2);

        Game1.activeClickableMenu = new CustomCraftingMenu((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, chests, station.CraftingRecipes, station.CookingRecipes);
    }

    private void OpenAndFixMenu(IClickableMenu instance)
    {
        bool isCooking = this.Helper.Reflection.GetField<bool>(instance, "cooking").GetValue();
        IReflectedMethod layoutRecipes = this.Helper.Reflection.GetMethod(instance, "layoutRecipes");

        var pagesOfCraftingRecipes = this.Helper.Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(instance,
                "pagesOfCraftingRecipes");
        pagesOfCraftingRecipes.SetValue(new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>());

        List<string> knownCraftingRecipes = this.ReducedCraftingRecipes.Where(recipe => Game1.player.craftingRecipes.ContainsKey(recipe)).ToList();

        layoutRecipes.Invoke(isCooking ? this.ReducedCookingRecipes : knownCraftingRecipes);
    }

    private List<IInventory> GetChests(Vector2 grabTile)
    {
        List<IInventory> chests = new List<IInventory>();

        IEnumerable<GameLocation> locations = Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations();

        if (this.Config.CraftFromFridgeWhenInHouse)
            if (Game1.currentLocation is FarmHouse house)
                chests.Add(house.fridge.Value.Items);

        int radius = this.Config.CraftingFromChestsRadius;
        if (radius == 0 && !this.Config.GlobalCraftFromChest)
            return chests;

        if (this.Config.GlobalCraftFromChest)
        {
            if (!this.Config.CraftFromFridgeWhenInHouse) //so we dont add this twice
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
            GameLocation location = Game1.currentLocation;

            for (int i = -radius; i < radius; i++)
            {
                for (int j = -radius; j < radius; j++)
                {
                    Vector2 tile = new(grabTile.X + i, grabTile.Y + j);
                    if (!location.objects.ContainsKey(tile)) continue;

                    SObject obj = location.objects[tile];
                    if (obj is Chest chest)
                        chests.Add(chest.Items);
                }
            }
        }

        return chests;

    }
}
