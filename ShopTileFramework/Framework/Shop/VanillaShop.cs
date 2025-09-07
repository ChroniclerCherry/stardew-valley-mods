#nullable disable

using System.Collections.Generic;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.ItemPriceAndStock;
using StardewModdingAPI;
using StardewValley;

namespace ShopTileFramework.Framework.Shop;

internal class VanillaShop : VanillaShopModel
{
    /*********
    ** Accessors
    *********/
    public List<ItemPriceAndStockManager> StockManagers { get; set; }
    public Dictionary<ISalable, ItemStockInformation> ItemPriceAndStock { get; set; }


    /*********
    ** Public methods
    *********/
    public void Initialize()
    {
        this.StockManagers = [];
    }

    public void UpdateItemPriceAndStock()
    {
        this.ItemPriceAndStock = [];
        ModEntry.StaticMonitor.Log($"Generating stock for {this.ShopName}", LogLevel.Debug);
        foreach (ItemPriceAndStockManager manager in this.StockManagers)
        {
            manager.Update();

            foreach ((ISalable item, ItemStockInformation stockInfo) in manager.ItemPriceAndStock)
                this.ItemPriceAndStock[item] = stockInfo;
        }
    }
}
