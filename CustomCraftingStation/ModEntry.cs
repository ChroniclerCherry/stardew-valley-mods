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

        private Dictionary<string, Dictionary<string, string>> TileCookingStations;
        private Dictionary<string, Dictionary<string, string>> CraftableCookingStations;

        private Dictionary<string, Dictionary<string, string>> TileCraftingStations;
        private Dictionary<string, Dictionary<string, string>> CraftableCraftingStations;

        private List<string> CookingRecipesToRemove;
        private List<string> CraftingRecipesToRemove;

        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
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
                } else if (CraftableCookingStations.ContainsKey(obj.Name))
                {
                    OpenCraftingMenu(CraftableCookingStations[obj.Name]);
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

        public void OpenCookingMenu(Dictionary<string, string> recipes)
        {
            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
            CraftingRecipe.cookingRecipes = recipes;
            Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
            CraftingRecipe.cookingRecipes = ReducedCookingRecipes;
        }

        public void OpenCraftingMenu(Dictionary<string, string> recipes)
        {
            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
            CraftingRecipe.cookingRecipes = recipes;
            Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false, true);
            CraftingRecipe.cookingRecipes = ReducedCookingRecipes;
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

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            //set limited recipes for default menus
            CraftingRecipe.cookingRecipes = ReducedCookingRecipes;
            CraftingRecipe.craftingRecipes = ReducedCraftingRecipes;
        }
        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            //return all recipes at end of day so that recipe unlocks can be checked for by vanilla
            CraftingRecipe.cookingRecipes = AllCookingRecipes;
            CraftingRecipe.craftingRecipes = AllCraftingRecipes;
        }

        private void RegisterContentPacks()
        {
            var packs = Helper.ContentPacks.GetOwned();

            TileCookingStations = new Dictionary<string, Dictionary<string, string>>();
            CraftableCookingStations = new Dictionary<string, Dictionary<string, string>>();
            CookingRecipesToRemove = new List<string>();
            CraftingRecipesToRemove = new List<string>();

            foreach (IContentPack pack in packs)
            {
                if (!pack.HasFile("Content.json"))
                {
                    Monitor.Log($"{pack.Manifest.UniqueID} is missing a Content.json", LogLevel.Error);
                    continue;
                }

                ContentPack contentPack = pack.LoadAsset<ContentPack>("Content.json");

                RegisterCookingStations(contentPack.CookingStations);
                RegisterCraftingStations(contentPack.CraftingStations);
            }
        }

        private void RegisterCookingStations(List<CraftingStation> CookingStations)
        {
            foreach (CraftingStation station in CookingStations)
            {
                Dictionary<string, string> recipesList = new Dictionary<string, string>();

                if (station.ExclusiveRecipes)
                    CookingRecipesToRemove.AddRange(station.Recipes);

                foreach (string recipe in station.Recipes)
                {
                    if (AllCookingRecipes.Keys.Contains(recipe))
                        recipesList.Add(recipe, AllCookingRecipes[recipe]);
                }

                if (station.TileData != null)
                {
                    if (TileCookingStations.Keys.Contains(station.TileData)){
                        Monitor.Log($"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.",LogLevel.Error);
                    } else
                    {
                        TileCookingStations.Add(station.TileData, recipesList);
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
                        CraftableCookingStations.Add(station.TileData, recipesList);
                    }

                }

            }
        }

        private void RegisterCraftingStations(List<CraftingStation> CraftingStations)
        {
            foreach (CraftingStation station in CraftingStations)
            {
                Dictionary<string, string> recipesList = new Dictionary<string, string>();

                if (station.ExclusiveRecipes)
                    CraftingRecipesToRemove.AddRange(station.Recipes);

                foreach (string recipe in station.Recipes)
                {
                    if (AllCraftingRecipes.Keys.Contains(recipe))
                        recipesList.Add(recipe, AllCraftingRecipes[recipe]);
                }

                if (station.TileData != null)
                {
                    if (TileCraftingStations.Keys.Contains(station.TileData) || TileCookingStations.Keys.Contains(station.TileData))
                    {
                        Monitor.Log($"Multiple mods are trying to use the Tiledata {station.TileData}; Only one will be applied.", LogLevel.Error);
                    }
                    else
                    {
                        TileCraftingStations.Add(station.TileData, recipesList);
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
                        CraftableCraftingStations.Add(station.TileData, recipesList);
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
