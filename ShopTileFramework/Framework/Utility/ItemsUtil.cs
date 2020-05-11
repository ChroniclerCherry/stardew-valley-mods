using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopTileFramework.Framework.Utility
{
    class ItemsUtil
    {

        public static Dictionary<string, IDictionary<int, string>> ObjectInfoSource { get; set; }
        private static List<string> RecipesList;

        public static void GetObjectInfoSource()
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
    }
}
