using System.Collections.Generic;
using System.Linq;
using ShopTileFramework.Framework.Apis;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FruitTrees;
using StardewValley.ItemTypeDefinitions;

namespace ShopTileFramework.Framework.Utility
{
    /// <summary>
    /// This class contains static utility methods used to handle items
    /// </summary>
    public static class ItemsUtil
    {
        public static List<string> RecipesList;

        public static List<string> PacksToRemove = new List<string>();
        public static List<string> RecipePacksToRemove = new List<string>();
        public static List<string> ItemsToRemove = new List<string>();

        /// <summary>
        /// Loads up the onject information for all types, 
        /// done at the start of each save loaded so that JA info is up to date
        /// </summary>
        public static void UpdateObjectInfoSource()
        {
            //load up recipe information
            RecipesList = CraftingRecipe.craftingRecipes.Keys.ToList();
            RecipesList.AddRange(CraftingRecipe.cookingRecipes.Keys);

            //add "recipe" to the end of every element
            RecipesList = RecipesList.Select(s => s + " Recipe").ToList();
        }

        /// <summary>
        /// Given and ItemInventoryAndStock, and a maximum number, randomly reduce the stock until it hits that number
        /// </summary>
        /// <param name="inventory">the ItemPriceAndStock</param>
        /// <param name="maxNum">The maximum number of items we want for this stock</param>
        public static void RandomizeStock(Dictionary<ISalable, ItemStockInformation> inventory, int maxNum)
        {
            while (inventory.Count > maxNum)
            {
                inventory.Remove(inventory.Keys.ElementAt(Game1.random.Next(inventory.Count)));
            }
        }

        /// <summary>Get the qualified item ID to spawn given its name.</summary>
        /// <param name="name">The item name.</param>
        /// <param name="itemType">The item type, matching a key recognized by <see cref="GetItemDataDefinitionFromType"/>.</param>
        /// <returns>Returns the item's qualified item ID, or <c>null</c> if not found.</returns>
        public static string GetItemIdByName(string name, string itemType = "Object")
        {
            foreach (IItemDataDefinition itemDataDefinition in GetItemDataDefinitionFromType(itemType))
            {
                foreach (ParsedItemData data in itemDataDefinition.GetAllData())
                {
                    if (data.InternalName == name)
                        return data.QualifiedItemId;
                }
            }

            return null;
        }

        /// <summary>Get the item data definition which provides items of a given type.</summary>
        /// <param name="itemType">The Multi Yield Crops type ID.</param>
        public static IEnumerable<IItemDataDefinition> GetItemDataDefinitionFromType(string itemType)
        {
            switch (itemType)
            {
                case "BigCraftable":
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_bigCraftable);
                    break;

                case "Boot":
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_boots);
                    break;

