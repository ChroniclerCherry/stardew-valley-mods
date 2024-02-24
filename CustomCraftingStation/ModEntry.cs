using System;
using System.Linq;
using System.Collections.Generic;
using CustomCraftingStation.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomCraftingStation
{
    public class ModEntry : Mod
    {
        private bool _openedNonCustomMenu;

        private Dictionary<string, CraftingStationConfig> _tileCraftingStations;
        private Dictionary<string, CraftingStationConfig> _craftableCraftingStations;

        private List<string> _cookingRecipesToRemove;
        private List<string> _craftingRecipesToRemove;

        private Config _config;
        public Type CookingSkillMenu;

        public List<string> ReducedCookingRecipes { get; set; }
        public List<string> ReducedCraftingRecipes { get; set; }

        public static bool MenuOverride = true;

        public override void Entry(IModHelper helper)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                Monitor.Log("Custom Crafting Stations does not currently support Android.",LogLevel.Error);
                return;
            }

            _config = Helper.ReadConfig<Config>();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
            Helper.Events.Display.MenuChanged += Display_MenuChanged;

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        public override object GetApi()
        {
            return new CCSApi();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            //register content packs
            RegisterContentPacks();

            //remove exclusive recipes
            ReducedCookingRecipes = new List<string>();
            ReducedCraftingRecipes = new List<string>();

            foreach (var recipe in CraftingRecipe.craftingRecipes.Where(recipe => !_craftingRecipesToRemove.Contains(recipe.Key)))
            {
                ReducedCraftingRecipes.Add(recipe.Key);
            }

            foreach (var recipe in CraftingRecipe.cookingRecipes.Where(recipe => !_cookingRecipesToRemove.Contains(recipe.Key)))
            {
                ReducedCookingRecipes.Add(recipe.Key);
            }
        }

        private void Display_RenderingActiveMenu(object sender, StardewModdingAPI.Events.RenderingActiveMenuEventArgs e)
        {

            if (!Context.IsWorldReady)
                return;

            if (_openedNonCustomMenu)
            {
                return;
            }

            if (!MenuOverride) return;
            _openedNonCustomMenu = true;

            var activeMenu = Game1.activeClickableMenu;
            if (activeMenu == null)
                return;

            IClickableMenu instance;



            if (activeMenu is CraftingPage)
                instance = activeMenu;
            else if (activeMenu is GameMenu gameMenu)
                instance = gameMenu.pages[GameMenu.craftingTab];
            else if (activeMenu.GetType() == CookingSkillMenu)
                instance = activeMenu;
            else
                return;
            OpenAndFixMenu(instance);
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.OldMenu == null ||
                e.OldMenu is CustomCraftingMenu
                || e.OldMenu is CraftingPage
                || e.OldMenu is GameMenu
                || e.OldMenu.GetType() == CookingSkillMenu)
            {
                _openedNonCustomMenu = false;
                if (e.NewMenu == null)
                    MenuOverride = true;
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            CookingSkillMenu = Type.GetType("CookingSkill.NewCraftingPage, CookingSkill");
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (!e.Button.IsActionButton())
                return;

            Vector2 grabTile = e.Cursor.GrabTile;

            Game1.currentLocation.Objects.TryGetValue(grabTile, out var obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (_craftableCraftingStations.ContainsKey(obj.Name))
                {
                    OpenCraftingMenu(_craftableCraftingStations[obj.Name], e.Cursor.GrabTile);
                    Helper.Input.Suppress(e.Button);
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

            if (_tileCraftingStations.ContainsKey(properties[1]))
            {
                OpenCraftingMenu(_tileCraftingStations[properties[1]], e.Cursor.GrabTile);
                Helper.Input.Suppress(e.Button);
            }
        }

        private void RegisterContentPacks()
        {
            var packs = Helper.ContentPacks.GetOwned();

            _tileCraftingStations = new Dictionary<string, CraftingStationConfig>();
            _craftableCraftingStations = new Dictionary<string, CraftingStationConfig>();
            _cookingRecipesToRemove = new List<string>();
            _craftingRecipesToRemove = new List<string>();

            foreach (var pack in packs)
            {
                if (!pack.HasFile("content.json"))
                {
                    Monitor.Log($"{pack.Manifest.UniqueID} is missing a content.json", LogLevel.Error);
                    continue;
                }

                ContentPack contentPack = pack.ModContent.Load<ContentPack>("content.json");

                RegisterCraftingStations(contentPack.CraftingStations);
            }
        }

        private void RegisterCraftingStations(List<CraftingStationConfig> craftingStations)
        {
            if (craftingStations == null)
                return;
            foreach (var station in craftingStations)
            {

                int numRecipes = station.CraftingRecipes.Count;
                for (int i = numRecipes - 1; i >= 0; i--)
                {
                    if (!CraftingRecipe.craftingRecipes.Keys.Contains(station.CraftingRecipes[i]))
                    {
                        Monitor.Log($"The recipe for {station.CraftingRecipes[i]} could not be found.");
                        station.CraftingRecipes.RemoveAt(i);
                    }
                }

                numRecipes = station.CookingRecipes.Count;
                for (int i = numRecipes - 1; i >= 0; i--)
                {
                    if (!CraftingRecipe.cookingRecipes.Keys.Contains(station.CookingRecipes[i]))
                    {
                        Monitor.Log($"The recipe for {station.CookingRecipes[i]} could not be found.");
                        station.CookingRecipes.RemoveAt(i);
                    }
                }

                if (station.ExclusiveRecipes)
                {
                    _craftingRecipesToRemove.AddRange(station.CraftingRecipes);
                    _cookingRecipesToRemove.AddRange(station.CookingRecipes);
                }

                if (station.TileData != null)
                {
                    if (_tileCraftingStations.Keys.Contains(station.TileData))
                    {
                        Monitor.Log(
                            $"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.",
                            LogLevel.Error);
                    }
                    else
                    {
                        if (station.TileData != null)
                            _tileCraftingStations.Add(station.TileData, station);
                    }
                }

                if (station.BigCraftable == null) continue;
                if (_craftableCraftingStations.Keys.Contains(station.BigCraftable))
                {
                    Monitor.Log(
                        $"Multiple mods are trying to use the BigCraftable {station.BigCraftable}; Only one will be applied.",
                        LogLevel.Error);
                }
                else
                {
                    if (station.BigCraftable != null)
                        _craftableCraftingStations.Add(station.BigCraftable, station);
                }
            }
        }

        public void OpenCraftingMenu(CraftingStationConfig station, Vector2 grabTile)
        {
            List<Chest> Chests = GetChests(grabTile);

            Vector2 centeringOnScreen =
                Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2,
                    600 + IClickableMenu.borderWidth * 2);

            var menu = new CustomCraftingMenu((int)centeringOnScreen.X, (int)centeringOnScreen.Y,
                800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, Chests, station.CraftingRecipes, station.CookingRecipes);

            Game1.activeClickableMenu = menu;
        }

        private void OpenAndFixMenu(IClickableMenu instance)
        {
            var isCooking = Helper.Reflection.GetField<bool>(instance, "cooking").GetValue();
            var layoutRecipes = Helper.Reflection.GetMethod(instance, "layoutRecipes");

            var pagesOfCraftingRecipes =
                Helper.Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(instance,
                    "pagesOfCraftingRecipes");
            pagesOfCraftingRecipes.SetValue(new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>());

            List<string> knownCraftingRecipes =
                ReducedCraftingRecipes.Where(recipe => Game1.player.craftingRecipes.ContainsKey(recipe)).ToList();

            layoutRecipes.Invoke(isCooking ? ReducedCookingRecipes : knownCraftingRecipes);
        }

        public List<Chest> GetChests(Vector2 grabTile)
        {
            List<Chest> chests = new List<Chest>();

            IEnumerable<GameLocation> locs;
            locs = Context.IsMainPlayer ? Game1.locations : Helper.Multiplayer.GetActiveLocations();

            if (_config.CraftFromFridgeWhenInHouse)
                if (Game1.currentLocation is FarmHouse house)
                    chests.Add(house.fridge.Value);

            int radius = _config.CraftingFromChestsRadius;
            if (radius == 0 && !_config.GlobalCraftFromChest)
                return chests;

            if (_config.GlobalCraftFromChest)
            {
                if (!_config.CraftFromFridgeWhenInHouse) //so we dont add this twice
                    chests.Add((Game1.getLocationFromName("FarmHouse") as FarmHouse)?.fridge.Value);

                foreach (var location in locs)
                {
                    foreach (var objs in location.objects)
                    {
                        foreach (var obj in objs)
                        {
                            if (obj.Value is Chest chest)
                                chests.Add(chest);
                        }
                    }
                }
            }
            else
            {
                var loc = Game1.currentLocation;

                for (int i = -radius; i < radius; i++)
                {
                    for (int j = -radius; j < radius; j++)
                    {
                        var tile = new Vector2(grabTile.X + i, grabTile.Y + j);
                        if (!loc.objects.ContainsKey(tile)) continue;

                        var obj = loc.objects[tile];
                        if (obj != null && obj is Chest chest)
                            chests.Add(chest);
                    }
                }
            }

            return chests;

        }
    }
}
