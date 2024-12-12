using System.Collections.Generic;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.ItemPriceAndStock;
using StardewModdingAPI;
using StardewValley;

namespace ShopTileFramework.Framework.Shop;

class VanillaShop : VanillaShopModel
{
    public List<ItemPriceAndStockManager> StockManagers { get; set; }
    public Dictionary<ISalable, ItemStockInformation> ItemPriceAndStock { get; set; }
    public IContentPack ContentPack { set; get; }
    public void Initialize()
    {
        this.StockManagers = new List<ItemPriceAndStockManager>();
    }

    public void UpdateItemPriceAndStock()
    {
        this.ItemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();
        ModEntry.monitor.Log($"Generating stock for {this.ShopName}", LogLevel.Debug);
        foreach (ItemPriceAndStockManager manager in this.StockManagers)
        {
            manager.Update();

            foreach ((ISalable item, ItemStockInformation stockInfo) in manager.ItemPriceAndStock)
                this.ItemPriceAndStock[item] = stockInfo;
        }
    }
}
