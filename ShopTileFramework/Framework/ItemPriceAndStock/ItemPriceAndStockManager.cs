using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    /// <summary>
    /// This class manages the total inventory for each shop
    /// </summary>
    class ItemPriceAndStockManager
    {
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        private readonly ItemStock[] ItemStocks;
        private readonly int maxNumItemsSoldInStore;

        /// <summary>
        /// Initializes the manager with the itemstocks, and how many items max this shop will contain
        /// </summary>
        /// <param name="ItemStocks"></param>
        /// <param name="data"></param>
        public ItemPriceAndStockManager(ItemStock[] ItemStocks, ItemShopModel data)
        {
            this.ItemStocks = ItemStocks;
            this.maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;

            //initialize each stock
            foreach (ItemStock stock in ItemStocks)
            {
                stock.Initialize(data.ShopName,data.ShopPrice);
            }
        }

        /// <summary>
        /// Refreshes the stock of all items, doing condition checking and randomization
        /// </summary>
        public void Update()
        {
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            foreach (ItemStock stock in ItemStocks)
            {
                var PriceAndStock = stock.Update();
                //null is returned if conhditions aren't met, skip adding this stock
                if (PriceAndStock == null) 
                    continue;

                Add(PriceAndStock);
            }

            //randomly reduces the stock of the whole store down to maxNumItemsSoldInStore
            ItemsUtil.RandomizeStock(ItemPriceAndStock,maxNumItemsSoldInStore);

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
