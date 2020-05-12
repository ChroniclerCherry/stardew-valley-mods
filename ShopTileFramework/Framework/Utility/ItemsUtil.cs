using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework.Framework.Utility
{
    class ItemsUtil
    {
        public static Dictionary<string, IDictionary<int, string>> ObjectInfoSource { get; set; }
        public static List<string> RecipesList;
        private static Dictionary<int, string> fruitTreeData;
        private static Dictionary<int, string> cropData;

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

        public static void RandomizeStock(Dictionary<ISalable, int[]> inventory, int MaxNum)
        {
            while (inventory.Count > MaxNum)
            {
                inventory.Remove(inventory.Keys.ElementAt(Game1.random.Next(inventory.Count)));
            }

        }

        public static int GetIndexByName(string name, IDictionary<int, string> ObjectInfo)
        {
            foreach (KeyValuePair<int, string> kvp in ObjectInfo)
            {
                if (kvp.Value.Split('/')[0] == name)
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        public static bool CheckItemType(string ItemType)
        {
            return (ItemType == "Seed" || ObjectInfoSource.ContainsKey(ItemType));
        }

        public static int GetSeedID(string cropName)
        {
            //int cropID = ModEntry.JsonAssets.GetCropId(cropName);
            int cropID = GetIndexByName(cropName, ObjectInfoSource["Object"]);
            foreach (KeyValuePair<int, string> kvp in cropData)
            {
                //find the tree id in crops information to get seed id
                Int32.TryParse(kvp.Value.Split('/')[3], out int id);
                if (cropID == id)
                    return kvp.Key;
            }

            return -1;
        }

        public static int GetSaplingID(string treeName)
        {
            int treeID = GetIndexByName(treeName, ObjectInfoSource["Object"]);
            foreach (KeyValuePair<int, string> kvp in fruitTreeData)
            {
                //find the tree id in fruitTrees information to get sapling id
                Int32.TryParse(kvp.Value.Split('/')[2], out int id);
                if (treeID == id)
                    return kvp.Key;
            }

            return -1;
        }
    }
}
