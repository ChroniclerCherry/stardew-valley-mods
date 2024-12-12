using System.Collections.Generic;
using ShopTileFramework.Framework.Shop;
using ShopTileFramework.Framework.Utility;
using StardewValley;

namespace ShopTileFramework.Framework.ItemPriceAndStock;

/// <summary>
/// This class manages the total inventory for each shop
/// </summary>
class ItemPriceAndStockManager
{
    public Dictionary<ISalable, ItemStockInformation> ItemPriceAndStock { get; set; }
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
        this._defaultSellPriceMultipler = data.DefaultSellPriceMultiplier;
        this._priceMultiplierWhen = data.PriceMultiplierWhen;
        this._itemStocks = data.ItemStocks;
        this._maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
        this._shopName = data.ShopName;
        this._shopPrice = data.ShopPrice;
    }

    public ItemPriceAndStockManager(VanillaShop data)
    {
        this._defaultSellPriceMultipler = data.DefaultSellPriceMultiplier;
        this._priceMultiplierWhen = data.PriceMultiplierWhen;
        this._itemStocks = data.ItemStocks;
        this._maxNumItemsSoldInStore = data.MaxNumItemsSoldInStore;
        this._shopName = data.ShopName;
        this._shopPrice = data.ShopPrice;
    }

    public void Initialize()
    {
        //initialize each stock
        foreach (ItemStock stock in this._itemStocks)
        {
            stock.Initialize(this._shopName, this._shopPrice, this._defaultSellPriceMultipler, this._priceMultiplierWhen);
        }
    }

    /// <summary>
    /// Refreshes the stock of all items, doing condition checking and randomization
    /// </summary>
    public void Update()
    {
        this.ItemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();
        ModEntry.monitor.Log($"Updating {this._shopName}");

        foreach (ItemStock stock in this._itemStocks)
        {
            var priceAndStock = stock.Update();
            //null is returned if conditions aren't met, skip adding this stock
            if (priceAndStock == null)
                continue;

            this.Add(priceAndStock);
        }

        //randomly reduces the stock of the whole store down to maxNumItemsSoldInStore
        ItemsUtil.RandomizeStock(this.ItemPriceAndStock, this._maxNumItemsSoldInStore);

    }

    /// <summary>
    /// Adds the stock from each ItemStock to the overall inventory
    /// </summary>
    /// <param name="dict"></param>
    private void Add(Dictionary<ISalable, ItemStockInformation> dict)
    {
        foreach (var kvp in dict)
        {
            this.ItemPriceAndStock.Add(kvp.Key, kvp.Value);
        }
    }
}
