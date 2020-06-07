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
        private Dictionary<string, string> AllCookingRecipes;
        private Dictionary<string, string> AllCraftingRecipes;

        private Dictionary<string, string> ReducedCookingRecipes;
        private Dictionary<string, string> ReducedCraftingRecipes;

        private Dictionary<string, string[]> TileCookingStations;
        private Dictionary<string, string[]> CraftableCookingStations;

        private Dictionary<string, string[]> TileCraftingStations;
        private Dictionary<string, string[]> CraftableCraftingStations;

        private List<string> CookingRecipesToRemove;
        private List<string> CraftingRecipesToRemove;

        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Helper.Events.Display.MenuChanged += Display_MenuChanged;
        }

        public bool OpenedModdedStation = false;
        public bool OpenedVanillaCrafting = false;

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (OpenedModdedStation)
            {
                OpenedModdedStation = false;
                return;
            }

            if (OpenedVanillaCrafting)
            {
                OpenedVanillaCrafting = false;
                CraftingRecipe.cookingRecipes = AllCookingRecipes;
                CraftingRecipe.craftingRecipes = AllCraftingRecipes;
            }
            else if (e.NewMenu is CraftingPage)
            {
                OpenedVanillaCrafting = true;
                CraftingRecipe.cookingRecipes = ReducedCookingRecipes;
                CraftingRecipe.craftingRecipes = ReducedCraftingRecipes;

                bool isCooking = Helper.Reflection.GetField<bool>(e.NewMenu, "cooking").GetValue();
                Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
                Game1.activeClickableMenu.exitThisMenuNoSound();
                Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, isCooking, true);
            }
            else if (e.NewMenu is GameMenu)
            {
                OpenedVanillaCrafting = true;
                CraftingRecipe.cookingRecipes = ReducedCookingRecipes;
                CraftingRecipe.craftingRecipes = ReducedCraftingRecipes;

                Game1.activeClickableMenu.exitThisMenuNoSound();
                Game1.activeClickableMenu = new GameMenu();

            }
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
            Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
            OpenedModdedStation = true;
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
            Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false, true);
            OpenedModdedStation = true;
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

    public class ContentPack
    {
        public List<CraftingStation> CookingStations;
        public List<CraftingStation> CraftingStations;
    }
    public class CraftingStation
    {
        public string BigCraftable { get; set; } //A big craftable to interact with to open the menu
        public string TileData { get; set; } //Name of the tiledata used to interact with to open the menu
        public bool ExclusiveRecipes { get; set; } = true; //Removes the listed recipes from the vanilla crafting menus
        public string[] Recipes { get; set; } //list of recipe names

    }
}
