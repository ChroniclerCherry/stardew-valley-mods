using System;
using System.Collections.Generic;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace ShopTileFramework.Framework.ItemPriceAndStock;

/// <summary>
/// This class stores the global data for each itemstock, in order to generate and add items by ID or name
/// to the stock
/// </summary>
internal class ItemBuilder
{
    /*********
    ** Fields
    *********/
    private Dictionary<ISalable, ItemStockInformation> ItemPriceAndStock;
    private readonly ItemStock ItemStock;


    /*********
    ** Public methods
    *********/
    public ItemBuilder(ItemStock itemStock)
    {
        this.ItemStock = itemStock;
    }

    /// <param name="itemPriceAndStock">the ItemPriceAndStock this builder will add items to</param>
    public void SetItemPriceAndStock(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock)
    {
        this.ItemPriceAndStock = itemPriceAndStock;
    }

    /// <summary>Add an item to the stock.</summary>
    /// <param name="itemIdOrName">The item ID or internal item name.</param>
    /// <param name="priceMultiplier"></param>
    public bool AddItemToStock(string itemIdOrName, double priceMultiplier = 1)
    {
        string itemId =
            ItemRegistry.GetData(itemIdOrName)?.ItemId
            ?? ItemsUtil.GetItemIdByName(itemIdOrName, this.ItemStock.ItemType);

        if (itemId is null)
        {
            ModEntry.StaticMonitor.Log($"{this.ItemStock.ItemType} with the name or ID \"{itemIdOrName}\" could not be added to the shop {this.ItemStock.ShopName}");
            return false;
        }

        if (ModEntry.VerboseLogging)
            ModEntry.StaticMonitor.Log($"Adding item ID {itemId} to {this.ItemStock.ShopName}", LogLevel.Debug);

        if (this.ItemStock.ItemType == "Seed" && this.ItemStock.FilterSeedsBySeason)
        {
            if (!ItemsUtil.IsInSeasonCrop(itemId)) return false;
        }

        var item = this.CreateItem(itemId);
        if (item == null)
        {
            return false;
        }

        if (this.ItemStock.IsRecipe)
        {
            if (!ItemsUtil.RecipesList.Contains(item.Name))
            {
                ModEntry.StaticMonitor.Log($"{item.Name} is not a valid recipe and won't be added.");
                return false;
            }
        }

        var priceStockCurrency = this.GetPriceStockAndCurrency(item, priceMultiplier);
        this.ItemPriceAndStock.Add(item, priceStockCurrency);

        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>
    /// Given an itemID, return an instance of that item with the parameters saved in this builder
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    private Item CreateItem(string itemId)
    {
        Item item = ItemRegistry.Create(itemId, 1, this.ItemStock.Quality);

        if (item is Object obj)
            obj.IsRecipe = this.ItemStock.IsRecipe;

        return item;
    }

    /// <summary>
    /// Creates the second parameter in ItemStockAndPrice, an array that holds info on the price, stock,
    /// and if it exists, the item currency it takes
    /// </summary>
    /// <param name="item">An instance of the item</param>
    /// <param name="priceMultiplier"></param>
    /// <returns>The array that's the second parameter in ItemPriceAndStock</returns>
    private ItemStockInformation GetPriceStockAndCurrency(ISalable item, double priceMultiplier)
    {
        //if no price is provided, use the item's sale price multiplied by defaultSellPriceMultiplier
        int price = this.ItemStock.StockPrice == -1
            ? (int)(item.salePrice() * this.ItemStock.DefaultSellPriceMultiplier)
            : this.ItemStock.StockPrice;
        price = (int)(price * priceMultiplier);

        int? currencyObjectStack = this.ItemStock.CurrencyObjectId != null
            ? Math.Max(this.ItemStock.StockCurrencyStack, 1)
            : null;

        return new ItemStockInformation(price, this.ItemStock.Stock, this.ItemStock.CurrencyObjectId, currencyObjectStack);
    }
}
