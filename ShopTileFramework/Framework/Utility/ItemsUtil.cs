using ShopTileFramework.Framework.API;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework.Framework.Utility
{
    /// <summary>
    /// This class contains static utility methods used to handle items
    /// </summary>
    public static class ItemsUtil
    {
        public static Dictionary<string, IDictionary<int, string>> ObjectInfoSource { get; set; }
        public static List<string> RecipesList;
        private static Dictionary<int, string> _fruitTreeData;
        private static Dictionary<int, string> _cropData;

        public static List<string> PacksToRemove = new List<string>();
        public static List<string> RecipePacksToRemove = new List<string>();
        public static List<string> ItemsToRemove = new List<string>();

        /// <summary>
        /// Loads up the onject information for all types, 
        /// done at the start of each save loaded so that JA info is up to date
        /// </summary>
        public static void UpdateObjectInfoSource()
        {
            //load up all the object information into a static dictionary
            ObjectInfoSource = new Dictionary<string, IDictionary<int, string>>
            {
                ["Object"] = Game1.objectInformation,
                ["BigCraftable"] = Game1.bigCraftablesInformation,
                ["Clothing"] = Game1.clothingInformation,
                ["Ring"] = Game1.objectInformation,
                ["Hat"] = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/hats"),
                ["Boot"] = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/Boots"),
                ["Furniture"] = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/Furniture"),
                ["Weapon"] = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/weapons")
            };

            //load up recipe information
            RecipesList = ModEntry.helper.GameContent.Load<Dictionary<string, string>>("Data/CraftingRecipes").Keys.ToList();
            RecipesList.AddRange(ModEntry.helper.GameContent.Load<Dictionary<string, string>>("Data/CookingRecipes").Keys.ToList());

            //add "recipe" to the end of every element
            RecipesList = RecipesList.Select(s => s + " Recipe").ToList();

            //load up tree and crop data
            _fruitTreeData = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/fruitTrees");
            _cropData = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/Crops");
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

        /// <summary>
        /// Get the itemID given a name and the object information that item belongs to
        /// </summary>
        /// <param name="name">name of the item</param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public static int GetIndexByName(string name, string itemType= "Object")
        {
            foreach (KeyValuePair<int, string> kvp in ObjectInfoSource[itemType])
            {
                if (kvp.Value.Split('/')[0] == name)
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if an itemtype is valid
        /// </summary>
        /// <param name="itemType">The name of the itemtype</param>
        /// <returns>True if it's a valid type, false if not</returns>
        public static bool CheckItemType(string itemType)
        {
            return (itemType == "Seed" || ObjectInfoSource.ContainsKey(itemType));
        }

        /// <summary>
        /// Given the name of a crop, return the ID of its seed object
        /// </summary>
        /// <param name="cropName">The name of the crop object</param>
        /// <returns>The ID of the seed object if found, -1 if not</returns>
        public static int GetSeedId(string cropName)
        {
            //int cropID = ModEntry.JsonAssets.GetCropId(cropName);
            int cropId = APIs.JsonAssets.GetCropId(cropName);
            foreach (KeyValuePair<int, string> kvp in _cropData)
            {
                //find the tree id in crops information to get seed id
                Int32.TryParse(kvp.Value.Split('/')[2], out int id);
                if (cropId == id)
                    return kvp.Key;
            }

            return -1;
        }

        /// <summary>
        /// Given the name of a tree crop, return the ID of its sapling object
        /// </summary>
        /// <returns>The ID of the sapling object if found, -1 if not</returns>
        public static int GetSaplingId(string treeName)
        {
            int treeId = APIs.JsonAssets.GetFruitTreeId(treeName);
            foreach (KeyValuePair<int, string> kvp in _fruitTreeData)
            {
                //find the tree id in fruitTrees information to get sapling id
                Int32.TryParse(kvp.Value.Split('/')[0], out int id);
                if (treeId == id)
                    return kvp.Key;
            }

            return -1;
        }

        public static void RegisterPacksToRemove(string[] JApacks,string[] recipePacks, string[] itemNames)
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
            if (APIs.JsonAssets == null)
                return;

            foreach (string pack in PacksToRemove)
            {
                var items = APIs.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllClothingFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllHatsFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllObjectsFromContentPack(pack);
                if (items != null)
                {
                    ItemsToRemove.AddRange(items);
                }

                var crops = APIs.JsonAssets.GetAllCropsFromContentPack(pack);

                if (crops != null)
                {
                    foreach (int seedId in crops.Select(GetSeedId))
                    {
                        ItemsToRemove.Add(ObjectInfoSource["Object"][seedId].Split('/')[0]);
                    }
                }

                var trees = APIs.JsonAssets.GetAllFruitTreesFromContentPack(pack);
                if (trees != null)
                {
                    foreach (int saplingID in trees.Select(GetSaplingId))
                    {ItemsToRemove.Add(ObjectInfoSource["Object"][saplingID].Split('/')[0]);
                    }
                }


                items = APIs.JsonAssets.GetAllWeaponsFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items);
            }

            foreach (string pack in RecipePacksToRemove)
            {
                var items = APIs.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    ItemsToRemove.AddRange(items.Select(i => (i + " Recipe")));

                items = APIs.JsonAssets.GetAllObjectsFromContentPack(pack);
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

        public static bool IsInSeasonCrop(int itemId)
        {
            if (_cropData.ContainsKey(itemId))
            {
                return _cropData[itemId].Split('/')[1].Contains(Game1.currentSeason);
            }

            if (_fruitTreeData.ContainsKey(itemId))
            {
                return _fruitTreeData[itemId].Split('/')[1].Contains(Game1.currentSeason);
            }

            return false;
        }
    }
}
