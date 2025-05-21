using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

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

        /// <summary>The cooking recipes which should only appear in a crafting station, not the default crafting menu.</summary>
        private readonly HashSet<string> ExclusiveCookingRecipes = new();

        /// <summary>The crafting recipes which should only appear in a crafting station, not the default crafting menu.</summary>
        private readonly HashSet<string> ExclusiveCraftingRecipes = new();


        /*********
        ** Accessors
        *********/
        /// <summary>The crafting stations for tiles with the <c>Action CraftingStation {id}</c> tile property, indexed by ID.</summary>
        public Dictionary<string, CraftingStationConfig> TileCraftingStations { get; } = new();

        /// <summary>The crafting stations for placed big-craftable-type items.</summary>
        public Dictionary<string, CraftingStationConfig> CraftableCraftingStations { get; } = new();

        /// <summary>The cooking recipes to show in the default crafting menus.</summary>
        public HashSet<string> DefaultCookingRecipes { get; } = new();

        /// <summary>The crafting recipes to show in the default crafting menus.</summary>
        public HashSet<string> DefaultCraftingRecipes { get; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary"/></param>
        public ContentManager(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        /// <summary>Reset and register the given content packs.</summary>
        public void ResetAndRegisterContentPacks(IEnumerable<IContentPack> contentPacks)
        {
            // reset
            this.ExclusiveCookingRecipes.Clear();
            this.ExclusiveCraftingRecipes.Clear();

            this.TileCraftingStations.Clear();
            this.CraftableCraftingStations.Clear();

            this.DefaultCookingRecipes.Clear();
            this.DefaultCraftingRecipes.Clear();

            // register content packs
            foreach (IContentPack pack in contentPacks)
            {
                if (!pack.HasFile("content.json"))
                {
                    this.Monitor.Log($"{pack.Manifest.UniqueID} is missing a content.json", LogLevel.Error);
                    continue;
                }

                ContentPack contentPack = pack.ModContent.Load<ContentPack>("content.json");

                this.RegisterCraftingStations(pack, contentPack.CraftingStations);
            }

            // track exclusive recipes
            foreach (string recipeId in CraftingRecipe.craftingRecipes.Keys)
            {
                if (!this.ExclusiveCraftingRecipes.Contains(recipeId))
                    this.DefaultCraftingRecipes.Add(recipeId);
            }

            foreach (string recipeId in CraftingRecipe.cookingRecipes.Keys)
            {
                if (!this.ExclusiveCookingRecipes.Contains(recipeId))
                    this.DefaultCookingRecipes.Add(recipeId);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register the crafting stations added by a content pack.</summary>
        /// <param name="contentPack">The content pack registering crafting stations.</param>
        /// <param name="craftingStations">The crafting stations to register.</param>
        private void RegisterCraftingStations(IContentPack contentPack, CraftingStationConfig[] craftingStations)
        {
            foreach (CraftingStationConfig station in craftingStations)
            {
                string? stationName = !string.IsNullOrWhiteSpace(station.BigCraftable)
                    ? station.BigCraftable
                    : station.TileData;

                // validate
                if (string.IsNullOrWhiteSpace(stationName))
                {
                    this.Monitor.Log($"Content pack '{contentPack.Manifest.Name}' has a station with neither {nameof(station.BigCraftable)} nor {nameof(station.TileData)} set, so it has no effect.", LogLevel.Warn);
                    continue;
                }
                this.PreprocessContentPackRecipeKeys(contentPack, stationName, station.CraftingRecipes, CraftingRecipe.craftingRecipes, "crafting");
                this.PreprocessContentPackRecipeKeys(contentPack, stationName, station.CookingRecipes, CraftingRecipe.cookingRecipes, "cooking");
                if (station.CookingRecipes.Count > 0 && station.CraftingRecipes.Count > 0)
                    this.Monitor.Log($"Content pack '{contentPack.Manifest.Name}' has station '{stationName}' with both cooking and crafting recipes; each station can only provide one recipe type.", LogLevel.Warn);

                // track exclusive recipes
                if (station.ExclusiveRecipes)
                {
                    this.ExclusiveCraftingRecipes.AddRange(station.CraftingRecipes);
                    this.ExclusiveCookingRecipes.AddRange(station.CookingRecipes);
                }

                // track station
                if (station.TileData != null)
                {
                    if (this.TileCraftingStations.Keys.Contains(station.TileData))
                        this.Monitor.Log($"Multiple mods are trying to use the {nameof(station.TileData)} {station.TileData}; Only one will be applied.", LogLevel.Error);
                    else
                        this.TileCraftingStations.Add(station.TileData, station);
                }

                if (station.BigCraftable != null)
                {
                    if (this.CraftableCraftingStations.Keys.Contains(station.BigCraftable))
                        this.Monitor.Log($"Multiple mods are trying to use the {nameof(station.BigCraftable)} {station.BigCraftable}; Only one will be applied.", LogLevel.Error);
                    else
                        this.CraftableCraftingStations.Add(station.BigCraftable, station);
                }
            }
        }

        /// <summary>Preprocess the recipe keys in a content pack to either fix or remove broken keys.</summary>
        /// <param name="contentPack">The content pack whose recipe keys are being preprocessed.</param>
        /// <param name="stationName">The name of the station whose recipes are being preprocessed.</param>
        /// <param name="stationRecipes">The recipe keys from the content pack to preprocess.</param>
        /// <param name="gameRecipes">The recipes from <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c> to match.</param>
        /// <param name="type">The recipe type to show in error messages, like 'cooking' or 'crafting'.</param>
        private void PreprocessContentPackRecipeKeys(IContentPack contentPack, string stationName, List<string> stationRecipes, Dictionary<string, string> gameRecipes, string type)
        {
            for (int i = stationRecipes.Count - 1; i >= 0; i--)
            {
                string key = stationRecipes[i];

                if (this.TryResolveRecipeKey(contentPack, stationName, gameRecipes, type, key, out string? foundKey))
                    stationRecipes[i] = foundKey;
                else
                {
                    this.Monitor.Log($"Content pack '{contentPack.Manifest.Name}' has station '{stationName}' with {type} recipe '{key}' which couldn't be found.");
                    stationRecipes.RemoveAt(i);
                }
            }
        }

        /// <summary>Resolve a recipe key from a content pack to the actual key in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c>.</summary>
        /// <param name="contentPack">The content pack whose recipe keys are being resolved.</param>
        /// <param name="stationName">The name of the station whose recipes are being preprocessed.</param>
        /// <param name="recipes">The recipes from <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c> to match.</param>
        /// <param name="type">The recipe type to show in error messages, like 'cooking' or 'crafting'.</param>
        /// <param name="key">The recipe key from the content pack.</param>
        /// <param name="foundKey">The equivalent key in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c>, if resolved successfully.</param>
        /// <returns>Returns whether the key was resolved successfully.</returns>
        private bool TryResolveRecipeKey(IContentPack contentPack, string stationName, Dictionary<string, string> recipes, string type, string key, [NotNullWhen(true)] out string? foundKey)
        {
            // exact match
            if (recipes.ContainsKey(key))
            {
                foundKey = key;
                return true;
            }

            // Json Assets changed its recipe keys from name to a generated internal ID, which breaks mods which refer to
            // the old names. If a recipe isn't found, see if there's a unique match by suffix.
            {
                string suffix = "_" + Regex.Replace(key, "[/&@#$%*{}\\[\\]\\s\\\\]", "_").Trim(); // based on FixIdJA in the Json Assets code: https://github.com/spacechase0/StardewValleyMods/blob/develop/JsonAssets/Mod.cs#L44

                string? match = null;
                foreach (string gameKey in recipes.Keys)
                {
                    if (gameKey.EndsWith(suffix))
                    {
                        if (match is null)
                            match = gameKey;
                        else
                        {
                            match = null;
                            break;
                        }
                    }
                }

                if (match != null)
                {
                    this.Monitor.Log($"Content pack '{contentPack.Manifest.Name}' has station '{stationName}' with {type} recipe '{key}' which couldn't be found. Resolved to likely recipe key '{match}'.");
                    foundKey = match;
                    return true;
                }
            }

            // no match found
            foundKey = null;
            return false;
        }
    }
}
