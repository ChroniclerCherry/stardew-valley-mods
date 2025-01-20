using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace CustomCraftingStations.Framework
{
    /// <summary>Manages content loaded from Custom Crafting Stations content packs.</summary>
    internal class ContentManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        public Dictionary<string, CraftingStationConfig> TileCraftingStations { get; } = new();
        public Dictionary<string, CraftingStationConfig> CraftableCraftingStations { get; } = new();

        public List<string> CookingRecipesToRemove { get; } = new();
        public List<string> CraftingRecipesToRemove { get; } = new();

        public List<string> ReducedCookingRecipes { get; } = new();
        public List<string> ReducedCraftingRecipes { get; } = new();


        /*********
        ** Public methods
        *********/
        public ContentManager(IEnumerable<IContentPack> contentPacks, IMonitor monitor)
        {
            this.Monitor = monitor;

            this.RegisterContentPacks(contentPacks);
        }


        /*********
        ** Private methods
        *********/
        private void RegisterContentPacks(IEnumerable<IContentPack> contentPacks)
        {
            // register content packs
            foreach (IContentPack pack in contentPacks)
            {
                if (!pack.HasFile("content.json"))
                {
                    this.Monitor.Log($"{pack.Manifest.UniqueID} is missing a content.json", LogLevel.Error);
                    continue;
                }

                ContentPack contentPack = pack.ModContent.Load<ContentPack>("content.json");

                this.RegisterCraftingStations(contentPack.CraftingStations);
            }

            // track exclusive recipes
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
    }
}
