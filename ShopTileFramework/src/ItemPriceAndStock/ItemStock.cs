using ShopTileFramework.API;
using ShopTileFramework.Data;
using ShopTileFramework.Utility;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.ItemPriceAndStock
{
    /// <summary>
    /// This class stores the data for each stock, with a stock being a list of items of the same itemtype
    /// and sharing the same store parameters such as price
    /// </summary>
    class ItemStock : ItemStockModel
    {
        private int currencyObjectID;
        private ItemBuilder builder;
        private Dictionary<ISalable, int[]> ItemPriceAndStock;

        /// <summary>
        /// Initialize the ItemStock, doing error checking on the quality, and setting the price to the store price
        /// if none is given specifically for this stock.
        /// Creates the builder
        /// </summary>
        /// <param name="shopName"></param>
        /// <param name="price"></param>
        internal void Initialize(string shopName, int price)
        {
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Initializing Item Stock:" +
                    $" | ItemType:{ItemType}," +
                    $" | IsRecipe:{IsRecipe}" +
                    $" | StockPrice: {StockPrice}" +
                    $" | StockItemCurrency: {StockItemCurrency}" +
                    $" | StockCurrencyStack : {StockCurrencyStack}" +
                    $" | Quality: {Quality}" +
                    $" | Stock: {Stock}" +
                    $" | MaxNumItemsSoldInItemStock: {MaxNumItemsSoldInItemStock}" +
                    $" | When : {When}", LogLevel.Debug);

            if (Quality < 0 || Quality == 3 || Quality > 4)
            {
                Quality = 0;
                ModEntry.monitor.Log("Item quality can only be 0,1,2, or 4. Defaulting to 0", LogLevel.Warn);
            }

            currencyObjectID = ItemsUtil.GetIndexByName(StockItemCurrency);

            //sets price to the store price if no stock price is given
            if (StockPrice == -1)
            {
                StockPrice = price;
            }

            //initializes the builder with all the parameters of this itemstock
            builder = new ItemBuilder(itemType: ItemType,
                                      isRecipe: IsRecipe,
                                      price: StockPrice,
                                      currencyItemID: currencyObjectID,
                                      currencyItemStack: StockCurrencyStack,
                                      stock: Stock,
                                      quality: Quality,
                                      shopName: shopName);
        }

        /// <summary>
        /// Resets the items of this item stock, with condition checks and randomization
        /// </summary>
        /// <returns></returns>
        public Dictionary<ISalable, int[]> Update()
        {
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log("\tUpdating an ItemStock...",LogLevel.Debug);


            if (When != null && !ConditionChecking.CheckConditions(When))
                return null; //did not pass conditions

            if (!ItemsUtil.CheckItemType(ItemType)) //check that itemtype is valid
            {
                ModEntry.monitor.Log($"\t\"{ItemType}\" is not a valid ItemType. No items from this stock will be added."
                    , LogLevel.Warn);
                return null;
            }

            ItemPriceAndStock = new Dictionary<ISalable, int[]>();
            builder.setItemPriceAndStock(ItemPriceAndStock);

            AddByID();
            AddByName();
            AddByJAPack();

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"\tReducing this stock down to {MaxNumItemsSoldInItemStock} items\n\n");
            ItemsUtil.RandomizeStock(ItemPriceAndStock, MaxNumItemsSoldInItemStock);
            return ItemPriceAndStock;
        }

        /// <summary>
        /// Add all items listed in the ItemIDs section
        /// </summary>
        private void AddByID()
        {
            if (ItemIDs == null)
                return;

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Adding items by ID...", LogLevel.Debug);

            foreach (var ItemID in ItemIDs)
            {
                builder.AddItemToStock(ItemID);
            }
        }

        /// <summary>
        /// Add all items listed in the ItemNames section
        /// </summary>
        private void AddByName()
        {
            if (ItemNames == null)
                return;

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Adding items by Name...", LogLevel.Debug);

            foreach (var ItemName in ItemNames)
            {
                builder.AddItemToStock(ItemName);
            }

        }

        /// <summary>
        /// Add all items from the JA Packs listed in the JAPacks section
        /// </summary>
        private void AddByJAPack()
        {
            if (JAPacks == null)
                return;

            if (APIs.JsonAssets == null)
                return;

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Adding items by JA Pack...", LogLevel.Debug);

            foreach (var JAPack in JAPacks)
            {
                ModEntry.monitor.Log($"Adding all {ItemType}s from {JAPack}", LogLevel.Debug);

                if (ItemType == "Seed")
                {
                    var crops = APIs.JsonAssets.GetAllCropsFromContentPack(JAPack);
                    var trees = APIs.JsonAssets.GetAllFruitTreesFromContentPack(JAPack);


                    if (crops != null)
                    {
                        if (ModEntry.VerboseLogging)
                            ModEntry.monitor.Log($"Adding seeds of crops from {JAPack}", LogLevel.Debug);

                        foreach (string crop in crops)
                        {
                            int id = ItemsUtil.GetSeedID(crop);
                            if (id >0)
                                builder.AddItemToStock(id);
                        }
                    }

                    if (trees != null)
                    {
                        if (ModEntry.VerboseLogging)
                            ModEntry.monitor.Log($"Adding saplings of tree crops from {JAPack}", LogLevel.Debug);

                        foreach (string tree in trees)
                        {
                            int id = ItemsUtil.GetSaplingID(tree);
                            if (id > 0)
                                builder.AddItemToStock(id);
                        }
                    }

                    continue; //skip the rest of the loop so we don't also add the none-seed version
                }

                var packs = getJAItems(JAPack);
                if (packs == null)
                {
                    ModEntry.monitor.Log($"No {ItemType} from {JAPack} could be found", ModEntry.VerboseLogging? LogLevel.Debug : LogLevel.Trace);
                    continue;
                }

                foreach (string itemName in packs)
                {
                    builder.AddItemToStock(itemName);
                }
            }
        }

        /// <summary>
        /// Depending on the itemtype, returns a list of the names of all items of that type in a JA pack
        /// </summary>
        /// <param name="JAPack">Unique ID of the pack</param>
        /// <returns>A list of all the names of the items of the right item type in that pack</returns>
        private List<string> getJAItems(string JAPack)
        {
            switch (ItemType)
            {
                case "Object":
                    return APIs.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                case "BigCraftable":
                    return APIs.JsonAssets.GetAllBigCraftablesFromContentPack(JAPack);
                case "Clothing":
                    return APIs.JsonAssets.GetAllClothingFromContentPack(JAPack);
                case "Ring":
                    return APIs.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                case "Hat":
                    return APIs.JsonAssets.GetAllHatsFromContentPack(JAPack);
                case "Weapon":
                    return APIs.JsonAssets.GetAllWeaponsFromContentPack(JAPack);
                default:
                    return null;
            }
        }

    }
}
