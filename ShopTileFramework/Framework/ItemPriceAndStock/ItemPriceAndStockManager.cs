using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    class ItemPriceAndStockManager
    {
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        private ItemStock[] ItemStocks;
        public ItemShopData shopData { get; }

        public ItemPriceAndStockManager(ItemStock[] ItemStocks, ItemShopData data)
        {
            this.ItemStocks = ItemStocks;
            this.shopData = data;
        }

        public void Update()
        {
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            foreach (ItemStock stock in ItemStocks)
            {
                Add(stock.Update());
            }

            ItemsUtil.RandomizeStock(ItemPriceAndStock,shopData.MaxNumItemsSoldInStore);

        }

        public void Add(Dictionary<ISalable, int[]> dict)
        {
            foreach (var kvp in dict)
            {
                ItemPriceAndStock.Add(kvp.Key, kvp.Value);
            }
        }


    }
}
