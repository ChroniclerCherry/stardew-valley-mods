#nullable disable

using System.Collections.Generic;
using ShopTileFramework.Framework.Shop;
using ShopTileFramework.Framework.Utility;
using StardewValley;

namespace ShopTileFramework.Framework.ItemPriceAndStock;

/// <summary>
/// This class manages the total inventory for each shop
/// </summary>
internal class ItemPriceAndStockManager
{
    /*********
    ** Fields
    *********/
    private readonly ItemStock[] ItemStocks;
    private readonly double DefaultSellPriceMultipler;
    private readonly Dictionary<double, string[]> PriceMultiplierWhen;
    private readonly int MaxNumItemsSoldInStore;
    private readonly string ShopName;
    private readonly int ShopPrice;


    /*********
    ** Accessors
    *********/
    public Dictionary<ISalable, ItemStockInformation> ItemPriceAndStock { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>
    /// Initializes the manager with the itemstocks, and how many items max this shop will contain
    /// </summary>
    /// <param name="data"></param>
    public ItemPriceAndStockManager(ItemShop data)
    {
        this.DefaultSellPriceMultipler = data.DefaultSellPriceMultiplier;
        this.PriceMultiplierWhen = data.PriceMultiplierWhen;
        this.ItemStocks = data.ItemStocks;
        this.MaxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
        this.ShopName = data.ShopName;
        this.ShopPrice = data.ShopPrice;
    }

    public ItemPriceAndStockManager(VanillaShop data)
    {
        this.DefaultSellPriceMultipler = data.DefaultSellPriceMultiplier;
        this.PriceMultiplierWhen = data.PriceMultiplierWhen;
        this.ItemStocks = data.ItemStocks;
        this.MaxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
        this.ShopName = data.ShopName;
        this.ShopPrice = data.ShopPrice;
    }

    public void Initialize()
    {
        //initialize each stock
        foreach (ItemStock stock in this.ItemStocks)
        {
            stock.Initialize(this.ShopName, this.ShopPrice, this.DefaultSellPriceMultipler, this.PriceMultiplierWhen);
        }
    }

    /// <summary>
    /// Refreshes the stock of all items, doing condition checking and randomization
    /// </summary>
    public void Update()
    {
        this.ItemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();
        ModEntry.StaticMonitor.Log($"Updating {this.ShopName}");

        foreach (ItemStock stock in this.ItemStocks)
        {
            var priceAndStock = stock.Update();
            //null is returned if conditions aren't met, skip adding this stock
            if (priceAndStock == null)
                continue;

            this.Add(priceAndStock);
        }

        //randomly reduces the stock of the whole store down to maxNumItemsSoldInStore
        ItemsUtil.RandomizeStock(this.ItemPriceAndStock, this.MaxNumItemsSoldInStore);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>
    /// Adds the stock from each ItemStock to the overall inventory
    /// </summary>
    /// <param name="dict"></param>
    private void Add(Dictionary<ISalable, ItemStockInformation> dict)
    {
        foreach ((ISalable item, ItemStockInformation stock) in dict)
        {
            this.ItemPriceAndStock.Add(item, stock);
        }
    }
}
