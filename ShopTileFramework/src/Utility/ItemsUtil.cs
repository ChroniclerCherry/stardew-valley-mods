using ShopTileFramework.API;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework.Utility
{
    /// <summary>
    /// This class contains static utility methods used to handle items
    /// </summary>
    class ItemsUtil
    {
        public static Dictionary<string, IDictionary<int, string>> ObjectInfoSource { get; set; }
        public static List<string> RecipesList;
        private static Dictionary<int, string> fruitTreeData;
        private static Dictionary<int, string> cropData;

        private static List<string> packsToRemove = new List<string>();
        private static List<string> itemsToRemove = new List<string>();

        /// <summary>
        /// Loads up the onject information for all types, 
        /// done at the start of each save loaded so that JA info is up to date
        /// </summary>
        public static void UpdateObjectInfoSource()
        {
            //load up all the object information into a static dictionary
            ObjectInfoSource = new Dictionary<string, IDictionary<int, string>>
            {
                { "Object", Game1.objectInformation },
                { "BigCraftable", Game1.bigCraftablesInformation },
                { "Clothing", Game1.clothingInformation },
                { "Ring", Game1.objectInformation },
                {
                    "Hat",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                        (@"Data/hats", ContentSource.GameContent)
                },
                {
                    "Boot",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                            (@"Data/Boots", ContentSource.GameContent)
                },
                {
                    "Furniture",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                            (@"Data/Furniture", ContentSource.GameContent)
                },
                {
                    "Weapon",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                            (@"Data/weapons", ContentSource.GameContent)
                }
            };

            //load up recipe information
            RecipesList = ModEntry.helper.Content.Load<Dictionary<string, string>>(@"Data/CraftingRecipes", ContentSource.GameContent).Keys.ToList();
            RecipesList.AddRange(ModEntry.helper.Content.Load<Dictionary<string, string>>(@"Data/CookingRecipes", ContentSource.GameContent).Keys.ToList());

            //add "recipe" to the end of every element
            RecipesList = RecipesList.Select(s => s + " Recipe").ToList();

            //load up tree and crop data
            fruitTreeData = ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/fruitTrees", ContentSource.GameContent);
            cropData = ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/Crops", ContentSource.GameContent);
        }

        /// <summary>
        /// Given and ItemInventoryAndStock, and a maximum number, randomly reduce the stock until it hits that number
        /// </summary>
        /// <param name="inventory">the ItemPriceAndStock</param>
        /// <param name="MaxNum">The maximum number of items we want for this stock</param>
        public static void RandomizeStock(Dictionary<ISalable, int[]> inventory, int MaxNum)
        {
            while (inventory.Count > MaxNum)
            {
                inventory.Remove(inventory.Keys.ElementAt(Game1.random.Next(inventory.Count)));
            }

        }

        /// <summary>
        /// Get the itemID given a name and the object information that item belongs to
        /// </summary>
        /// <param name="name">name of the item</param>
        /// <param name="ObjectInfo">the information data for that item's type</param>
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
        /// <param name="ItemType">The name of the itemtype</param>
        /// <returns>True if it's a valid type, false if not</returns>
        public static bool CheckItemType(string ItemType)
        {
            return (ItemType == "Seed" || ObjectInfoSource.ContainsKey(ItemType));
        }

        /// <summary>
        /// Given the name of a crop, return the ID of its seed object
        /// </summary>
        /// <param name="cropName">The name of the crop object</param>
        /// <returns>The ID of the seed object if found, -1 if not</returns>
        public static int GetSeedID(string cropName)
        {
            //int cropID = ModEntry.JsonAssets.GetCropId(cropName);
            int cropID = GetIndexByName(cropName);
            foreach (KeyValuePair<int, string> kvp in cropData)
            {
                //find the tree id in crops information to get seed id
                Int32.TryParse(kvp.Value.Split('/')[3], out int id);
                if (cropID == id)
                    return kvp.Key;
            }

            return -1;
        }

        /// <summary>
        /// Given the name of a tree crop, return the ID of its sapling object
        /// </summary>
        /// <param name="cropName">The name of the tree crop object</param>
        /// <returns>The ID of the sapling object if found, -1 if not</returns>
        public static int GetSaplingID(string treeName)
        {
            int treeID = GetIndexByName(treeName);
            foreach (KeyValuePair<int, string> kvp in fruitTreeData)
            {
                //find the tree id in fruitTrees information to get sapling id
                Int32.TryParse(kvp.Value.Split('/')[2], out int id);
                if (treeID == id)
                    return kvp.Key;
            }

            return -1;
        }

        public static void RegisterPacksToRemove(string[] JApacks)
        {
            packsToRemove = packsToRemove.Union(JApacks).ToList();
        }

        public static void RegisterItemsToRemove()
        {
            if (APIs.JsonAssets == null)
                return;

            foreach (string pack in packsToRemove)
            {

                var items = APIs.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    itemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllClothingFromContentPack(pack);
                if (items != null)
                    itemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllHatsFromContentPack(pack);
                if (items != null)
                    itemsToRemove.AddRange(items);
                    

                items = APIs.JsonAssets.GetAllObjectsFromContentPack(pack);
                if (items != null)
                {
                    itemsToRemove.AddRange(items);
                    itemsToRemove.AddRange(items.Select(i => (i + " Recipe")));
                }
                    

                items = APIs.JsonAssets.GetAllWeaponsFromContentPack(pack);
                if (items != null)
                    itemsToRemove.AddRange(items);
            }
        }

        public static Dictionary<ISalable, int[]> RemoveSpecifiedJAPacks(Dictionary<ISalable, int[]> stock)
        {
            List<ISalable> removeItems = (stock.Keys.Where(item => itemsToRemove.Contains(item.Name))).ToList();
            
            foreach (var item in removeItems)
            {
                stock.Remove(item);
            }

            return stock;
        }
    }
}
