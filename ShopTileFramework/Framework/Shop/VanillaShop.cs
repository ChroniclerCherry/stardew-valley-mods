using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.ItemPriceAndStock;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Shop
{
    class VanillaShop : VanillaShopModel
    {
        public List<ItemPriceAndStockManager> StockManagers { get; set; }
        public Dictionary<ISalable, ItemStockInformation> ItemPriceAndStock { get; set; }
        public IContentPack ContentPack { set; get; }
        public void Initialize()
        {
            StockManagers = new List<ItemPriceAndStockManager>();
        }

        public void UpdateItemPriceAndStock()
        {
            ItemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Debug);
            foreach (ItemPriceAndStockManager manager in StockManagers)
            {
                manager.Update();

                foreach ((ISalable item, ItemStockInformation stockInfo) in manager.ItemPriceAndStock)
                    ItemPriceAndStock[item] = stockInfo;
            }
        }
    }
}
