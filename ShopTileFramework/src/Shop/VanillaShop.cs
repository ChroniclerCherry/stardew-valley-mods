using ShopTileFramework.Data;
using ShopTileFramework.ItemPriceAndStock;
using StardewModdingAPI;
using System.Collections.Generic;

namespace ShopTileFramework.Shop
{
    class VanillaShop : VanillaShopModel
    {
        public List<ItemPriceAndStockManager> StockManagers { get; set; }
        public IContentPack ContentPack { set; get; }
        public void Initialize()
        {
            StockManagers = new List<ItemPriceAndStockManager>();
        }

        public void UpdateItemPriceAndStock()
        {
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Debug);
            foreach(ItemPriceAndStockManager manager in StockManagers)
            {
                manager.Update();
            }
        }
    }
}
