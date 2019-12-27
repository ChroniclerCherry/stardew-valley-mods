using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework
{
    class Shop
    {
        public string ShopName { get; set; }
        private static Random random = new Random();
        private Texture2D Portrait = null;
        private string Quote { get; set; }
        private int ShopPrice { get; set; }
        private ItemStock[] ItemStocks { get; set; }
        private int MaxNumItemsSoldInStore { get; set; }
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        public Shop(ShopPack pack, IContentPack contentPack)
        {
            ShopName = pack.ShopName;
            ItemStocks = pack.ItemStocks;
            Quote = pack.Quote;
            MaxNumItemsSoldInStore = pack.MaxNumItemsSoldInStore;

            //try and load in the portrait
            if (pack.PortraitPath != null)
            {
                try
                {
                    Portrait = contentPack.LoadAsset<Texture2D>(pack.PortraitPath);
                }
                catch (Exception ex)
                {
                    ModEntry.monitor.Log(ex.Message, LogLevel.Warn);
                }
            }
        }

        public void UpdateItemPriceAndStock()
        {
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Trace);
            //list of all items from this ItemStock
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            foreach (ItemStock Inventory in ItemStocks)
            {
                Dictionary<ISalable, int[]> ItemStockInventory = new Dictionary<ISalable, int[]>();
                //StockPrice overrides ShopPrice
                int Price = Inventory.StockPrice;
                if (Price == -1)
                {
                    Price = ShopPrice;
                }

                //add in all items specified by index
                if (Inventory.ItemIDs != null)
                {
                    foreach (var ItemID in Inventory.ItemIDs)
                    {
                        AddItem(Inventory.ItemType, ItemID, Price, Inventory.Stock, ItemStockInventory);
                    }
                }

                //add in all items specified by name
                if (Inventory.ItemNames != null)
                {
                    foreach (var ItemName in Inventory.ItemNames)
                    {
                        AddItem(Inventory.ItemType, ItemName, Price, Inventory.Stock, ItemStockInventory);
                    }
                }

                //add in all items from specified JA packs
                if (Inventory.JAPacks != null && ModEntry.JsonAssets != null)
                {
                    foreach (var JAPack in Inventory.JAPacks)
                    {
                        ModEntry.monitor.Log($"Adding objects from JA pack {JAPack}", LogLevel.Trace);

                        if (Inventory.ItemType == "Object")
                        {
                            var ObjectData = Game1.objectInformation;
                            var CropData = ModEntry.helper.Content.Load<Dictionary<int,
                                string>>(@"Data/Crops", ContentSource.GameContent);
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllCropsFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string ItemName in attemptToGetPack)
                                {
                                    var CropID = ModEntry.JsonAssets.GetCropId(ItemName);
                                    foreach (KeyValuePair<int, string> kvp in CropData)
                                    {
                                        //find the crop id in crop information to get seed id
                                        Int32.TryParse(kvp.Value.Split('/')[2], out int id);
                                        if (CropID == id)
                                        {
                                            AddItem(Inventory.ItemType, kvp.Key, Price, Inventory.Stock, ItemStockInventory);
                                        }
                                    }
                                }
                            }
                            var FruitTreeData = ModEntry.helper.Content.Load<Dictionary<int,
                                string>>(@"Data/fruitTrees", ContentSource.GameContent);
                            attemptToGetPack = ModEntry.JsonAssets.GetAllFruitTreesFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string ItemName in attemptToGetPack)
                                {
                                    var TreeID = ModEntry.JsonAssets.GetFruitTreeId(ItemName);

                                    foreach (KeyValuePair<int, string> kvp in FruitTreeData)
                                    {
                                        //find the tree id in fruitTrees information to get sapling id
                                        Int32.TryParse(kvp.Value.Split('/')[1], out int id);
                                        if (TreeID == id)
                                        {
                                            AddItem(Inventory.ItemType, kvp.Key, Price, Inventory.Stock, ItemStockInventory);
                                        }
                                    }
                                }
                            }

                        }
                        else if (Inventory.ItemType == "BigCraftable")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllBigCraftablesFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string CraftableName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, CraftableName, Price, Inventory.Stock, ItemStockInventory);
                                }
                            }
                        }
                        else if (Inventory.ItemType == "Hat")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllHatsFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string HatName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, HatName, Price, Inventory.Stock, ItemStockInventory);
                                }
                            }
                        }
                        else if (Inventory.ItemType == "Weapon")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllWeaponsFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string WeaponName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, WeaponName, Price, Inventory.Stock, ItemStockInventory);
                                }
                            }
                        }
                        else if (Inventory.ItemType == "Clothing")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllClothingFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string ClothingName in ModEntry.JsonAssets.GetAllClothingFromContentPack(JAPack))
                                {
                                    AddItem(Inventory.ItemType, ClothingName, Price, Inventory.Stock, ItemStockInventory);
                                }
                            }

                        }
                    }
                }
                ModEntry.monitor.Log($"Randomizing {Inventory.ItemType}s and max stock is {Inventory.MaxNumItemsSoldInItemStock}", LogLevel.Debug);
                randomizeStock(ItemStockInventory, Inventory.MaxNumItemsSoldInItemStock);
                AddToItemPriceAndStock(ItemStockInventory);

            }
            ModEntry.monitor.Log($"Randomizing {ShopName} and max stock is {MaxNumItemsSoldInStore}", LogLevel.Debug);
            //randomly reduce store's entire inventory to the specified MaxNumItemsSoldInStore
            randomizeStock(ItemPriceAndStock, MaxNumItemsSoldInStore);
        }

        private void AddToItemPriceAndStock(Dictionary<ISalable, int[]> dict)
        {
            foreach (var stuff in dict)
            {
                ItemPriceAndStock.Add(stuff.Key,stuff.Value);
            }
        }
        private void randomizeStock (Dictionary<ISalable, int[]> inventory, int MaxNum)
        {
            while (inventory.Count > MaxNum)
            {
                inventory.Remove(inventory.Keys.ElementAt(random.Next(inventory.Count - 1)));
            }
            
        }

        private void AddItem(String ItemType,int itemID,int Price, int Stock, Dictionary<ISalable, int[]> itemStockInventory)
        {
            var i = GetItem(ItemType, itemID);
            if (i != null)
            {
                itemStockInventory.Add(i, new int[] { (Price == -1)
                                                    ? i.salePrice() : Price, Stock });
            }
            else
            {
                ModEntry.monitor.Log($"Crop of {itemID} " +
                    $"named could not be added to {ShopName}",
                    LogLevel.Warn);
            }
        }

        private void AddItem(String ItemType, String ItemName, int Price, int Stock, Dictionary<ISalable, int[]> itemStockInventory)
        {
            var i = GetItem(ItemType, ItemName);
            if (i != null)
            {
                itemStockInventory.Add(i, new int[] { (Price == -1)
                                                    ? i.salePrice() : Price, Stock });
            }
            else
            {
                ModEntry.monitor.Log($"{ItemType} named "  +
                    $"\"{ItemName}\" could not be added to {ShopName}",
                    LogLevel.Warn);
            }
        }

        private Item GetItem(string objectType, int index)
        {
            Item item = null;
            if (objectType == "Object")
            {
                if (index != -1)
                    item = new StardewValley.Object(index, 1);
            }
            else if (objectType == "BigCraftable")
            {
                if (index != -1)
                    item = new StardewValley.Object(Vector2.Zero, index);
            }
            else if (objectType == "Clothing")
            {
                if (index != -1)
                    item = new Clothing(index);
            }
            else if (objectType == "Ring")
            {
                if (index != -1)
                    item = new Ring(index);
            }
            else if (objectType == "Hat")
            {
                if (index != -1)
                    item = new Hat(index);
            }
            else if (objectType == "Boots")
            {
                if (index != -1)
                    item = new Boots(index);
            }
            else if (objectType == "Furniture")
            {
                if (index != -1)
                    item = new Furniture(index, Vector2.Zero);
            }
            else if (objectType == "Weapon")
            {
                if (index != -1)
                    item = new MeleeWeapon(index);
            }

            return item;
        }

        private Item GetItem(string objectType, string name)
        {
            Item item = null;
            int index = -1;

            if (name != null)
            {
                if (objectType == "Object")
                {
                    index = GetIndexByName(name, Game1.objectInformation);
                }
                else if (objectType == "Ring")
                {
                    index = GetIndexByName(name, Game1.bigCraftablesInformation);
                }
                else if (objectType == "BigCraftable")
                {
                    index = GetIndexByName(name, Game1.bigCraftablesInformation);
                }
                else if (objectType == "Clothing")
                {
                    index = GetIndexByName(name, Game1.clothingInformation);
                }
                else if (objectType == "Hat")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>
                        (@"Data/hats", ContentSource.GameContent));
                }
                else if (objectType == "Boots")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>
                        (@"Data/Boots", ContentSource.GameContent));

                }
                else if (objectType == "Furniture")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>
                        (@"Data/Furniture", ContentSource.GameContent));
                }
                else if (objectType == "Weapon")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>
                        (@"Data/weapons", ContentSource.GameContent));
                }
                return GetItem(objectType, index);
            }
            return item;
        }

        private int GetIndexByName(string name, IDictionary<int, string> ObjectInfo)
        {
            foreach (KeyValuePair<int, string> kvp in ObjectInfo)
            {
                if (kvp.Value.StartsWith(name))
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        public void DisplayStore()
        {
            Game1.activeClickableMenu = new ShopMenu(ItemPriceAndStock);
        }
    }
}
