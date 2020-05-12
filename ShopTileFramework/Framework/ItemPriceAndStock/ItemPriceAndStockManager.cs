using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    class ItemPriceAndStockManager
    {
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        private readonly ItemStock[] ItemStocks;
        public ItemShopModel shopData;

        public ItemPriceAndStockManager(ItemStock[] ItemStocks, ItemShopModel data)
        {
            this.ItemStocks = ItemStocks;
            this.shopData = data;

            foreach (ItemStock stock in ItemStocks)
            {
                stock.Initialize(data);
            }
        }

        public void Update()
        {
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            foreach (ItemStock stock in ItemStocks)
            {
                var PriceAndStock = stock.Update();
                if (PriceAndStock == null)
                    continue;

                Add(PriceAndStock);
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
