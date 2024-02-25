using System;
using System.Collections.Generic;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    /// <summary>
    /// This class stores the global data for each itemstock, in order to generate and add items by ID or name
    /// to the stock
    /// </summary>
    class ItemBuilder
    {
        private Dictionary<ISalable, ItemStockInformation> _itemPriceAndStock;
        private readonly ItemStock _itemStock;

        public ItemBuilder(ItemStock itemStock)
        {
            this._itemStock = itemStock;
        }

        /// <param name="itemPriceAndStock">the ItemPriceAndStock this builder will add items to</param>
        public void SetItemPriceAndStock(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock)
        {
            this._itemPriceAndStock = itemPriceAndStock;
        }

        /// <summary>Add an item to the stock.</summary>
        /// <param name="itemIdOrName">The item ID or internal item name.</param>
        /// <param name="priceMultiplier"></param>
        public bool AddItemToStock(string itemIdOrName, double priceMultiplier = 1)
        {
            string itemId =
                ItemRegistry.GetData(itemIdOrName)?.ItemId
                ?? ItemsUtil.GetItemIdByName(itemIdOrName, _itemStock.ItemType);

            if (itemId is null)
            {
                ModEntry.monitor.Log($"{_itemStock.ItemType} with the name or ID \"{itemIdOrName}\" could not be added to the shop {_itemStock.ShopName}", LogLevel.Trace);
                return false;
            }

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Adding item ID {itemId} to {_itemStock.ShopName}", LogLevel.Debug);

            if (_itemStock.ItemType == "Seed" && _itemStock.FilterSeedsBySeason)
            {
                if (!ItemsUtil.IsInSeasonCrop(itemId)) return false;
            }

            var item = CreateItem(itemId);
            if (item == null)
            {
                return false;
            }

            if (_itemStock.IsRecipe)
            {
                if (!ItemsUtil.RecipesList.Contains(item.Name))
                {
                    ModEntry.monitor.Log($"{item.Name} is not a valid recipe and won't be added.", LogLevel.Trace);
                    return false;
                }
            }

            var priceStockCurrency = GetPriceStockAndCurrency(item, priceMultiplier);
            _itemPriceAndStock.Add(item, priceStockCurrency);

            return true;       
        }

        /// <summary>
        /// Given an itemID, return an instance of that item with the parameters saved in this builder
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private Item CreateItem(string itemId)
        {
            Item item = ItemRegistry.Create(itemId, _itemStock.Stock, _itemStock.Quality);

            if (item is Object obj)
                obj.IsRecipe = _itemStock.IsRecipe;

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
            int price = _itemStock.StockPrice == -1
                ? (int)(item.salePrice() * _itemStock.DefaultSellPriceMultiplier)
                : _itemStock.StockPrice;
            price = (int)(price * priceMultiplier);

            int? currencyObjectStack = _itemStock.CurrencyObjectId != null
                ? Math.Max(_itemStock.StockCurrencyStack, 1)
                : null;

            return new ItemStockInformation(price, _itemStock.Stock, _itemStock.CurrencyObjectId, currencyObjectStack);
        }
    }
}
