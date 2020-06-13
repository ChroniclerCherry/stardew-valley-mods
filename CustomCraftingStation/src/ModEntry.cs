using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomCraftingStation
{
    public class ModEntry : Mod
    {
        public Dictionary<string, string> AllCookingRecipes;
        public Dictionary<string, string> AllCraftingRecipes;

        public Dictionary<string, string> ReducedCookingRecipes;
        public Dictionary<string, string> ReducedCraftingRecipes;

        private Dictionary<string, string[]> _tileCookingStations;
        private Dictionary<string, string[]> _craftableCookingStations;

        private Dictionary<string, string[]> _tileCraftingStations;
        private Dictionary<string, string[]> _craftableCraftingStations;

        private List<string> _cookingRecipesToRemove;
        private List<string> _craftingRecipesToRemove;

        public bool OpenedModdedStation { get; set; } = false;

        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            PatchCraftingpage.Initialize(Monitor, this);
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                if (e.Button != SButton.MouseLeft)
                    return;
                if (e.Cursor.GrabTile != e.Cursor.Tile)
                    return;
            }
            else if (!e.Button.IsActionButton())
                return;

            Vector2 grabTile = e.Cursor.GrabTile;

            Game1.currentLocation.Objects.TryGetValue(grabTile, out var obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (_craftableCookingStations.ContainsKey(obj.Name)){
                    OpenCookingMenu(_craftableCookingStations[obj.Name]);
                    Helper.Input.Suppress(e.Button);
                    return;
                } else if (_craftableCraftingStations.ContainsKey(obj.Name))
                {
                    OpenCraftingMenu(_craftableCraftingStations[obj.Name]);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
            }

            //No relevant BigCraftable found so check for tiledata
            string tileProperty = Game1.currentLocation.doesTileHaveProperty((int)grabTile.X, (int)grabTile.Y, "Action", "Buildings");
            if (tileProperty == null)
                return;

            string[] properties = tileProperty.Split(' ');
            if (properties[0] != "CraftingStation")
                return;

            if (_craftableCookingStations.ContainsKey(properties[1]))
            {
                OpenCookingMenu(_craftableCookingStations[properties[1]]);
                Helper.Input.Suppress(e.Button);
            } else if (_craftableCraftingStations.ContainsKey(properties[1]))
            {
                OpenCraftingMenu(_craftableCookingStations[properties[1]]);
                Helper.Input.Suppress(e.Button);
            }
        }

        public void OpenCookingMenu(string[] recipes)
        {
            Dictionary<string, string> stationRecipes = new Dictionary<string, string>();
            foreach (var kvp in AllCookingRecipes.Where(kvp => recipes.Contains(kvp.Key)))
            {
                stationRecipes.Add(kvp.Key, kvp.Value);
            }
            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
            CraftingRecipe.cookingRecipes = stationRecipes;
            OpenedModdedStation = true;
            Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
            CraftingRecipe.cookingRecipes = AllCookingRecipes;
        }

        public void OpenCraftingMenu(string[] recipes)
        {
            Dictionary<string, string> stationRecipes = new Dictionary<string, string>();
            foreach (var kvp in AllCraftingRecipes.Where(kvp => recipes.Contains(kvp.Key)))
            {
                stationRecipes.Add(kvp.Key, kvp.Value);
            }

            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
            CraftingRecipe.craftingRecipes = stationRecipes;
            OpenedModdedStation = true;
            Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false, true);
            CraftingRecipe.craftingRecipes = AllCraftingRecipes;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            //save the full list of recipes
            AllCookingRecipes = CraftingRecipe.cookingRecipes;
            AllCraftingRecipes = CraftingRecipe.craftingRecipes;

            //register content packs
            RegisterContentPacks();

            //remove exclusive recipes
            ReducedCookingRecipes = new Dictionary<string, string>();
            ReducedCraftingRecipes = new Dictionary<string, string>();

            foreach (var kvp in AllCookingRecipes.Where(kvp => !_cookingRecipesToRemove.Contains(kvp.Key)))
            {
                ReducedCookingRecipes.Add(kvp.Key,kvp.Value);
            }

            foreach (var kvp in AllCraftingRecipes.Where(kvp => !_craftingRecipesToRemove.Contains(kvp.Key)))
            {
                ReducedCraftingRecipes.Add(kvp.Key, kvp.Value);
            }
        }

        private void RegisterContentPacks()
        {
            var packs = Helper.ContentPacks.GetOwned();

            _tileCookingStations = new Dictionary<string, string[]>();
            _craftableCookingStations = new Dictionary<string, string[]>();
            _tileCraftingStations = new Dictionary<string, string[]>();
            _craftableCraftingStations = new Dictionary<string, string[]>();
            _cookingRecipesToRemove = new List<string>();
            _craftingRecipesToRemove = new List<string>();

            foreach (var pack in packs)
            {
                if (!pack.HasFile("content.json"))
                {
                    Monitor.Log($"{pack.Manifest.UniqueID} is missing a content.json", LogLevel.Error);
                    continue;
                }

                ContentPack contentPack = pack.LoadAsset<ContentPack>("content.json");

                RegisterCookingStations(contentPack.CookingStations);
                RegisterCraftingStations(contentPack.CraftingStations);
            }
        }

        private void RegisterCookingStations(List<CraftingStation> cookingStations)
        {
            if (cookingStations == null)
                return;
            foreach (CraftingStation station in cookingStations)
            {
                if (station.ExclusiveRecipes)
                    _cookingRecipesToRemove.AddRange(station.Recipes);

                if (station.TileData != null)
                {
                    if (_tileCookingStations.Keys.Contains(station.TileData)){
                        Monitor.Log($"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.",LogLevel.Error);
                    } else
                    {
                        if (station.TileData != null)
                            _tileCookingStations.Add(station.TileData, station.Recipes);
                    }
                    
                }

                if (station.BigCraftable == null) continue;
                if (_craftableCookingStations.Keys.Contains(station.BigCraftable))
                {
                    Monitor.Log($"Multiple mods are trying to use the BigCraftable {station.BigCraftable}; Only one will be applied.", LogLevel.Error);
                }
                else
                {
                    if (station.BigCraftable != null)
                        _craftableCookingStations.Add(station.BigCraftable, station.Recipes);
                }

            }
        }

        private void RegisterCraftingStations(List<CraftingStation> craftingStations)
        {
            if (craftingStations == null)
                return;
            foreach (var station in craftingStations)
            {
                if (station.ExclusiveRecipes)
                    _craftingRecipesToRemove.AddRange(station.Recipes);

                if (station.TileData != null)
                {
                    if (_tileCraftingStations.Keys.Contains(station.TileData) || _tileCookingStations.Keys.Contains(station.TileData))
                    {
                        Monitor.Log($"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.", LogLevel.Error);
                    }
                    else
                    {
                        if (station.TileData != null)
                            _tileCraftingStations.Add(station.TileData, station.Recipes);
                    }

                }

                if (station.BigCraftable == null) continue;
                if (_craftableCraftingStations.Keys.Contains(station.BigCraftable) || _craftableCookingStations.Keys.Contains(station.BigCraftable))
                {
                    Monitor.Log($"Multiple mods are trying to use the BigCraftable {station.BigCraftable}; Only one will be applied.", LogLevel.Error);
                }
                else
                {
                    if (station.BigCraftable != null)
                        _craftableCraftingStations.Add(station.BigCraftable, station.Recipes);
                }

            }
        }
    }
}
