using Microsoft.Xna.Framework;
using ShopTileFramework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Collections.Generic;

namespace ShopTileFramework.ItemPriceAndStock
{
    /// <summary>
    /// This class stores the global data for each itemstock, in order to generate and add items by ID or name
    /// to the stock
    /// </summary>
    class ItemBuilder
    {
        //These are set for each stock
        private readonly string _itemType;
        private readonly bool _isRecipe;
        private readonly int _stockPrice;
        private readonly int _quality;
        private readonly int _currencyItemId;
        private readonly int _currencyItemStack;
        private readonly int _stock;
        private readonly double _defaultSellPriceMultiplier;
        private readonly string _shopName;
        private Dictionary<ISalable, int[]> _itemPriceAndStock;

        /// <summary>
        /// Sets the global data for this item stock
        /// </summary>
        /// <param name="itemType">The string name of the item type</param>
        /// <param name="isRecipe">If items are recipes or not</param>
        /// <param name="price">The price of </param>
        /// <param name="currencyItemId">the object ID of the currency item</param>
        /// <param name="currencyItemStack">How many of the currency item is needed</param>
        /// <param name="stock">how much of the item is available</param>
        /// <param name="quality">The quality of the items</param>
        /// <param name="shopName">Name of the shop, kept for logging purposes</param>
        /// <param name="defaultSellPriceMultiplier"></param>
        public ItemBuilder(string itemType,
                           bool isRecipe,
                           int price,
                           int currencyItemId,
                           int currencyItemStack,
                           int stock,
                           int quality,
                           string shopName,
                           double defaultSellPriceMultiplier) {
            this._itemType = itemType;
            this._isRecipe = isRecipe;
            this._stockPrice = price;
            this._currencyItemId = currencyItemId;
            this._currencyItemStack = currencyItemStack;
            this._stock = stock;
            this._quality = quality;
            this._shopName = shopName;
            this._defaultSellPriceMultiplier = defaultSellPriceMultiplier;
        }

        /// <param name="itemPriceAndStock">the ItemPriceAndStock this builder will add items to</param>
        public void SetItemPriceAndStock(Dictionary<ISalable, int[]> itemPriceAndStock)
        {
            this._itemPriceAndStock = itemPriceAndStock;
        }

        /// <summary>
        /// Takes an item name, and adds that item to the stock
        /// </summary>
        /// <param name="itemName">name of the item</param>
        /// <param name="priceMultiplier"></param>
        /// <returns></returns>
        public bool AddItemToStock(string itemName, double priceMultiplier = 1)
        {
            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Getting ID of {itemName} to add to {_shopName}",LogLevel.Debug);

            int id = ItemsUtil.GetIndexByName(itemName,_itemType);
            if (id < 0)
            {
                ModEntry.monitor.Log($"{_itemType} named \"{itemName}\" could not be added to the Shop {_shopName}", ModEntry.VerboseLogging ? LogLevel.Debug : LogLevel.Trace);
                return false;
            }

            return AddItemToStock(id, priceMultiplier);
        }

        /// <summary>
        /// Takes an item id, and adds that item to the stock
        /// </summary>
        /// <param name="itemId">the id of the item</param>
        /// <param name="priceMultiplier"></param>
        /// <returns></returns>
        public bool AddItemToStock(int itemId, double priceMultiplier = 1)
        {

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"Adding item ID {itemId} to {_shopName}", LogLevel.Debug);

            if (itemId < 0)
            {
                ModEntry.monitor.Log($"{_itemType} of ID {itemId} could not be added to the Shop {_shopName}", ModEntry.VerboseLogging ? LogLevel.Debug : LogLevel.Trace);
                return false;
            }

            var item = CreateItem(itemId);
            if (item == null)
            {
                return false;
            }

            if (_isRecipe)
            {
                if (!ItemsUtil.RecipesList.Contains(item.Name))
                {
                    ModEntry.monitor.Log($"{item.Name} is not a valid recipe and won't be added.", ModEntry.VerboseLogging ? LogLevel.Debug : LogLevel.Trace);
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
        private ISalable CreateItem(int itemId)
        {
            switch (_itemType)
            {
                case "Object":
                    return new Object(itemId, _isRecipe? 1 : _stock, _isRecipe, quality: _quality);
                case "Seed":
                    return new Object(itemId, _isRecipe ? 1 : _stock, _isRecipe, quality: _quality);
                case "BigCraftable":
                    return new Object(Vector2.Zero, itemId) { Stack = _isRecipe ? 1 : _stock, IsRecipe = _isRecipe };
                case "Clothing":
                    return new Clothing(itemId);
                case "Ring":
                    return new Ring(itemId);
                case "Hat":
                    return new Hat(itemId);
                case "Boot":
                    return new Boots(itemId);
                case "Furniture":
                    return new Furniture(itemId, Vector2.Zero);
                case "Weapon":
                    return new MeleeWeapon(itemId);
                default: return null;
            }
        }

        /// <summary>
        /// Creates the second parameter in ItemStockAndPrice, an array that holds info on the price, stock,
        /// and if it exists, the item currency it takes
        /// </summary>
        /// <param name="item">An instance of the item</param>
        /// <param name="priceMultiplier"></param>
        /// <returns>The array that's the second parameter in ItemPriceAndStock</returns>
        private int[] GetPriceStockAndCurrency(ISalable item, double priceMultiplier)
        {
            int[] priceStockCurrency;
            //if no price is provided, use the item's sale price multiplied by defaultSellPriceMultiplier
            var price = (_stockPrice == -1) ? (int)(item.salePrice()*this._defaultSellPriceMultiplier) : _stockPrice;
            price = (int)(price*priceMultiplier);

            if (_currencyItemId == -1) // no currency item
            {
                priceStockCurrency = new[] { price, _stock };
            }
            else if (_currencyItemStack == -1) //no stack provided for currency item so defaults to 1
            {
                priceStockCurrency = new[] { price, _stock, _currencyItemId };
            }
            else //both currency item and stack provided
            {
                priceStockCurrency = new[] { price, _stock, _currencyItemId, _currencyItemStack };
            }

            return priceStockCurrency;
        }
    }
}
