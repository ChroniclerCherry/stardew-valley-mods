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
        private Texture2D Portrait = null;
        internal string PortraitPath { get; }
        private readonly string Quote;
        private readonly int ShopPrice;
        internal ItemStock[] ItemStocks { get; }
        private readonly int MaxNumItemsSoldInStore;
        internal Dictionary<ISalable, int[]> ItemPriceAndStock;

        public string ShopName { get; }
        private string[] OpenConditions;
        private string ClosedMessage;
        private readonly string StoreCurrency;
        private List<int> CategoriesToSellHere;

        private static Dictionary<string, IDictionary<int, string>> ObjectInfoSource;

        private static List<string> RecipesList;

        Dictionary<string,string> LocalizedQuote;
        Dictionary<string, string> LocalizedClosedMessage;

        private IContentPack contentPack;

        public Shop(ShopPack pack, IContentPack contentPack)
        {
            ShopName = pack.ShopName;
            OpenConditions = pack.When;
            ClosedMessage = pack.ClosedMessage;
            ShopPrice = pack.ShopPrice;
            StoreCurrency = pack.StoreCurrency;
            CategoriesToSellHere = pack.CategoriesToSellHere;
            ItemStocks = pack.ItemStocks;
            Quote = pack.Quote;
            MaxNumItemsSoldInStore = pack.MaxNumItemsSoldInStore;
            LocalizedQuote = pack.LocalizedQuote;
            LocalizedClosedMessage = pack.LocalizedClosedMessage;
            PortraitPath = pack.PortraitPath;
            this.contentPack = contentPack;

            UpdatePortrait();
        }

        public void UpdatePortrait()
        {
            if (PortraitPath == null)
                return;

            string seasonalPath = PortraitPath.Insert(PortraitPath.IndexOf('.'), "_" + Game1.currentSeason);
            try
            {
                if (contentPack.HasFile(seasonalPath))
                {
                    Portrait = contentPack.LoadAsset<Texture2D>(seasonalPath);
                }
                else if (contentPack.HasFile(PortraitPath))
                {
                    Portrait = contentPack.LoadAsset<Texture2D>(PortraitPath);
                }
            }
            catch (Exception ex)
            {
                ModEntry.monitor.Log(ex.Message, LogLevel.Warn);
            }

        }

        public void UpdateItemPriceAndStock()
        {
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Debug);
            //list of all items from this ItemStock
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            foreach (ItemStock Inventory in ItemStocks)
            {
                int quality = Inventory.Quality;

                if (quality < 0 || quality == 3 || quality > 4)
                {
                    quality = 0;
                    ModEntry.monitor.Log("Item quality can only be 0,1,2, or 4", LogLevel.Trace);
                }

                if (Inventory.When != null && !ConditionChecking.CheckConditions(Inventory.When))
                    continue;

                if (Inventory.ItemType != "Seed" && !ObjectInfoSource.ContainsKey(Inventory.ItemType))
                {
                    ModEntry.monitor.Log($" \"{Inventory.ItemType}\" is not a valid ItemType. Some items will not be added.", LogLevel.Warn);
                    continue;
                }

                int CurrencyItemID = GetIndexByName(Inventory.StockItemCurrency, Game1.objectInformation);

                int CurrencyItemStack = Inventory.StockCurrencyStack;

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
                        AddItem(Inventory.ItemType, ItemID, Inventory.IsRecipe, Price, Inventory.Stock, ItemStockInventory, quality, CurrencyItemID, CurrencyItemStack);
                    }
                }

                //add in all items specified by name
                if (Inventory.ItemNames != null)
                {
                    foreach (var ItemName in Inventory.ItemNames)
                    {
                        AddItem(Inventory.ItemType, ItemName, Inventory.IsRecipe, Price, Inventory.Stock, ItemStockInventory, quality, CurrencyItemID, CurrencyItemStack);
                    }
                }

                if (Inventory.JAPacks != null && ModEntry.JsonAssets != null)
                {   //add in all items from specified JA packs
                    foreach (var JAPack in Inventory.JAPacks)
                    {
                        ModEntry.monitor.Log($"Adding objects from JA pack {JAPack}", LogLevel.Trace);

                        if (Inventory.ItemType == "Seed")
                        {
                            //add seeds
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
                                            AddItem("Object", kvp.Key, false, Price, Inventory.Stock, ItemStockInventory, quality, CurrencyItemID, CurrencyItemStack);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No Crops from {JAPack} could be found. No seeds are added.", LogLevel.Trace);
                            }

                            //add tree saplings
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
                                        Int32.TryParse(kvp.Value.Split('/')[0], out int id);
                                        if (TreeID == id)
                                        {
                                            AddItem("Object", kvp.Key, Inventory.IsRecipe, Price, Inventory.Stock, ItemStockInventory, quality, CurrencyItemID, CurrencyItemStack);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No Trees from {JAPack} could not be found. No saplings are added.", LogLevel.Trace);
                            }
                        }
                        else if (Inventory.ItemType == "Object")
                        {


                            //add all other objects from the JA pack
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string ItemName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, ItemName, Inventory.IsRecipe, Price, Inventory.Stock, ItemStockInventory, quality, CurrencyItemID, CurrencyItemStack);
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No Objects from {JAPack} could be found. No items are added.", LogLevel.Trace);
                            }
                        }
                        else if (Inventory.ItemType == "BigCraftable")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllBigCraftablesFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string CraftableName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, CraftableName, Inventory.IsRecipe, Price, Inventory.Stock, ItemStockInventory, 0, CurrencyItemID, CurrencyItemStack);
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No BigCraftable from {JAPack} could be found. No items are added.", LogLevel.Trace);
                            }
                        }
                        else if (Inventory.ItemType == "Hat")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllHatsFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string HatName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, HatName, false, Price, Inventory.Stock, ItemStockInventory, 0, CurrencyItemID, CurrencyItemStack);
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No Hats from {JAPack} could be found. No items are added.", LogLevel.Trace);
                            }
                        }
                        else if (Inventory.ItemType == "Weapon")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllWeaponsFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string WeaponName in attemptToGetPack)
                                {
                                    AddItem(Inventory.ItemType, WeaponName, false, Price, Inventory.Stock, ItemStockInventory, 0, CurrencyItemID, CurrencyItemStack);
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No Weapons from {JAPack} could be found. No items are added.", LogLevel.Trace);
                            }
                        }
                        else if (Inventory.ItemType == "Clothing")
                        {
                            var attemptToGetPack = ModEntry.JsonAssets.GetAllClothingFromContentPack(JAPack);
                            if (attemptToGetPack != null)
                            {
                                foreach (string ClothingName in ModEntry.JsonAssets.GetAllClothingFromContentPack(JAPack))
                                {
                                    AddItem(Inventory.ItemType, ClothingName, false, Price, Inventory.Stock, ItemStockInventory, 0, CurrencyItemID, CurrencyItemStack);
                                }
                            }
                            else
                            {
                                ModEntry.monitor.Log($"No Clothing from {JAPack} could be found. No items are added.", LogLevel.Trace);
                            }

                        }
                    }
                }
                //randomly reduce the ItemStock's inventory to the specified MaxNumItemsSoldInItemStock
                RandomizeStock(ItemStockInventory, Inventory.MaxNumItemsSoldInItemStock);
                AddToItemPriceAndStock(ItemStockInventory);

            }
            //randomly reduce store's entire inventory to the specified MaxNumItemsSoldInStore
            RandomizeStock(ItemPriceAndStock, MaxNumItemsSoldInStore);

        }

        private void AddToItemPriceAndStock(Dictionary<ISalable, int[]> dict)
        {
            foreach (var kvp in dict)
            {
                ItemPriceAndStock.Add(kvp.Key, kvp.Value);
            }
        }
        private static void RandomizeStock(Dictionary<ISalable, int[]> inventory, int MaxNum)
        {
            while (inventory.Count > MaxNum)
            {
                inventory.Remove(inventory.Keys.ElementAt(Game1.random.Next(inventory.Count)));
            }

        }

        private void AddItem(string ItemType,
            int itemID,
            bool isRecipe,
            int Price,
            int Stock,
            Dictionary<ISalable, int[]> itemStockInventory,
            int quality,
            int ItemCurrencyID = -1,
            int ItemCurrencyStack = -1)
        {
            Item i = GetItem(ItemType, itemID, Stock, isRecipe, quality);

            if (i == null)
            {
                ModEntry.monitor.Log($"Item of ID {itemID} could not be added to {ShopName}", LogLevel.Debug);
                return;
            }

            if (isRecipe)
            {
                Stock = 1;
                if (!RecipesList.Contains(i.Name))
                {
                    ModEntry.monitor.Log($"{i.Name} is not a valid recipe and won't be added.");
                    return;
                }
            }

            int[] PriceStockCurrency;
            var price = (Price == -1) ? i.salePrice() : Price;
            if (ItemCurrencyID == -1)
            {
                PriceStockCurrency = new int[] { price, Stock };

            }
            else if (ItemCurrencyStack == -1)
            {
                PriceStockCurrency = new int[] { price, Stock, ItemCurrencyID };
            }
            else
            {
                PriceStockCurrency = new int[] { price, Stock, ItemCurrencyID, ItemCurrencyStack };
            }

            itemStockInventory.Add(i, PriceStockCurrency);

        }

        private void AddItem(string ItemType,
            String ItemName,
            bool isRecipe,
            int Price,
            int Stock,
            Dictionary<ISalable, int[]> itemStockInventory,
            int quality,
            int ItemCurrencyID = -1,
            int ItemCurrencyStack = -1)
        {
            Item i = GetItem(ItemType, ItemName, Stock, isRecipe,quality);

            if (i == null)
            {
                ModEntry.monitor.Log($"{ItemType} named \"{ItemName}\" could not be added to {ShopName}", LogLevel.Debug);
                return;
            }

            if (isRecipe)
            {
                Stock = 1;
                if (!RecipesList.Contains(i.Name))
                {
                    ModEntry.monitor.Log($"{i.Name} is not a valid recipe and won't be added.");
                    return;
                }
            }

            int[] PriceStockCurrency;
            var price = (Price == -1) ? i.salePrice() : Price;
            if (ItemCurrencyID == -1)
            {
                PriceStockCurrency = new int[] { price, Stock };
            }
            else if (ItemCurrencyStack == -1)
            {
                PriceStockCurrency = new int[] { price, Stock, ItemCurrencyID };
            }
            else
            {
                PriceStockCurrency = new int[] { price, Stock, ItemCurrencyID, ItemCurrencyStack };
            }
            itemStockInventory.Add(i, PriceStockCurrency);
        }
        private static Item GetItem(string objectType,
            int index,
            int stock,
            bool isRecipe,
            int quality)
        {
            Item item = null;

            if (index == -1)
            {
                return null;
            }
            switch (objectType)
            {
                case "Object":
                    item = new StardewValley.Object(index, stock, isRecipe,quality:quality);
                    break;
                case "BigCraftable":
                    item = new StardewValley.Object(Vector2.Zero, index) { Stack = stock, IsRecipe = isRecipe };
                    break;
                case "Clothing":
                    item = new Clothing(index);
                    break;
                case "Ring":
                    item = new Ring(index);
                    break;
                case "Hat":
                    item = new Hat(index);
                    break;
                case "Boot":
                    item = new Boots(index);
                    break;
                case "Furniture":
                    item = new Furniture(index, Vector2.Zero);
                    break;
                case "Weapon":
                    item = new MeleeWeapon(index);
                    break;
            }
            return item;
        }

        private static Item GetItem(string objectType,
                                    string name,
                                    int stock,
                                    bool isRecipe,
                                    int quality)
        {

            if (name == null)
                return null;

            ObjectInfoSource.TryGetValue(objectType, out var InfoSource);
            if (InfoSource != null)
            {
                return GetItem(objectType, GetIndexByName(name, InfoSource), stock, isRecipe,quality);
            }
            else
            {
                return null;
            }

        }

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

        private static int GetIndexByName(
            string name,
            IDictionary<int, string> ObjectInfo)
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

        public void DisplayShop()
        {
            ModEntry.monitor.Log($"Atempting to open the shop \"{ShopName}\"");
            if (ConditionChecking.CheckConditions(OpenConditions))
            {
                int currency = 0;
                switch (StoreCurrency)
                {
                    case "festivalScore":
                        currency = 1;
                        break;
                    case "clubCoins":
                        currency = 2;
                        break;
                }

                var ShopMenu = new ShopMenu(ItemPriceAndStock,
                    currency: currency);
                if (CategoriesToSellHere != null)
                    ShopMenu.categoriesToSellHere = CategoriesToSellHere;

                ShopMenu.portraitPerson = new NPC();
                ShopMenu.portraitPerson.Name = "STF." + ShopName;
                if (Portrait != null)
                {
                    ShopMenu.portraitPerson.Portrait = Portrait;
                }

                if (Quote != null)
                {
                    ShopMenu.potraitPersonDialogue = Game1.parseText(ModEntry.localize(Quote, LocalizedQuote), Game1.dialogueFont, 304);
                }

                Game1.activeClickableMenu = ShopMenu;
            }
            else if (ClosedMessage != null)
            {
                Game1.activeClickableMenu = new DialogueBox(ModEntry.localize(ClosedMessage, LocalizedClosedMessage));
            }

        }
    }
}