                case "Clothing":
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_pants);
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_shirt);
                    break;

                case "Furniture":
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_furniture);
                    break;

                case "Hat":
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_hat);
                    break;

                case "Object":
                case "Ring":
                    yield return ItemRegistry.GetObjectTypeDefinition();
                    break;

                case "Weapon":
                    yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_weapon);
                    break;
            }
        }

        /// <summary>
        /// Checks if an itemtype is valid
        /// </summary>
        /// <param name="itemType">The name of the itemtype</param>
        /// <returns>True if it's a valid type, false if not</returns>
        public static bool CheckItemType(string itemType)
        {
            return itemType == "Seed" || GetItemDataDefinitionFromType(itemType) is not null;
        }

        /// <summary>
        /// Given the name of a crop, return the ID of its seed object
        /// </summary>
        /// <param name="cropName">The name of the crop object</param>
        /// <returns>The ID of the seed object if found, -1 if not</returns>
        public static string GetSeedId(string cropName)
        {
            //int cropID = ModEntry.JsonAssets.GetCropId(cropName);
            string cropId = ApiManager.JsonAssets.GetCropId(cropName);
            foreach ((string id, CropData data) in Game1.cropData)
            {
                // find the seed id in crops information to get seed id
                if (data.HarvestItemId == cropId)
                    return id;
            }

            return null;
        }

        /// <summary>
        /// Given the name of a tree crop, return the ID of its sapling object
        /// </summary>
        /// <returns>The ID of the sapling object if found, -1 if not</returns>
        public static string GetSaplingId(string treeName)
        {
            string treeId = ApiManager.JsonAssets.GetFruitTreeId(treeName);
            foreach ((string saplingId, FruitTreeData data) in Game1.fruitTreeData)
            {
                //find the tree id in fruitTrees information to get sapling id
                if (data.Fruit.Any(p => p.ItemId == treeId))
                    return saplingId;
            }

            return null;
        }

        public static void RegisterPacksToRemove(string[] JApacks, string[] recipePacks, string[] itemNames)
        {
            if (JApacks != null)
                PacksToRemove = PacksToRemove.Union(JApacks).ToList();

            if (recipePacks != null)
                RecipePacksToRemove = RecipePacksToRemove.Union(recipePacks).ToList();

            if (itemNames != null)
                ItemsToRemove = ItemsToRemove.Union(itemNames).ToList();
        }

        public static void RegisterItemsToRemove()
        {
            if (ApiManager.JsonAssets == null)
                return;

            foreach (string pack in PacksToRemove)
            {
                var items = ApiManager.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);

                items = ApiManager.JsonAssets.GetAllClothingFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);

                items = ApiManager.JsonAssets.GetAllHatsFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);

                items = ApiManager.JsonAssets.GetAllObjectsFromContentPack(pack);
                if (items != null)
                {
                    ItemsToRemove.AddRange(items);
                }

                var crops = ApiManager.JsonAssets.GetAllCropsFromContentPack(pack);

                if (crops != null)
                {
                    foreach (string seedId in crops.Select(GetSeedId))
                    {
                        ItemsToRemove.Add(ItemRegistry.GetDataOrErrorItem(seedId).InternalName);
                    }
                }

                var trees = ApiManager.JsonAssets.GetAllFruitTreesFromContentPack(pack);
                if (trees != null)
                {
                    foreach (string saplingId in trees.Select(GetSaplingId))
                        ItemsToRemove.Add(ItemRegistry.GetDataOrErrorItem(saplingId).InternalName);
                }


                items = ApiManager.JsonAssets.GetAllWeaponsFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);
            }

            foreach (string pack in RecipePacksToRemove)
            {
                var items = ApiManager.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items.Select(i => (i + " Recipe")));

                items = ApiManager.JsonAssets.GetAllObjectsFromContentPack(pack);
                if (items != null)
                {
                    ItemsToRemove.AddRange(items.Select(i => (i + " Recipe")));
                }
            }
        }

        public static Dictionary<ISalable, ItemStockInformation> RemoveSpecifiedJAPacks(Dictionary<ISalable, ItemStockInformation> stock)
        {
            List<ISalable> removeItems = (stock.Keys.Where(item => ItemsToRemove.Contains(item.Name))).ToList();

            foreach (var item in removeItems)
            {
                stock.Remove(item);
            }

            return stock;
        }

        public static void RemoveSoldOutItems(Dictionary<ISalable, ItemStockInformation> stock)
        {
            ISalable[] keysToRemove = stock.Where(kvp => kvp.Value.Stock == 0).Select(kvp => kvp.Key).ToArray();
            foreach (ISalable item in keysToRemove)
                stock.Remove(item);
        }

        public static bool IsInSeasonCrop(string itemId)
        {
            if (itemId is not null)
            {
                if (Game1.cropData.TryGetValue(itemId, out CropData cropData))
                    return cropData.Seasons?.Contains(Game1.season) is true;

                if (Game1.fruitTreeData.TryGetValue(itemId, out FruitTreeData treeData))
                    return treeData.Seasons?.Contains(Game1.season) is true;
            }

            return false;
        }
    }
}
