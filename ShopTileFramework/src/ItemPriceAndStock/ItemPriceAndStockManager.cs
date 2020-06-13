using ShopTileFramework.Shop;
using ShopTileFramework.Utility;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.ItemPriceAndStock
{
    /// <summary>
    /// This class manages the total inventory for each shop
    /// </summary>
    class ItemPriceAndStockManager
    {
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        private readonly ItemStock[] _itemStocks;
        private readonly double _defaultSellPriceMultipler;
        private readonly Dictionary<double, string[]> _priceMultiplierWhen;
        private readonly int _maxNumItemsSoldInStore;
        private readonly string _shopName;
        private readonly int _shopPrice;

        /// <summary>
        /// Initializes the manager with the itemstocks, and how many items max this shop will contain
        /// </summary>
        /// <param name="data"></param>
        public ItemPriceAndStockManager(ItemShop data)
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

            _defaultSellPriceMultipler = data.DefaultSellPriceMultiplier;
            _priceMultiplierWhen = data.PriceMultiplierWhen;
            _itemStocks = data.ItemStocks;
            _maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
            _shopName = data.ShopName;
            _shopPrice = data.ShopPrice;
        }
        public ItemPriceAndStockManager(VanillaShop data)
        {
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Initializing Vanilla shop:" +
                    $" ShopName: {data.ShopName}",
                    LogLevel.Debug);

            _defaultSellPriceMultipler = data.DefaultSellPriceMultiplier;
            _priceMultiplierWhen = data.PriceMultiplierWhen;
            _itemStocks = data.ItemStocks;
            _maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
            _shopName = data.ShopName;
            _shopPrice = data.ShopPrice;
        }

        public void Initialize()
        {
            //initialize each stock
            foreach (ItemStock stock in _itemStocks)
            {
                stock.Initialize(_shopName, _shopPrice,_defaultSellPriceMultipler,_priceMultiplierWhen);
            }
        }

        /// <summary>
        /// Refreshes the stock of all items, doing condition checking and randomization
        /// </summary>
        public void Update()
        {
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"---------updating {_shopName}--------------");

            foreach (ItemStock stock in _itemStocks)
            {
                var priceAndStock = stock.Update();
                //null is returned if conhditions aren't met, skip adding this stock
                if (priceAndStock == null) 
                    continue;

                Add(priceAndStock);
            }

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Reducing shop stock down to {_maxNumItemsSoldInStore} items");
            //randomly reduces the stock of the whole store down to maxNumItemsSoldInStore
            ItemsUtil.RandomizeStock(ItemPriceAndStock,_maxNumItemsSoldInStore);

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"---------finished updating {_shopName}--------------");

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
