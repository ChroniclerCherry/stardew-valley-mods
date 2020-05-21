using Harmony;
using ShopTileFramework.Data;
using ShopTileFramework.Utility;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework.ItemPriceAndStock
{
    /// <summary>
    /// This class manages the total inventory for each shop
    /// </summary>
    class ItemPriceAndStockManager
    {
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        private ItemStock[] itemStocks;
        private readonly int maxNumItemsSoldInStore;
        private readonly string shopName;

        /// <summary>
        /// Initializes the manager with the itemstocks, and how many items max this shop will contain
        /// </summary>
        /// <param name="ItemStocks"></param>
        /// <param name="data"></param>
        public ItemPriceAndStockManager(ItemStock[] ItemStocks, ItemShopModel data)
        {
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Initializing Shop:" +
                    $" ShopName: {data.ShopName}" +
                    $" | StoreCurrency: {data.StoreCurrency}" +
                    $" | CategoriesToSellHere: {data.CategoriesToSellHere}" +
                    $" | PortraitPath: {data.PortraitPath}" +
                    $" | Quote: {data.Quote}" +
                    $" | ShopPrice: {data.ShopPrice}" +
                    $" | MaxNumItemsSoldInStore: {data.MaxNumItemsSoldInStore}" +
                    $" | When: {data.When}" +
                    $" | ClosedMessage: {data.ClosedMessage}\n", LogLevel.Debug);

            itemStocks = ItemStocks;
            maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
            shopName = data.ShopName;

            //initialize each stock
            foreach (ItemStock stock in ItemStocks)
            {
                stock.Initialize(data.ShopName,data.ShopPrice);
            }
        }

        public void AddToStock(ItemStock[] ItemStocks) => itemStocks.Concat(ItemStocks);

        public ItemPriceAndStockManager(ItemStock[] ItemStocks, VanillaShopModel data)
        {
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Initializing Vanilla shop:" +
                    $" ShopName: {data.ShopName}",
                    LogLevel.Debug);

            itemStocks = ItemStocks;
            maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
            shopName = data.ShopName;

            //initialize each stock
            foreach (ItemStock stock in ItemStocks)
            {
                stock.Initialize(data.ShopName, data.ShopPrice);
            }
        }

        /// <summary>
        /// Refreshes the stock of all items, doing condition checking and randomization
        /// </summary>
        public void Update()
        {
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"---------updating {shopName}--------------");

            foreach (ItemStock stock in itemStocks)
            {
                var PriceAndStock = stock.Update();
                //null is returned if conhditions aren't met, skip adding this stock
                if (PriceAndStock == null) 
                    continue;

                Add(PriceAndStock);
            }

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Reducing shop stock down to {maxNumItemsSoldInStore} items");
            //randomly reduces the stock of the whole store down to maxNumItemsSoldInStore
            ItemsUtil.RandomizeStock(ItemPriceAndStock,maxNumItemsSoldInStore);

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"---------finished updating {shopName}--------------");

        }

        /// <summary>
        /// Adds the stock from each ItemStock to the overall inventory
        /// </summary>
        /// <param name="dict"></param>
        private void Add(Dictionary<ISalable, int[]> dict)
        {
            foreach (var kvp in dict)
            {
                ItemPriceAndStock.Add(kvp.Key, kvp.Value);
            }
        }

    }
}
