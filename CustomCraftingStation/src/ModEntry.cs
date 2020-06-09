using CustomCraftingStation.src;
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

        private Dictionary<string, string[]> TileCookingStations;
        private Dictionary<string, string[]> CraftableCookingStations;

        private Dictionary<string, string[]> TileCraftingStations;
        private Dictionary<string, string[]> CraftableCraftingStations;

        private List<string> CookingRecipesToRemove;
        private List<string> CraftingRecipesToRemove;

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

            Game1.currentLocation.Objects.TryGetValue(grabTile, out StardewValley.Object obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (CraftableCookingStations.ContainsKey(obj.Name)){
                    OpenCookingMenu(CraftableCookingStations[obj.Name]);
                    Helper.Input.Suppress(e.Button);
                    return;
                } else if (CraftableCraftingStations.ContainsKey(obj.Name))
                {
                    OpenCraftingMenu(CraftableCraftingStations[obj.Name]);
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

            if (CraftableCookingStations.ContainsKey(properties[1]))
            {
                OpenCookingMenu(CraftableCookingStations[properties[1]]);
                Helper.Input.Suppress(e.Button);
            } else if (CraftableCraftingStations.ContainsKey(properties[1]))
            {
                OpenCraftingMenu(CraftableCookingStations[properties[1]]);
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

            foreach (KeyValuePair<string, string> kvp in AllCookingRecipes)
            {
                if (!CookingRecipesToRemove.Contains(kvp.Key))
                    ReducedCookingRecipes.Add(kvp.Key,kvp.Value);
            }

            foreach (KeyValuePair<string, string> kvp in AllCraftingRecipes)
            {
                if (!CraftingRecipesToRemove.Contains(kvp.Key))
                    ReducedCraftingRecipes.Add(kvp.Key, kvp.Value);
            }
        }

        private void RegisterContentPacks()
        {
            var packs = Helper.ContentPacks.GetOwned();

            TileCookingStations = new Dictionary<string, string[]>();
            CraftableCookingStations = new Dictionary<string, string[]>();
            TileCraftingStations = new Dictionary<string, string[]>();
            CraftableCraftingStations = new Dictionary<string, string[]>();
            CookingRecipesToRemove = new List<string>();
            CraftingRecipesToRemove = new List<string>();

            foreach (IContentPack pack in packs)
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

        private void RegisterCookingStations(List<CraftingStation> CookingStations)
        {
            if (CookingStations == null)
                return;
            foreach (CraftingStation station in CookingStations)
            {
                if (station.ExclusiveRecipes)
                    CookingRecipesToRemove.AddRange(station.Recipes);

                if (station.TileData != null)
                {
                    if (TileCookingStations.Keys.Contains(station.TileData)){
                        Monitor.Log($"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.",LogLevel.Error);
                    } else
                    {
                        if (station.TileData != null)
                            TileCookingStations.Add(station.TileData, station.Recipes);
                    }
                    
                }

                if (station.BigCraftable != null)
                {
                    if (CraftableCookingStations.Keys.Contains(station.BigCraftable))
                    {
                        Monitor.Log($"Multiple mods are trying to use the BigCraftable {station.BigCraftable}; Only one will be applied.", LogLevel.Error);
                    }
                    else
                    {
                        if (station.BigCraftable != null)
                            CraftableCookingStations.Add(station.BigCraftable, station.Recipes);
                    }

                }

            }
        }

        private void RegisterCraftingStations(List<CraftingStation> CraftingStations)
        {
            if (CraftingStations == null)
                return;
            foreach (CraftingStation station in CraftingStations)
            {
                if (station.ExclusiveRecipes)
                    CraftingRecipesToRemove.AddRange(station.Recipes);

                if (station.TileData != null)
                {
                    if (TileCraftingStations.Keys.Contains(station.TileData) || TileCookingStations.Keys.Contains(station.TileData))
                    {
                        Monitor.Log($"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.", LogLevel.Error);
                    }
                    else
                    {
                        if (station.TileData != null)
                            TileCraftingStations.Add(station.TileData, station.Recipes);
                    }

                }

                if (station.BigCraftable != null)
                {
                    if (CraftableCraftingStations.Keys.Contains(station.BigCraftable) || CraftableCookingStations.Keys.Contains(station.BigCraftable))
                    {
                        Monitor.Log($"Multiple mods are trying to use the BigCraftable {station.BigCraftable}; Only one will be applied.", LogLevel.Error);
                    }
                    else
                    {
                        if (station.BigCraftable != null)
                            CraftableCraftingStations.Add(station.BigCraftable, station.Recipes);
                    }

                }

            }
        }
    }
}
