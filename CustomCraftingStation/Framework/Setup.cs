using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using CustomCraftingStation.Framework;

namespace CustomCraftingStation
{
    public partial class ModEntry
    {
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

        private void RegisterContentPacks()
        {
            var packs = Helper.ContentPacks.GetOwned();

            _tileCraftingStations = new Dictionary<string, CraftingStation>();
            _craftableCraftingStations = new Dictionary<string, CraftingStation>();
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

                RegisterCraftingStations(contentPack.CraftingStations);
            }
        }

        private void RegisterCraftingStations(List<CraftingStation> craftingStations)
        {
            if (craftingStations == null)
                return;
            foreach (var station in craftingStations)
            {

                int numRecipes = station.CraftingRecipes.Count;
                for (int i = numRecipes-1; i >= 0; i--)
                {
                    if (!CraftingRecipe.craftingRecipes.Keys.Contains(station.CraftingRecipes[i]))
                    {
                        Monitor.Log($"The recipe for {station.CraftingRecipes[i]} could not be found.");
                        station.CraftingRecipes.RemoveAt(i);
                    }
                }

                numRecipes = station.CookingRecipes.Count;
                for (int i = numRecipes-1; i >= 0; i--)
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
    }
}